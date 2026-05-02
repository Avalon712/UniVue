using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace UniVue.CodeGen
{
    /// <summary>
    /// 为带 <c>LazyInitUIAttribute</c> 的只读自动属性生成懒加载 <c>get</c> 实现；仅当声明类型派生自 <c>BaseUI</c>。
    /// 路径为 <c>UI.name + attribute.Path</c> 后交给 <c>BaseUI.FindByPath</c> 与 <c>GameObject.GetComponent(Type)</c>。
    /// </summary>
    internal static class UILazyInitUIInjector
    {
        private const string BaseUIFullName = "UniVue.UI.BaseUI";
        private const string LazyInitUIAttributeFullName = "UniVue.UI.LazyInitUIAttribute";
        private const string GameObjectTypeFullName = "UnityEngine.GameObject";
        private const string ComponentTypeFullName = "UnityEngine.Component";
        private const string ObjectTypeFullName = "UnityEngine.Object";
        private const string StringTypeFullName = "System.String";
        private const string SystemTypeTypeFullName = "System.Type";

        public static bool Inject(AssemblyDefinition assemblyDefinition, List<DiagnosticMessage> diagnostics)
        {
            if (assemblyDefinition == null) return false;

            bool modified = false;
            foreach (ModuleDefinition module in assemblyDefinition.Modules)
            foreach (TypeDefinition type in GetTopLevelAndNestedTypes(module))
                modified |= TryInjectType(module, type, diagnostics);

            return modified;
        }

        private static IEnumerable<TypeDefinition> GetTopLevelAndNestedTypes(ModuleDefinition module)
        {
            foreach (TypeDefinition t in module.Types)
            {
                yield return t;
                foreach (TypeDefinition n in GetNestedRecursively(t))
                    yield return n;
            }
        }

        private static IEnumerable<TypeDefinition> GetNestedRecursively(TypeDefinition type)
        {
            if (!type.HasNestedTypes) yield break;
            foreach (TypeDefinition nested in type.NestedTypes)
            {
                yield return nested;
                foreach (TypeDefinition d in GetNestedRecursively(nested))
                    yield return d;
            }
        }

        private static bool TryInjectType(ModuleDefinition module, TypeDefinition type,
                                          List<DiagnosticMessage> diagnostics)
        {
            if (type == null) return false;
            if (type.IsInterface) return false;

            bool modified = false;
            if (IsTypeOrDerivedFrom(type, BaseUIFullName))
            {
                foreach (PropertyDefinition property in type.Properties)
                    modified |= TryInjectLazyProperty(module, type, property, diagnostics);
            }

            return modified;
        }

        private static bool TryInjectLazyProperty(ModuleDefinition module, TypeDefinition type,
                                                  PropertyDefinition property, List<DiagnosticMessage> diagnostics)
        {
            if (property == null || property.GetMethod == null) return false;
            if (!property.GetMethod.HasBody) return false;
            if (property.GetMethod.IsStatic) return false;

            if (!TryGetLazyInitPathFromAttribute(property, out string pathSuffix) ||
                string.IsNullOrEmpty(pathSuffix)) return false;

            if (property.PropertyType.IsValueType) return false;

            FieldDefinition fieldDef = FindAutoPropertyBackingField(type, property);
            if (fieldDef == null)
            {
                diagnostics.Add(CreateWarning(
                                              $"{type.FullName}.{property.Name}: 未找到自动属性后援字段，LazyInitUI 已跳过。"));
                return false;
            }

            // get-only 自动属性的后援字段为 initonly，在 getter 外赋值会 invalid；懒加载需写入该字段，故取消只读
            fieldDef.IsInitOnly = false;

            TypeReference returnTypeRef = module.ImportReference(property.PropertyType);
            FieldReference fieldRef = module.ImportReference(fieldDef);

            if (!TryResolveLazyInitReferences(module, out LazyInitReferences refs, diagnostics)) return false;

            try
            {
                MethodDefinition getter = property.GetMethod;
                EmitGetterBody(module, type, getter, fieldRef, returnTypeRef, pathSuffix, refs);
            }
            catch (Exception e)
            {
                diagnostics.Add(CreateWarning(
                                              $"{type.FullName}.{property.Name}: LazyInitUI 注入失败: {e.Message}"));
                return false;
            }

            return true;
        }

        private static void EmitGetterBody(
            ModuleDefinition module,
            TypeDefinition type,
            MethodDefinition getter,
            FieldReference fieldRef,
            TypeReference returnTypeRef,
            string pathSuffix,
            LazyInitReferences refs)
        {
            MethodBody body = getter.Body;
            body.Instructions.Clear();
            body.Variables.Clear();
            body.ExceptionHandlers.Clear();
            body.InitLocals = true;

            VariableDefinition vPath = new(module.TypeSystem.String);
            body.Variables.Add(new VariableDefinition(returnTypeRef));
            body.Variables.Add(new VariableDefinition(refs.GameObjectType));
            body.Variables.Add(vPath);

            ILProcessor il = body.GetILProcessor();

            MethodReference getUi = module.ImportReference(refs.get_UI);
            MethodReference getName = module.ImportReference(refs.get_name);
            MethodReference concat2 = module.ImportReference(refs.string_Concat2);
            MethodReference findByPath = module.ImportReference(refs.findByPath);
            MethodReference getTypeFromHandle = module.ImportReference(refs.get_TypeFromHandle);
            MethodReference getComponent = module.ImportReference(refs.getComponent_Type);

            Instruction labelCacheHit = il.Create(OpCodes.Nop);
            Instruction labelAfterFind = il.Create(OpCodes.Nop);

            // fast path: if (_field != null) return _field; — 仅用 ldfld + brtrue，避免 dup 导致入分支时栈上多留 1 个值（stack height 0 vs 1）
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, fieldRef));
            il.Append(il.Create(OpCodes.Brtrue_S, labelCacheHit));

            // fullPath = string.Concat(this.UI.get_name, pathSuffix);
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(ChooseCallKind(getUi), getUi));
            il.Append(Instruction.Create(ChooseCallKind(getName), getName));
            il.Append(il.Create(OpCodes.Ldstr, pathSuffix));
            il.Append(il.Create(OpCodes.Call, concat2));
            il.Append(il.Create(OpCodes.Stloc, vPath));

            // go = this.FindByPath(fullPath);
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldloc, vPath));
            il.Append(Instruction.Create(ChooseCallKind(findByPath), findByPath));

            // stloc.1  GameObject? — index 1
            il.Append(il.Create(OpCodes.Stloc_1));
            il.Append(il.Create(OpCodes.Ldloc_1));
            il.Append(il.Create(OpCodes.Brtrue_S, labelAfterFind));

            // 未找到 GameObject: field = null; return null
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldnull));
            il.Append(il.Create(OpCodes.Stfld, fieldRef));
            il.Append(il.Create(OpCodes.Ldnull));
            il.Append(il.Create(OpCodes.Ret));

            il.Append(labelAfterFind);

            // go.GetComponent(typeof(T))
            il.Append(il.Create(OpCodes.Ldloc_1));
            il.Append(il.Create(OpCodes.Ldtoken, returnTypeRef));
            il.Append(il.Create(OpCodes.Call, getTypeFromHandle));
            il.Append(il.Create(OpCodes.Callvirt, getComponent));
            if (!returnTypeRef.IsValueType)
                il.Append(il.Create(OpCodes.Castclass, returnTypeRef));

            // stloc.0 = component
            il.Append(il.Create(OpCodes.Stloc_0));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldloc_0));
            il.Append(il.Create(OpCodes.Stfld, fieldRef));

            il.Append(labelCacheHit);
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, fieldRef));
            il.Append(il.Create(OpCodes.Ret));

            body.Optimize();
        }

        /// <summary>非虚 / 未重写实例方法用 <see cref="OpCodes.Call" />，否则 <see cref="OpCodes.Callvirt" />。</summary>
        private static OpCode ChooseCallKind(MethodReference methodRef)
        {
            if (methodRef == null) return OpCodes.Call;
            MethodDefinition md = methodRef.Resolve();
            if (md == null) return OpCodes.Call;
            if (md.IsStatic) return OpCodes.Call;
            if (md.IsVirtual) return OpCodes.Callvirt;
            return OpCodes.Call;
        }

        private static bool TryGetLazyInitPathFromAttribute(PropertyDefinition property, out string path)
        {
            path = null;
            if (!property.HasCustomAttributes) return false;
            foreach (CustomAttribute ca in property.CustomAttributes)
            {
                if (ca == null || ca.AttributeType == null) continue;
                if (!string.Equals(ca.AttributeType.FullName, LazyInitUIAttributeFullName, StringComparison.Ordinal))
                    continue;
                if (ca.Constructor == null || ca.ConstructorArguments.Count < 1) continue;
                if (ca.ConstructorArguments[0].Type.FullName == StringTypeFullName)
                {
                    path = ca.ConstructorArguments[0].Value as string;
                    return !string.IsNullOrEmpty(path);
                }
            }

            return false;
        }

        private static FieldDefinition FindAutoPropertyBackingField(TypeDefinition type, PropertyDefinition property)
        {
            string expected = "<" + property.Name + ">k__BackingField";
            foreach (FieldDefinition f in type.Fields)
            {
                if (f.Name == expected)
                    return f;
            }

            if (property.GetMethod != null && property.GetMethod.HasBody &&
                property.GetMethod.Body.Instructions != null)
            {
                foreach (Instruction i in property.GetMethod.Body.Instructions)
                {
                    if (i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference fr &&
                        string.Equals(type.FullName, fr.DeclaringType.FullName, StringComparison.Ordinal) &&
                        fr.Name.Contains("k__BackingField", StringComparison.Ordinal))
                    {
                        return fr.Resolve();
                    }
                }
            }

            return null;
        }

        private static bool TryResolveLazyInitReferences(
            ModuleDefinition module,
            out LazyInitReferences refs,
            List<DiagnosticMessage> diagnostics)
        {
            refs = new LazyInitReferences();
            TypeDefinition baseUIt = GetTypeByFullName(module, BaseUIFullName);
            if (baseUIt == null)
            {
                diagnostics.Add(CreateWarning("LazyInitUI: 未解析到 UniVue.UI.BaseUI，已跳过。"));
                return false;
            }

            MethodDefinition getUiM =
                baseUIt.Methods.FirstOrDefault(m => m.Name == "get_UI" && m.Parameters.Count == 0);
            MethodDefinition findPath = baseUIt.Methods.FirstOrDefault(m =>
                                                                           m.Name == "FindByPath" &&
                                                                           m.Parameters.Count == 1 &&
                                                                           m.Parameters[0].ParameterType.FullName ==
                                                                           StringTypeFullName);
            if (getUiM == null || findPath == null)
            {
                diagnostics.Add(CreateWarning("LazyInitUI: BaseUI 上缺少 get_UI 或 FindByPath(string)，已跳过。"));
                return false;
            }

            TypeDefinition goT = GetTypeByFullName(module, GameObjectTypeFullName);
            TypeDefinition objT = GetTypeByFullName(module, ObjectTypeFullName);
            TypeDefinition compT = GetTypeByFullName(module, ComponentTypeFullName);
            TypeDefinition stringT = GetTypeByFullName(module, StringTypeFullName);
            TypeDefinition typeT = GetTypeByFullName(module, SystemTypeTypeFullName);
            if (goT is null || objT is null || stringT is null || typeT is null)
            {
                diagnostics.Add(CreateWarning("LazyInitUI: 未解析到 Unity 或 mscorlib 中的类型，已跳过。"));
                return false;
            }

            MethodDefinition getNameM =
                objT.Methods.FirstOrDefault(m => m.Name == "get_name" && m.Parameters.Count == 0);
            MethodDefinition concatM = stringT.Methods.FirstOrDefault(m =>
                                                                          m.Name == "Concat" && m.IsStatic &&
                                                                          m.Parameters.Count == 2 &&
                                                                          m.Parameters[0].ParameterType.FullName ==
                                                                          StringTypeFullName &&
                                                                          m.Parameters[1].ParameterType.FullName ==
                                                                          StringTypeFullName);
            MethodDefinition gtfhM = typeT.Methods.FirstOrDefault(m =>
                                                                      m.Name == "GetTypeFromHandle" && m.IsStatic &&
                                                                      m.Parameters.Count == 1);
            // Unity: GameObject.GetComponent(Type)；部分版本在 Component 上
            MethodDefinition getCompM = goT != null
                ? goT.Methods.FirstOrDefault(m =>
                                                 m.Name == "GetComponent" && m.HasThis && m.Parameters.Count == 1 &&
                                                 m.Parameters[0].ParameterType.FullName == SystemTypeTypeFullName)
                : null;
            if (getCompM == null && compT != null)
            {
                getCompM = compT.Methods.FirstOrDefault(m =>
                                                            m.Name == "GetComponent" && m.HasThis &&
                                                            m.Parameters.Count == 1 &&
                                                            m.Parameters[0].ParameterType.FullName ==
                                                            SystemTypeTypeFullName);
            }

            if (getNameM == null || concatM == null || gtfhM == null || getCompM == null)
            {
                diagnostics.Add(CreateWarning("LazyInitUI: 未解析到 get_name/Concat/GetTypeFromHandle/GetComponent(Type)，已跳过。"));
                return false;
            }

            refs.get_UI = module.ImportReference(getUiM);
            refs.findByPath = module.ImportReference(findPath);
            refs.get_name = module.ImportReference(getNameM);
            refs.GameObjectType = module.ImportReference(goT);
            refs.string_Concat2 = module.ImportReference(concatM);
            refs.get_TypeFromHandle = module.ImportReference(gtfhM);
            refs.getComponent_Type = module.ImportReference(getCompM);
            return true;
        }

        private static TypeDefinition GetTypeByFullName(ModuleDefinition module, string fullName)
        {
            TypeDefinition inModule = GetAllModuleTypes(module).FirstOrDefault(t => t.FullName == fullName);
            if (inModule != null) return inModule;
            IAssemblyResolver resolver = module.AssemblyResolver;
            if (module.AssemblyReferences is not null)
            {
                foreach (AssemblyNameReference anr in module.AssemblyReferences)
                {
                    try
                    {
                        AssemblyDefinition asm = resolver.Resolve(anr);
                        inModule = GetAllModuleTypes(asm.MainModule)
                           .FirstOrDefault(t => t.FullName == fullName);
                        if (inModule != null) return inModule;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return null;
        }

        private static IEnumerable<TypeDefinition> GetAllModuleTypes(ModuleDefinition m)
        {
            foreach (TypeDefinition t in m.Types)
            {
                yield return t;
                foreach (TypeDefinition d in GetNestedRecursively(t))
                    yield return d;
            }
        }

        private static bool IsTypeOrDerivedFrom(TypeReference type, string baseTypeFullName)
        {
            TypeReference c = type;
            while (c != null)
            {
                if (string.Equals(c.FullName, baseTypeFullName, StringComparison.Ordinal)) return true;
                TypeDefinition resolved = SafeResolveType(c);
                c = resolved != null ? resolved.BaseType : null;
            }

            return false;
        }

        private static TypeDefinition SafeResolveType(TypeReference tr)
        {
            if (tr is null) return null;
            try
            {
                return tr.Resolve();
            }
            catch
            {
                return null;
            }
        }

        private static DiagnosticMessage CreateWarning(string message)
        {
            return new DiagnosticMessage { DiagnosticType = DiagnosticType.Warning, MessageData = message };
        }

        private sealed class LazyInitReferences
        {
            public MethodReference findByPath;
            public TypeReference GameObjectType;
            public MethodReference get_name; // on UnityEngine.Object
            public MethodReference get_TypeFromHandle;
            public MethodReference get_UI;
            public MethodReference getComponent_Type;
            public MethodReference string_Concat2;
        }
    }
}