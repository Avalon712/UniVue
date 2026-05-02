using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace UniVue.CodeGen
{
    internal static class NotifyPropertyChangedInjector
    {
        private const string BaseModelFullName = "UniVue.Model.BaseModel";

        public static bool Inject(AssemblyDefinition assemblyDefinition)
        {
            bool modified = false;
            if (assemblyDefinition == null) return false;

            foreach (ModuleDefinition module in assemblyDefinition.Modules)
            foreach (TypeDefinition type in module.Types)
                modified |= InjectType(module, type);

            return modified;
        }

        private static bool InjectType(ModuleDefinition module, TypeDefinition type)
        {
            bool modified = false;

            if (type == null) return false;

            if (IsSubclassOfBaseModel(type))
            {
                foreach (PropertyDefinition property in type.Properties)
                    modified |= InjectPropertySetter(module, type, property);
            }

            foreach (TypeDefinition nestedType in type.NestedTypes) modified |= InjectType(module, nestedType);

            return modified;
        }

        private static bool InjectPropertySetter(ModuleDefinition module, TypeDefinition ownerType,
                                                 PropertyDefinition property)
        {
            MethodDefinition getter = property.GetMethod;
            MethodDefinition setter = property.SetMethod;

            if (getter == null || setter == null) return false;

            if (!getter.HasBody || !setter.HasBody || setter.IsStatic || setter.IsAbstract) return false;

            if (HasCheckPropertyChangedCall(setter)) return false;

            ILProcessor il = setter.Body.GetILProcessor();
            VariableDefinition oldValueVar = new(module.ImportReference(property.PropertyType));
            setter.Body.Variables.Add(oldValueVar);
            setter.Body.InitLocals = true;

            Instruction first = setter.Body.Instructions[0];
            List<Instruction> captureOldValueInstructions = new();

            captureOldValueInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            for (int i = 0; i < getter.Parameters.Count; i++)
                captureOldValueInstructions.Add(CreateLoadArgInstruction(i + 1));

            OpCode callGetterOpCode = getter.IsVirtual && !getter.IsFinal ? OpCodes.Callvirt : OpCodes.Call;
            captureOldValueInstructions.Add(Instruction.Create(callGetterOpCode, module.ImportReference(getter)));
            captureOldValueInstructions.Add(Instruction.Create(OpCodes.Stloc, oldValueVar));

            for (int i = 0; i < captureOldValueInstructions.Count; i++)
                il.InsertBefore(first, captureOldValueInstructions[i]);

            MethodReference checkMethodRef =
                CreateCheckPropertyChangedMethodReference(module, ownerType, property.PropertyType);
            List<Instruction> returnInstructions = new();

            foreach (Instruction instruction in setter.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ret)
                    returnInstructions.Add(instruction);
            }

            int valueArgIndex = setter.Parameters.Count;
            foreach (Instruction ret in returnInstructions)
            {
                il.InsertBefore(ret, Instruction.Create(OpCodes.Ldarg_0));
                il.InsertBefore(ret, Instruction.Create(OpCodes.Ldstr, property.Name));
                il.InsertBefore(ret, Instruction.Create(OpCodes.Ldloc, oldValueVar));
                il.InsertBefore(ret, CreateLoadArgInstruction(valueArgIndex));
                il.InsertBefore(ret, Instruction.Create(OpCodes.Call, checkMethodRef));
            }

            return true;
        }

        private static MethodReference CreateCheckPropertyChangedMethodReference(
            ModuleDefinition module, TypeDefinition ownerType, TypeReference propertyType)
        {
            MethodDefinition baseMethodDefinition = FindBaseModelCheckMethod(ownerType);
            MethodReference importedMethod = module.ImportReference(baseMethodDefinition);

            GenericInstanceMethod genericInstance = new(importedMethod);
            genericInstance.GenericArguments.Add(module.ImportReference(propertyType));
            return genericInstance;
        }

        private static MethodDefinition FindBaseModelCheckMethod(TypeDefinition ownerType)
        {
            TypeReference current = ownerType.BaseType;
            while (current != null)
            {
                TypeDefinition resolved = current.Resolve();
                if (resolved == null) break;

                if (resolved.FullName == BaseModelFullName)
                {
                    foreach (MethodDefinition method in resolved.Methods)
                    {
                        if (method.Name == "CheckPropertyChanged" && method.HasGenericParameters &&
                            method.Parameters.Count == 3)
                            return method;
                    }
                }

                current = resolved.BaseType;
            }

            throw new InvalidOperationException("Failed to locate BaseModel.CheckPropertyChanged<T>.");
        }

        private static bool HasCheckPropertyChangedCall(MethodDefinition method)
        {
            foreach (Instruction instruction in method.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Call && instruction.OpCode != OpCodes.Callvirt) continue;

                if (instruction.Operand is not MethodReference calledMethod) continue;

                if (calledMethod.Name == "CheckPropertyChanged") return true;
            }

            return false;
        }

        private static bool IsSubclassOfBaseModel(TypeDefinition type)
        {
            TypeReference current = type.BaseType;
            while (current != null)
            {
                if (current.FullName == BaseModelFullName) return true;

                try
                {
                    TypeDefinition resolved = current.Resolve();
                    if (resolved == null) return false;

                    current = resolved.BaseType;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private static Instruction CreateLoadArgInstruction(int argumentIndex)
        {
            return argumentIndex switch
            {
                0 => Instruction.Create(OpCodes.Ldarg_0),
                1 => Instruction.Create(OpCodes.Ldarg_1),
                2 => Instruction.Create(OpCodes.Ldarg_2),
                3 => Instruction.Create(OpCodes.Ldarg_3),
                _ => Instruction.Create(OpCodes.Ldarg_S, (byte)argumentIndex)
            };
        }
    }
}