using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace UniVue.CodeGen
{
    internal static class ParamsGCOptimizationInjector
    {
        private const string OptimizationAttributeFullName = "UniVue.Internal.InternalParamsGCOptimizationAttribute";
        private const string ParamsTypeFullNamePrefix = "UniVue.Common.Params`1";

        public static bool Inject(AssemblyDefinition assemblyDefinition, List<DiagnosticMessage> diagnostics)
        {
            if (assemblyDefinition == null) return false;

            bool modified = false;
            foreach (ModuleDefinition module in assemblyDefinition.Modules)
            foreach (TypeDefinition type in module.Types)
                modified |= InjectType(module, type, diagnostics);

            return modified;
        }

        private static bool InjectType(ModuleDefinition module, TypeDefinition type,
                                       List<DiagnosticMessage> diagnostics)
        {
            if (type == null) return false;

            bool modified = false;
            foreach (MethodDefinition method in type.Methods)
                modified |= InjectMethod(module, method, diagnostics);

            foreach (TypeDefinition nestedType in type.NestedTypes)
                modified |= InjectType(module, nestedType, diagnostics);

            return modified;
        }

        private static bool InjectMethod(ModuleDefinition module, MethodDefinition method,
                                         List<DiagnosticMessage> diagnostics)
        {
            if (method == null || !method.HasBody) return false;

            bool modified = false;
            ILProcessor il = method.Body.GetILProcessor();
            IList<Instruction> instructions = method.Body.Instructions;

            for (int i = 0; i < instructions.Count; i++)
            {
                Instruction callInstruction = instructions[i];
                if (!IsCallInstruction(callInstruction, out MethodReference calledMethodRef)) continue;

                MethodDefinition calledMethodDef = SafeResolve(calledMethodRef);
                if (!IsOptimizableParamsMethod(calledMethodDef, out ArrayType paramsArrayType)) continue;

                if (!TryFindReplacementMethod(calledMethodDef, paramsArrayType.ElementType,
                                              out MethodDefinition replacementMethod))
                {
                    diagnostics.Add(CreateWarning(
                                                  $"ParamsGCOptimization skipped call in {method.FullName}: cannot find 'in Params<T>' overload for {calledMethodDef.FullName}."));
                    continue;
                }

                if (!TryGetParamsValueType(replacementMethod, out TypeReference paramsValueType))
                    continue;

                if (!TryParseInlineParamsArray(instructions, i, paramsArrayType,
                                               out InlineArrayParseResult parseResult))
                    continue;

                // 特性约束：仅优化不超过 10 个参数的 params 调用，超过则保持原调用不变。
                if (parseResult.ElementCount > 10) continue;

                bool isValueTypeElement = IsValueType(paramsArrayType.ElementType);
                if (!TryCreateParamsFactoryMethod(module, paramsValueType, paramsArrayType.ElementType,
                                                  isValueTypeElement,
                                                  out MethodReference factoryMethod))
                {
                    diagnostics.Add(CreateWarning(
                                                  $"ParamsGCOptimization skipped call in {method.FullName}: cannot resolve UniVue.Common.Params<T>._ factory method."));
                    continue;
                }

                method.Body.InitLocals = true;
                VariableDefinition paramsLocal = new(module.ImportReference(paramsValueType));
                method.Body.Variables.Add(paramsLocal);

                if (!isValueTypeElement)
                {
                    int missingCount = 10 - parseResult.ElementCount;
                    for (int j = 0; j < missingCount; j++)
                        il.InsertBefore(callInstruction, Instruction.Create(OpCodes.Ldnull));

                    il.InsertBefore(callInstruction, Instruction.Create(OpCodes.Call, factoryMethod));
                }
                else
                {
                    // 结构体 params 走 _<TStruct>(int length, in TStruct v0 ... v9)。
                    VariableDefinition[] valueLocals = new VariableDefinition[parseResult.ElementCount];
                    for (int valueIndex = parseResult.ElementCount - 1; valueIndex >= 0; valueIndex--)
                    {
                        VariableDefinition valueLocal = new(module.ImportReference(paramsArrayType.ElementType));
                        method.Body.Variables.Add(valueLocal);
                        valueLocals[valueIndex] = valueLocal;
                        il.InsertBefore(callInstruction, Instruction.Create(OpCodes.Stloc, valueLocal));
                    }

                    VariableDefinition defaultValueLocal = null;
                    if (parseResult.ElementCount < 10)
                    {
                        defaultValueLocal = new VariableDefinition(module.ImportReference(paramsArrayType.ElementType));
                        method.Body.Variables.Add(defaultValueLocal);
                        il.InsertBefore(callInstruction, Instruction.Create(OpCodes.Ldloca, defaultValueLocal));
                        il.InsertBefore(callInstruction,
                                        Instruction.Create(OpCodes.Initobj,
                                                           module.ImportReference(paramsArrayType.ElementType)));
                    }

                    il.InsertBefore(callInstruction, CreateLoadIntInstruction(parseResult.ElementCount));
                    for (int argIndex = 0; argIndex < 10; argIndex++)
                    {
                        VariableDefinition argLocal = argIndex < parseResult.ElementCount
                            ? valueLocals[argIndex]
                            : defaultValueLocal;
                        il.InsertBefore(callInstruction, Instruction.Create(OpCodes.Ldloca, argLocal));
                    }

                    il.InsertBefore(callInstruction, Instruction.Create(OpCodes.Call, factoryMethod));
                }

                il.InsertBefore(callInstruction, Instruction.Create(OpCodes.Stloc, paramsLocal));
                il.InsertBefore(callInstruction, Instruction.Create(OpCodes.Ldloca, paramsLocal));

                callInstruction.Operand = module.ImportReference(replacementMethod);

                foreach (int scaffoldIndex in parseResult.ScaffoldInstructionIndexes)
                {
                    Instruction scaffold = instructions[scaffoldIndex];
                    scaffold.OpCode = OpCodes.Nop;
                    scaffold.Operand = null;
                }

                modified = true;
            }

            return modified;
        }

        private static bool IsCallInstruction(Instruction instruction, out MethodReference methodReference)
        {
            methodReference = null;
            if (instruction == null) return false;
            if (instruction.OpCode != OpCodes.Call && instruction.OpCode != OpCodes.Callvirt) return false;
            methodReference = instruction.Operand as MethodReference;
            return methodReference != null;
        }

        private static bool IsOptimizableParamsMethod(MethodDefinition methodDefinition, out ArrayType paramsArrayType)
        {
            paramsArrayType = null;
            if (methodDefinition == null) return false;
            if (!HasOptimizationAttribute(methodDefinition)) return false;
            if (!methodDefinition.HasParameters) return false;

            ParameterDefinition lastParameter = methodDefinition.Parameters[methodDefinition.Parameters.Count - 1];
            if (!HasParamArrayAttribute(lastParameter)) return false;
            paramsArrayType = lastParameter.ParameterType as ArrayType;
            return paramsArrayType != null && paramsArrayType.Rank == 1;
        }

        private static bool HasParamArrayAttribute(ParameterDefinition parameterDefinition)
        {
            if (parameterDefinition == null || !parameterDefinition.HasCustomAttributes) return false;
            for (int i = 0; i < parameterDefinition.CustomAttributes.Count; i++)
            {
                CustomAttribute attribute = parameterDefinition.CustomAttributes[i];
                if (attribute?.AttributeType == null) continue;
                if (string.Equals(attribute.AttributeType.FullName, "System.ParamArrayAttribute",
                                  StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool HasOptimizationAttribute(MethodDefinition methodDefinition)
        {
            if (methodDefinition == null || !methodDefinition.HasCustomAttributes) return false;
            for (int i = 0; i < methodDefinition.CustomAttributes.Count; i++)
            {
                CustomAttribute attribute = methodDefinition.CustomAttributes[i];
                if (attribute?.AttributeType == null) continue;
                if (string.Equals(attribute.AttributeType.FullName, OptimizationAttributeFullName,
                                  StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool TryFindReplacementMethod(MethodDefinition sourceMethod, TypeReference elementType,
                                                     out MethodDefinition replacementMethod)
        {
            replacementMethod = null;
            if (sourceMethod == null || sourceMethod.DeclaringType == null || elementType == null) return false;

            foreach (MethodDefinition candidate in sourceMethod.DeclaringType.Methods)
            {
                if (candidate == null || candidate == sourceMethod) continue;
                if (!string.Equals(candidate.Name, sourceMethod.Name, StringComparison.Ordinal)) continue;
                if (candidate.Parameters.Count != sourceMethod.Parameters.Count) continue;
                if (candidate.IsStatic != sourceMethod.IsStatic) continue;

                bool headParametersMatched = true;
                for (int i = 0; i < sourceMethod.Parameters.Count - 1; i++)
                {
                    string left = sourceMethod.Parameters[i].ParameterType.FullName;
                    string right = candidate.Parameters[i].ParameterType.FullName;
                    if (!string.Equals(left, right, StringComparison.Ordinal))
                    {
                        headParametersMatched = false;
                        break;
                    }
                }

                if (!headParametersMatched) continue;

                ParameterDefinition candidateLast = candidate.Parameters[candidate.Parameters.Count - 1];
                if (candidateLast.ParameterType is not ByReferenceType byRefType) continue;
                if (byRefType.ElementType is not GenericInstanceType genericParamsType) continue;
                if (!genericParamsType.FullName.StartsWith(ParamsTypeFullNamePrefix, StringComparison.Ordinal))
                    continue;
                if (genericParamsType.GenericArguments.Count != 1) continue;

                TypeReference replacementElementType = genericParamsType.GenericArguments[0];
                if (!AreSameType(replacementElementType, elementType)) continue;

                replacementMethod = candidate;
                return true;
            }

            return false;
        }

        private static bool TryGetParamsValueType(MethodDefinition replacementMethod, out TypeReference paramsValueType)
        {
            paramsValueType = null;
            if (replacementMethod == null || !replacementMethod.HasParameters) return false;

            ParameterDefinition lastParameter = replacementMethod.Parameters[replacementMethod.Parameters.Count - 1];
            if (lastParameter.ParameterType is not ByReferenceType byRefType) return false;
            paramsValueType = byRefType.ElementType;
            return paramsValueType != null;
        }

        private static bool TryParseInlineParamsArray(IList<Instruction> instructions, int callInstructionIndex,
                                                      ArrayType expectedArrayType, out InlineArrayParseResult result)
        {
            result = default;
            if (instructions == null || callInstructionIndex <= 0 || expectedArrayType == null) return false;

            int newArrayIndex = FindNearestNewArray(instructions, callInstructionIndex, expectedArrayType.ElementType);
            if (newArrayIndex < 0) return false;

            int lengthLoadIndex = FindPreviousNonNopInstructionIndex(instructions, newArrayIndex - 1);
            if (lengthLoadIndex < 0 || !TryReadConstantInt(instructions[lengthLoadIndex], out int length)) return false;
            if (length < 0) return false;

            HashSet<int> scaffoldIndexes = new();
            scaffoldIndexes.Add(lengthLoadIndex);
            scaffoldIndexes.Add(newArrayIndex);

            int cursor = newArrayIndex + 1;
            int parsedElementCount = 0;
            while (parsedElementCount < length)
            {
                cursor = FindNextNonNopInstructionIndex(instructions, cursor, callInstructionIndex);
                if (cursor < 0 || instructions[cursor].OpCode != OpCodes.Dup) return false;
                scaffoldIndexes.Add(cursor);

                int indexLoadInstructionIndex =
                    FindNextNonNopInstructionIndex(instructions, cursor + 1, callInstructionIndex);
                if (indexLoadInstructionIndex < 0 ||
                    !TryReadConstantInt(instructions[indexLoadInstructionIndex], out int elementIndex) ||
                    elementIndex != parsedElementCount)
                    return false;
                scaffoldIndexes.Add(indexLoadInstructionIndex);

                int valueStart =
                    FindNextNonNopInstructionIndex(instructions, indexLoadInstructionIndex + 1, callInstructionIndex);
                if (valueStart < 0) return false;

                int stelemIndex = FindNextStelemInstructionIndex(instructions, valueStart, callInstructionIndex);
                if (stelemIndex < 0 || stelemIndex <= valueStart) return false;
                if (!CanKeepValueInstructionRange(instructions, valueStart, stelemIndex - 1)) return false;

                scaffoldIndexes.Add(stelemIndex);

                parsedElementCount++;
                cursor = stelemIndex + 1;
            }

            int trailingIndex = FindNextNonNopInstructionIndex(instructions, cursor, callInstructionIndex);
            if (trailingIndex >= 0 && trailingIndex < callInstructionIndex) return false;

            result = new InlineArrayParseResult(length, scaffoldIndexes);
            return true;
        }

        private static int FindNearestNewArray(IList<Instruction> instructions, int callInstructionIndex,
                                               TypeReference expectedElementType)
        {
            for (int i = callInstructionIndex - 1; i >= 0; i--)
            {
                Instruction instruction = instructions[i];
                if (instruction.OpCode == OpCodes.Nop) continue;
                if (instruction.OpCode != OpCodes.Newarr) continue;
                if (instruction.Operand is not TypeReference elementType) continue;
                if (!AreSameType(elementType, expectedElementType)) continue;
                return i;
            }

            return -1;
        }

        private static int FindPreviousNonNopInstructionIndex(IList<Instruction> instructions, int startIndex)
        {
            for (int i = startIndex; i >= 0; i--)
            {
                if (instructions[i].OpCode != OpCodes.Nop)
                    return i;
            }

            return -1;
        }

        private static int FindNextNonNopInstructionIndex(IList<Instruction> instructions, int startIndex,
                                                          int endExclusive)
        {
            for (int i = startIndex; i < endExclusive; i++)
            {
                if (instructions[i].OpCode != OpCodes.Nop)
                    return i;
            }

            return -1;
        }

        private static int FindNextStelemInstructionIndex(IList<Instruction> instructions, int startIndex,
                                                          int endExclusive)
        {
            for (int i = startIndex; i < endExclusive; i++)
            {
                if (IsStelemInstruction(instructions[i]))
                    return i;
            }

            return -1;
        }

        private static bool IsStelemInstruction(Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Stelem_Any ||
                   instruction.OpCode == OpCodes.Stelem_I ||
                   instruction.OpCode == OpCodes.Stelem_I1 ||
                   instruction.OpCode == OpCodes.Stelem_I2 ||
                   instruction.OpCode == OpCodes.Stelem_I4 ||
                   instruction.OpCode == OpCodes.Stelem_I8 ||
                   instruction.OpCode == OpCodes.Stelem_R4 ||
                   instruction.OpCode == OpCodes.Stelem_R8 ||
                   instruction.OpCode == OpCodes.Stelem_Ref;
        }

        private static bool CanKeepValueInstructionRange(IList<Instruction> instructions, int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                FlowControl flowControl = instructions[i].OpCode.FlowControl;
                if (flowControl == FlowControl.Branch ||
                    flowControl == FlowControl.Cond_Branch ||
                    flowControl == FlowControl.Return ||
                    flowControl == FlowControl.Throw ||
                    flowControl == FlowControl.Break)
                    return false;
            }

            return true;
        }

        private static bool TryCreateParamsFactoryMethod(ModuleDefinition module, TypeReference paramsValueType,
                                                         TypeReference elementType, bool isValueTypeElement,
                                                         out MethodReference factoryMethod)
        {
            factoryMethod = null;
            TypeDefinition paramsTypeDefinition = SafeResolve(paramsValueType);
            if (paramsTypeDefinition == null) return false;

            MethodDefinition factoryMethodDefinition = null;
            foreach (MethodDefinition method in paramsTypeDefinition.Methods)
            {
                if (method == null) continue;
                if (!method.IsStatic) continue;
                if (!string.Equals(method.Name, "_", StringComparison.Ordinal)) continue;
                if (!method.HasGenericParameters || method.GenericParameters.Count != 1) continue;
                if (!isValueTypeElement && method.Parameters.Count != 10) continue;
                if (isValueTypeElement && method.Parameters.Count != 11) continue;

                GenericParameter genericParameter = method.GenericParameters[0];
                if (!isValueTypeElement && !genericParameter.HasReferenceTypeConstraint) continue;
                if (isValueTypeElement && !genericParameter.HasNotNullableValueTypeConstraint) continue;

                if (isValueTypeElement)
                {
                    if (method.Parameters[0].ParameterType.MetadataType != MetadataType.Int32) continue;
                    bool byRefParamsMatched = true;
                    for (int i = 1; i < method.Parameters.Count; i++)
                    {
                        if (method.Parameters[i].ParameterType is not ByReferenceType byRefType ||
                            !AreSameType(byRefType.ElementType, genericParameter))
                        {
                            byRefParamsMatched = false;
                            break;
                        }
                    }

                    if (!byRefParamsMatched) continue;
                }

                factoryMethodDefinition = method;
                break;
            }

            if (factoryMethodDefinition == null) return false;

            MethodReference hostMethod =
                MakeHostInstanceGeneric(factoryMethodDefinition, module.ImportReference(elementType));
            MethodReference importedHostMethod = module.ImportReference(hostMethod);
            GenericInstanceMethod genericFactoryMethod = new(importedHostMethod);
            genericFactoryMethod.GenericArguments.Add(module.ImportReference(elementType));
            factoryMethod = genericFactoryMethod;
            return true;
        }

        private static MethodReference MakeHostInstanceGeneric(MethodReference methodReference,
                                                               params TypeReference[] declaringTypeArguments)
        {
            GenericInstanceType declaringTypeInstance = new(methodReference.DeclaringType);
            for (int i = 0; i < declaringTypeArguments.Length; i++)
                declaringTypeInstance.GenericArguments.Add(declaringTypeArguments[i]);

            MethodReference hostMethod = new(methodReference.Name, methodReference.ReturnType, declaringTypeInstance)
            {
                HasThis = methodReference.HasThis,
                ExplicitThis = methodReference.ExplicitThis,
                CallingConvention = methodReference.CallingConvention
            };

            foreach (ParameterDefinition parameter in methodReference.Parameters)
                hostMethod.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (GenericParameter genericParameter in methodReference.GenericParameters)
                hostMethod.GenericParameters.Add(new GenericParameter(genericParameter.Name, hostMethod));

            return hostMethod;
        }

        private static Instruction CreateLoadIntInstruction(int value)
        {
            return value switch
            {
                -1 => Instruction.Create(OpCodes.Ldc_I4_M1),
                0 => Instruction.Create(OpCodes.Ldc_I4_0),
                1 => Instruction.Create(OpCodes.Ldc_I4_1),
                2 => Instruction.Create(OpCodes.Ldc_I4_2),
                3 => Instruction.Create(OpCodes.Ldc_I4_3),
                4 => Instruction.Create(OpCodes.Ldc_I4_4),
                5 => Instruction.Create(OpCodes.Ldc_I4_5),
                6 => Instruction.Create(OpCodes.Ldc_I4_6),
                7 => Instruction.Create(OpCodes.Ldc_I4_7),
                8 => Instruction.Create(OpCodes.Ldc_I4_8),
                <= sbyte.MaxValue and >= sbyte.MinValue => Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)value),
                _ => Instruction.Create(OpCodes.Ldc_I4, value)
            };
        }

        private static bool TryReadConstantInt(Instruction instruction, out int value)
        {
            value = 0;
            if (instruction == null) return false;

            switch (instruction.OpCode.Code)
            {
                case Code.Ldc_I4_M1:
                    value = -1;
                    return true;
                case Code.Ldc_I4_0:
                    value = 0;
                    return true;
                case Code.Ldc_I4_1:
                    value = 1;
                    return true;
                case Code.Ldc_I4_2:
                    value = 2;
                    return true;
                case Code.Ldc_I4_3:
                    value = 3;
                    return true;
                case Code.Ldc_I4_4:
                    value = 4;
                    return true;
                case Code.Ldc_I4_5:
                    value = 5;
                    return true;
                case Code.Ldc_I4_6:
                    value = 6;
                    return true;
                case Code.Ldc_I4_7:
                    value = 7;
                    return true;
                case Code.Ldc_I4_8:
                    value = 8;
                    return true;
                case Code.Ldc_I4_S:
                    value = (sbyte)instruction.Operand;
                    return true;
                case Code.Ldc_I4:
                    value = (int)instruction.Operand;
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsReferenceType(TypeReference typeReference)
        {
            if (typeReference == null) return false;
            if (typeReference.IsGenericParameter) return false;
            if (typeReference.IsByReference || typeReference.IsPointer) return false;

            TypeDefinition resolved = SafeResolve(typeReference);
            return resolved == null || !resolved.IsValueType;
        }

        private static bool IsValueType(TypeReference typeReference)
        {
            if (typeReference == null) return false;
            if (typeReference.IsByReference || typeReference.IsPointer) return false;
            if (typeReference.IsGenericParameter) return false;
            TypeDefinition resolved = SafeResolve(typeReference);
            return resolved != null && resolved.IsValueType;
        }

        private static bool AreSameType(TypeReference left, TypeReference right)
        {
            if (left == null || right == null) return false;
            return string.Equals(left.FullName, right.FullName, StringComparison.Ordinal);
        }

        private static TypeDefinition SafeResolve(TypeReference typeReference)
        {
            if (typeReference == null) return null;
            try
            {
                return typeReference.Resolve();
            }
            catch
            {
                return null;
            }
        }

        private static MethodDefinition SafeResolve(MethodReference methodReference)
        {
            if (methodReference == null) return null;
            try
            {
                return methodReference.Resolve();
            }
            catch
            {
                return null;
            }
        }

        private static DiagnosticMessage CreateWarning(string message)
        {
            return new DiagnosticMessage
            {
                DiagnosticType = DiagnosticType.Warning,
                MessageData = message
            };
        }

        private readonly struct InlineArrayParseResult
        {
            public InlineArrayParseResult(int elementCount, HashSet<int> scaffoldInstructionIndexes)
            {
                ElementCount = elementCount;
                ScaffoldInstructionIndexes = scaffoldInstructionIndexes;
            }

            public int ElementCount { get; }
            public HashSet<int> ScaffoldInstructionIndexes { get; }
        }
    }
}