/*
MIT License

Copyright (c) 2018 P Collyer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Proxy
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.SymbolStore;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using global::Proxy.Reflection;
    using global::Proxy.Reflection.Emit;

    /// <summary>
    /// A factory for building proxy types.
    /// </summary>
    internal class ProxyTypeGenerator
        : IProxyTypeGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyTypeGenerator"/> class.
        /// </summary>
        public ProxyTypeGenerator()
        {
        }

        /// <summary>
        /// Gets the name of a proxy type.
        /// </summary>
        /// <param name="proxyType">The proxy type.</param>
        /// <param name="proxiedType">The type being proxied.</param>
        /// <returns>The type name.</returns>
        public static string TypeName(Type proxyType, Type proxiedType)
        {
            return string.Format("Dynamic.Proxies.{0}_{1}", proxyType.Name, proxiedType.Name);
        }

        /// <summary>
        /// Gets or creates the <see cref="Type"/> that represents the proxy type.
        /// </summary>
        /// <typeparam name="T">The type of proxy required.</typeparam>
        /// <param name="baseType">The base <see cref="Type"/> to proxy.</param>
        /// <param name="scope">The current dependency injection scope.</param>
        /// <returns>A <see cref="Type"/> representing the proxy type.</returns>
        public Type GetOrCreateProxyType<T>(Type baseType, IServiceProvider scope = null)
        {
            Type proxyType = TypeFactory
                .Default
                .GetType(
                    TypeName(typeof(T), baseType),
                    true);

            if (proxyType == null)
            {
                proxyType = this.GenerateProxyType(typeof(T), baseType, scope);
            }

            return proxyType;
        }

        /// <summary>
        /// Generate the proxy instance.
        /// </summary>
        /// <typeparam name="T">The type of proxy to create.</typeparam>
        /// <param name="implementation">The proxy implementation.</param>
        /// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
        /// <returns>An instance of the proxy type.</returns>
        public T CreateProxy<T>(IProxy implementation, IServiceProvider serviceProvider = null)
        {
            Utility.ThrowIfArgumentNull(implementation, nameof(implementation));

            Type proxyType = this.GetOrCreateProxyType<T>(typeof(IProxy));
            return (T)Activator.CreateInstance(proxyType, implementation, serviceProvider);
        }

        /// <summary>
        /// Generate the proxy type.
        /// </summary>
        /// <param name="proxyType">The interface the proxy type must implement.</param>
        /// <param name="proxiedType">The type being proxied.</param>
        /// <param name="serviceProvider">The dependency injection scope.</param>
        /// <returns>A <see cref="Type"/> representing the proxy type.</returns>
        private Type GenerateProxyType(
            Type proxyType,
            Type proxiedType,
            IServiceProvider serviceProvider)
        {
            if (proxyType.IsInterface == false)
            {
                throw new ArgumentException("Argument is not an interface", "proxyType");
            }

            TypeBuilder typeBuilder = TypeFactory
                .Default
                .ModuleBuilder
                .DefineType(
                    TypeName(proxyType, proxiedType),
                    TypeAttributes.Class | TypeAttributes.Public);

            typeBuilder.AddInterfaceImplementation(proxyType);

            FieldBuilder targetField = typeBuilder
                .DefineField(
                    "target",
                    proxiedType,
                    FieldAttributes.Private);

            FieldBuilder dependencyResolverField = typeBuilder
                .DefineField(
                    "serviceProvider",
                    typeof(IServiceProvider),
                    FieldAttributes.Private);

            TypeFactoryContext context = new TypeFactoryContext(
                typeBuilder,
                proxyType,
                proxiedType,
                serviceProvider,
                targetField,
                dependencyResolverField);

            this.ImplementInterface(context);

            this.EmitConstructor(context);

            this.EmitIProxyMetadataInterface(context);

            this.EmitIProxiedObjectInterface(context);

            return context
                .TypeBuilder
                .CreateTypeInfo()
                .AsType();
        }

        /// <summary>
        /// Implements the interface for the proxy type.
        /// </summary>
        /// <param name="context">The current factory context.</param>
        private void ImplementInterface(TypeFactoryContext context)
        {
            Dictionary<string, MethodBuilder> propertyMethods = new Dictionary<string, MethodBuilder>();

            foreach (var memberInfo in context.NewType.GetMembers())
            {
                if (memberInfo.MemberType == MemberTypes.Method)
                {
                    MethodInfo methodInfo = (MethodInfo)memberInfo;
                    Type[] methodArgs = methodInfo
                        .GetParameters()
                        .Select(p => p.ParameterType)
                        .ToArray();

                    Type[] genericArguments = null;
                    MethodBuilder methodBuilder = context
                        .TypeBuilder
                        .DefineMethod(
                            methodInfo.Name,
                            MethodAttributes.Public | MethodAttributes.Virtual,
                            methodInfo.ReturnType,
                            methodArgs);

                    if (methodInfo.ContainsGenericParameters == true)
                    {
                        genericArguments = methodInfo.GetGenericArguments();
                        GenericTypeParameterBuilder[] genericTypeParameterBuilder = methodBuilder
                            .DefineGenericParameters(genericArguments.Select(t => t.Name).ToArray());
                        for (int m = 0; m < genericTypeParameterBuilder.Length; m++)
                        {
                            genericTypeParameterBuilder[m].SetGenericParameterAttributes(genericArguments[m].GenericParameterAttributes);
                        }
                    }

                    ILGenerator methodIL = methodBuilder.GetILGenerator();

                    this.EmitCallToProxyImplementation(context, methodIL, methodInfo, methodArgs);

                    if (methodInfo.IsProperty() == true)
                    {
                        propertyMethods.Add(methodInfo.Name, methodBuilder);
                    }
                }
                else if (memberInfo.MemberType == MemberTypes.Property)
                {
                    PropertyBuilder propertyBuilder = context
                        .TypeBuilder
                        .DefineProperty(
                            memberInfo.Name,
                            PropertyAttributes.SpecialName,
                            ((PropertyInfo)memberInfo).PropertyType,
                            null);

                    MethodBuilder getMethod;
                    if (propertyMethods.TryGetValue(memberInfo.PropertyGetName(), out getMethod) == true)
                    {
                        propertyBuilder.SetGetMethod(getMethod);
                    }

                    MethodBuilder setMethod;
                    if (propertyMethods.TryGetValue(memberInfo.PropertySetName(), out setMethod) == true)
                    {
                        propertyBuilder.SetSetMethod(setMethod);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a constructor to the proxy type.
        /// </summary>
        /// <param name="context">The current factory context.</param>
        private void EmitConstructor(TypeFactoryContext context)
        {
            // Build Constructor.
            ConstructorBuilder constructorBuilder = context.TypeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.HasThis,
                new Type[]
                {
                    context.BaseType,
                    typeof(IServiceProvider)
                });

            constructorBuilder.DefineParameter(1, ParameterAttributes.None, "target");
            constructorBuilder.DefineParameter(2, ParameterAttributes.None, "serviceProvider");

            ILGenerator il = constructorBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, context.BaseObjectField);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, context.ServiceProviderField);

            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits the IL to call a proxy method implementation.
        /// </summary>
        /// <param name="context">The type factory context.</param>
        /// <param name="methodIL">The methods <see cref="ILGenerator"/>.</param>
        /// <param name="methodInfo">The method to implement.</param>
        /// <param name="methodArgTypes">The methods arguments.</param>
        private void EmitCallToProxyImplementation(
            TypeFactoryContext context,
            ILGenerator methodIL,
            MethodInfo methodInfo,
            Type[] methodArgTypes)
        {
            MethodInfo invokeMethod = typeof(IProxy).GetMethod("Invoke", new Type[] { typeof(MethodInfo), typeof(object[]) });
            MethodInfo makeGenericMethod = typeof(MethodInfo).GetMethod("MakeGenericMethod", new Type[] { typeof(Type[]) });

            LocalBuilder localGenArgTypes = methodIL.DeclareLocal(typeof(Type[]));
            LocalBuilder localArguments = methodIL.DeclareLocal(typeof(object[]));
            LocalBuilder localMethodInfo = methodIL.DeclareLocal(typeof(MethodInfo));

            if (methodInfo.IsGenericMethodDefinition == true)
            {
                Type[] genArgTypes = methodInfo.GetGenericArguments();

                methodIL.EmitArray(
                   typeof(Type),
                    localGenArgTypes,
                    genArgTypes.Length,
                    (ilGen, index) =>
                    {
                        ilGen.EmitTypeOf(genArgTypes[index]);
                    });
            }

            methodIL.Emit(OpCodes.Nop);

            // Build the arguments array.
            methodIL.EmitArray(
                typeof(object),
                localArguments,
                methodArgTypes.Length,
                (ilGen, index) =>
                {
                    ilGen.EmitLdArg(index);
                    ilGen.EmitConv(methodArgTypes[index], typeof(object), false);
                });

            methodIL.Emit(OpCodes.Nop);

            if (methodInfo.IsGenericMethodDefinition == true)
            {
                // Get the MethodInfo of the method being called on the proxy.
                methodIL.EmitMethod(methodInfo);
                methodIL.Emit(OpCodes.Ldloc_S, localGenArgTypes);
                methodIL.Emit(OpCodes.Callvirt, makeGenericMethod);
                methodIL.Emit(OpCodes.Stloc_S, localMethodInfo);
            }
            else
            {
                // Get the MethodInfo of the method being called on the proxy.
                methodIL.EmitMethod(methodInfo);
                methodIL.Emit(OpCodes.Stloc_S, localMethodInfo);
            }

            // Call the proxy implementations invoke method.
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, context.BaseObjectField);
            methodIL.Emit(OpCodes.Ldloc_S, localMethodInfo);
            methodIL.Emit(OpCodes.Ldloc_S, localArguments);
            methodIL.Emit(OpCodes.Callvirt, invokeMethod);

            // Does the method have a return type?
            if (methodInfo.ReturnType != typeof(void))
            {
                methodIL.EmitConv(typeof(object), methodInfo.ReturnType, false);
            }
            else
            {
                // Remove the returned value
                methodIL.Emit(OpCodes.Pop);
            }

            methodIL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits the IL for the <see cref="IProxiedObject"/> interfaces 'ProxiedObject' property.
        /// </summary>
        /// <param name="context">The type factory context.</param>
        private void EmitIProxiedObjectInterface(TypeFactoryContext context)
        {
            MethodBuilder getProxiedObject = context.TypeBuilder.DefineMethod(
                "get_ProxiedObject",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.HasThis,
                typeof(object),
                new Type[0]);

            ILGenerator methodIL = getProxiedObject.GetILGenerator();
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, context.BaseObjectField);
            methodIL.Emit(OpCodes.Ret);

            context.TypeBuilder.AddInterfaceImplementation(typeof(IProxiedObject));
            PropertyBuilder propertyProxiedObject = context
                .TypeBuilder
                .DefineProperty(
                    "ProxiedObject",
                    PropertyAttributes.None,
                    typeof(object),
                    Type.EmptyTypes);

            propertyProxiedObject.SetGetMethod(getProxiedObject);
        }

        /// <summary>
        /// Emits the IL for the <see cref="IProxyMetadata"/> interface.
        /// </summary>
        /// <param name="context">The type factory context.</param>
        private void EmitIProxyMetadataInterface(TypeFactoryContext context)
        {
            context.TypeBuilder.AddInterfaceImplementation(typeof(IProxyMetadata));

            context.TypeBuilder.EmitProperty<object>(
                "InstanceData",
                CallingConventions.HasThis,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                (methodIL) =>
                {
                    methodIL.Emit(OpCodes.Ldarg_0);
                    methodIL.Emit(OpCodes.Ldfld, context.BaseObjectField);
                    methodIL.Emit(OpCodes.Ret);
                },
                null);

            context
                .TypeBuilder
                .EmitProperty<IServiceProvider>(
                    "ServiceProvider",
                    CallingConventions.HasThis,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                    (methodIL) =>
                    {
                        methodIL.Emit(OpCodes.Ldarg_0);
                        methodIL.Emit(OpCodes.Ldfld, context.ServiceProviderField);
                        methodIL.Emit(OpCodes.Ret);
                    },
                    null);
        }
    }
}