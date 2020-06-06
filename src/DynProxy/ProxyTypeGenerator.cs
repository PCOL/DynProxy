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

namespace DynProxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading.Tasks;
    using FluentIL;

    /// <summary>
    /// A factory for building proxy types.
    /// </summary>
    public class ProxyTypeGenerator
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
        /// Generate the proxy instance.
        /// </summary>
        /// <typeparam name="T">The type of proxy to create.</typeparam>
        /// <param name="implementation">The proxy implementation.</param>
        /// <returns>An instance of the proxy type.</returns>
        public T CreateProxy<T>(IProxy implementation)
        {
            Utility.ThrowIfArgumentNull(implementation, nameof(implementation));

            Type proxyType = TypeFactory
                .Default
                .GetType(
                    TypeName(typeof(T), typeof(IProxy)),
                    true);

            if (proxyType == null)
            {
                proxyType = this.GenerateProxyType(typeof(T), typeof(IProxy));
            }

            return (T)Activator.CreateInstance(proxyType, implementation);
        }

        /// <summary>
        /// Generate the proxy instance.
        /// </summary>
        /// <param name="proxyType">The proxy type.</param>
        /// <param name="implementation">The proxy implementation.</param>
        /// <param name="action">Optional proxy builder action.</param>
        /// <param name="proxyOptions">Optional proxy options.</param>
        /// <returns>An instance of the proxy type.</returns>
        public object CreateProxy(Type proxyType, IProxy implementation, Action<IProxyBuilderContext> action = null, ProxyOptions proxyOptions = null)
        {
            return this.CreateProxy(proxyType, null, implementation, action, proxyOptions);
        }

        /// <summary>
        /// Generate the proxy instance.
        /// </summary>
        /// <param name="proxyType">The proxy type.</param>
        /// <param name="proxyBaseType">The proxy base type.</param>
        /// <param name="implementation">The proxy implementation.</param>
        /// <param name="action">Optional proxy builder action.</param>
        /// <param name="proxyOptions">Optional proxy options.</param>
        /// <returns>An instance of the proxy type.</returns>
        public object CreateProxy(Type proxyType, Type proxyBaseType, IProxy implementation, Action<IProxyBuilderContext> action = null, ProxyOptions proxyOptions = null)
        {
            Utility.ThrowIfArgumentNull(implementation, nameof(implementation));

            var typeName = proxyOptions?.TypeName ?? TypeName(proxyType, typeof(IProxy));

            Type proxy = TypeFactory
                .Default
                .GetType(
                    typeName,
                    true);

            if (proxy == null)
            {
                proxy = this.GenerateProxyType(proxyType, typeof(IProxy), proxyBaseType, action, proxyOptions);
            }

            return Activator.CreateInstance(proxy, implementation);
        }

        /// <summary>
        /// Generate the proxy type.
        /// </summary>
        /// <param name="proxyType">The interface the proxy type must implement.</param>
        /// <param name="proxyTargetType">The target type to receive the proxied calls.</param>
        /// <param name="proxyOptions">The proxy generation options.</param>
        /// <returns>A <see cref="Type"/> representing the proxy type.</returns>
        private Type GenerateProxyType(
            Type proxyType,
            Type proxyTargetType,
            ProxyOptions proxyOptions = null)
        {
            return this.GenerateProxyType(proxyType, proxyTargetType, null, null, proxyOptions);
        }

        /// <summary>
        /// Generate the proxy type.
        /// </summary>
        /// <param name="proxyType">The interface the proxy type must implement.</param>
        /// <param name="proxyTargetType">The target type to receive the proxied calls.</param>
        /// <param name="proxyBaseType">The base type.</param>
        /// <param name="action">An action to allow build type injection.</param>
        /// <param name="proxyOptions">The proxy generation options.</param>
        /// <returns>A <see cref="Type"/> representing the proxy type.</returns>
        private Type GenerateProxyType(
            Type proxyType,
            Type proxyTargetType,
            Type proxyBaseType,
            Action<IProxyBuilderContext> action,
            ProxyOptions proxyOptions = null)
        {
            if (proxyBaseType?.IsInterface == true)
            {
                throw new ArgumentException("Argument cannot be an interface", "proxyBaseType");
            }

            if (proxyType.IsInterface == false &&
                proxyType != proxyBaseType)
            {
                throw new ArgumentException("Argument is not an interface", "proxyType");
            }

            if (typeof(IProxy).IsAssignableFrom(proxyTargetType) == false)
            {
                throw new ArgumentException("Argument does not implement <IProxy>", "proxiedType");
            }

            var typeName = proxyOptions?.TypeName ?? TypeName(proxyType, proxyTargetType);

            var typeBuilder = TypeFactory
                .Default
                .NewType(typeName)
                    .Public()
                    .Implements(typeof(IProxiedObject));

            if (proxyType.IsInterface == true)
            {
                typeBuilder.Implements(proxyType);
            }

            if (proxyBaseType != null)
            {
                typeBuilder.InheritsFrom(proxyBaseType);
            }

            var targetField = typeBuilder
                .NewField<IProxy>("target")
                .Private();

            var context = new ProxyBuilderContext(
                typeBuilder,
                proxyType,
                proxyTargetType,
                targetField,
                null);

            action?.Invoke(context);

            if (proxyType.IsInterface == true)
            {
                this.ImplementInterfaces(context, context.NewType);
            }

            this.EmitConstructor(context);

            this.EmitIProxiedObjectInterface(context);

            return context
                .TypeBuilder
                .CreateType();
        }

        /// <summary>
        /// Implements the interface for the proxy type.
        /// </summary>
        /// <param name="context">The current builder context.</param>
        /// <param name="interfaceType">The interface type.</param>
        private void ImplementInterfaces(ProxyBuilderContext context, Type interfaceType)
        {
            this.ImplementInterface(context, interfaceType);
            var interfaces = interfaceType.GetInterfaces();
            if (interfaces != null)
            {
                foreach (var iface in interfaces)
                {
                    this.ImplementInterfaces(context, iface);
                }
            }
        }

        /// <summary>
        /// Implements the interface for the proxy type.
        /// </summary>
        /// <param name="context">The current builder context.</param>
        /// <param name="interfaceType">The interface type.</param>
        private void ImplementInterface(ProxyBuilderContext context, Type interfaceType)
        {
            var propertyMethods = new Dictionary<string, IMethodBuilder>();

            foreach (var memberInfo in interfaceType.GetMembers())
            {
                if (memberInfo.MemberType == MemberTypes.Method)
                {
                    MethodInfo methodInfo = (MethodInfo)memberInfo;
                    Type[] methodArgs = methodInfo
                        .GetParameters()
                        .Select(p => p.ParameterType)
                        .ToArray();

                    Type[] genericArguments = null;
                    var methodBuilder = context
                        .TypeBuilder
                        .NewMethod(methodInfo.Name)
                            .MethodAttributes(MethodAttributes.Public | MethodAttributes.Virtual)
                            .Params(methodArgs)
                            .Returns(methodInfo.ReturnType);

                    if (methodInfo.ContainsGenericParameters == true)
                    {
                        genericArguments = methodInfo.GetGenericArguments();
                        foreach (var arg in genericArguments)
                        {
                            methodBuilder.NewGenericParameter(arg.Name)
                                .Attributes = arg.GenericParameterAttributes;
                        }
                    }

                    var methodIL = methodBuilder.Body();

                    this.EmitCallToProxyImplementation(context, methodIL, methodInfo, methodArgs);

                    if (methodInfo.IsProperty() == true)
                    {
                        propertyMethods.Add(methodInfo.Name, methodBuilder);
                    }
                }
                else if (memberInfo.MemberType == MemberTypes.Property)
                {
                    var propertyBuilder = context
                        .TypeBuilder
                        .NewProperty(memberInfo.Name, ((PropertyInfo)memberInfo).PropertyType)
                        .Attributes(PropertyAttributes.SpecialName);

                    if (propertyMethods.TryGetValue(
                        memberInfo.PropertyGetName(),
                        out IMethodBuilder getMethod) == true)
                    {
                        propertyBuilder.GetMethod = getMethod;
                    }

                    if (propertyMethods.TryGetValue(
                        memberInfo.PropertySetName(),
                        out IMethodBuilder setMethod) == true)
                    {
                        propertyBuilder.SetMethod = setMethod;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a constructor to the proxy type.
        /// </summary>
        /// <param name="context">The current builder context.</param>
        private void EmitConstructor(ProxyBuilderContext context)
        {
            // Build Constructor.
            var constructorBuilder = context
                .TypeBuilder
                .NewConstructor()
                .Public()
                .HideBySig()
                .SpecialName()
                .RTSpecialName()
                .Param(context.BaseType, "target")
                .Body()
                    .LdArg0()
                    .LdArg1()
                    .StFld(context.BaseObjectField)
                    .Ret();
        }

        /// <summary>
        /// Emits the IL to call a proxy method implementation.
        /// </summary>
        /// <param name="context">The proxy builder context.</param>
        /// <param name="methodIL">The methods <see cref="ILGenerator"/>.</param>
        /// <param name="methodInfo">The method to implement.</param>
        /// <param name="methodArgTypes">The methods arguments.</param>
        private void EmitCallToProxyImplementation(
            ProxyBuilderContext context,
            IEmitter methodIL,
            MethodInfo methodInfo,
            Type[] methodArgTypes)
        {
            MethodInfo invokeMethod = typeof(IProxy).GetMethod("Invoke", new Type[] { typeof(MethodInfo), typeof(object[]) });
            MethodInfo invokeAsyncMethod = typeof(IProxy).GetMethod("InvokeAsync", new Type[] { typeof(MethodInfo), typeof(object[]) });
            MethodInfo makeGenericMethod = typeof(MethodInfo).GetMethod("MakeGenericMethod", new Type[] { typeof(Type[]) });

            methodIL
                .DeclareLocal(typeof(Type[]), out ILocal localGenArgTypes)
                .DeclareLocal(typeof(object[]), out ILocal localArguments)
                .DeclareLocal(typeof(MethodInfo), out ILocal localMethodInfo);

            if (methodInfo.IsGenericMethodDefinition == true)
            {
                Type[] genArgTypes = methodInfo.GetGenericArguments();

                methodIL.Array(
                    typeof(Type),
                    localGenArgTypes,
                    genArgTypes.Length,
                    (index) =>
                    {
                        methodIL.EmitTypeOf(genArgTypes[index]);
                    });
            }

            methodIL.Nop();

            // Build the arguments array.
            methodIL.Array(
                typeof(object),
                localArguments,
                methodArgTypes.Length,
                (index) =>
                {
                    methodIL
                        .LdArg(index + 1)
                        .Conv(methodArgTypes[index], typeof(object), false);
                });

            methodIL.Nop();

            if (methodInfo.IsGenericMethodDefinition == true)
            {
                // Get the MethodInfo of the method being called on the proxy.
                methodIL
                    .EmitMethod(methodInfo, methodInfo.DeclaringType)
                    .LdLoc(localGenArgTypes)
                    .CallVirt(makeGenericMethod)
                    .StLoc(localMethodInfo);
            }
            else
            {
                // Get the MethodInfo of the method being called on the proxy.
                methodIL
                    .EmitMethod(methodInfo, methodInfo.DeclaringType)
                    .StLoc(localMethodInfo);
            }

            methodIL.Nop();

            if (methodInfo.ReturnType == typeof(Task) ||
                (methodInfo.ReturnType.IsGenericType == true && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
            {
                var actualReturnType = methodInfo.ReturnType.GetGenericArguments()[0];
                var asyncTaskExecutorType = typeof(AsyncTaskExecutor<>).MakeGenericType(actualReturnType);
                var asyncTaskExecutorTypeCtor = asyncTaskExecutorType.GetConstructor(new[] { typeof(IProxy), typeof(MethodInfo), typeof(object[]) });
                var asyncTaskExecutorExecuteAsync = asyncTaskExecutorType.GetMethod("ExecuteAsync");

                methodIL
                    .DeclareLocal(asyncTaskExecutorType, out ILocal localAsyncProxy)
                    .DeclareLocal(typeof(Task<>).MakeGenericType(actualReturnType), out ILocal localReturn)

/* class
                    .LdArg0()
                    .LdFld(context.BaseObjectField)
                    .LdLoc(localMethodInfo)
                    .LdLoc(localArguments)
                    .Newobj(asyncTaskExecutorTypeCtor)
                    .StLoc(localAsyncProxy)

                    .LdLoc(localAsyncProxy)
                    .Call(asyncTaskExecutorExecuteAsync)
                    .StLoc(localReturn)
*/

/* Struct */
                    .LdLocAS(localAsyncProxy)
                    .LdArg0()
                    .LdFld(context.BaseObjectField)
                    .LdLoc(localMethodInfo)
                    .LdLoc(localArguments)
                    .Call(asyncTaskExecutorTypeCtor)
                    .LdLocAS(localAsyncProxy)
                    .Call(asyncTaskExecutorExecuteAsync)
                    .StLoc(localReturn)

                    .LdLoc(localReturn);
            }
            else
            {
                // Call the proxy implementations invoke method.
                methodIL
                    .LdArg0()
                    .LdFld(context.BaseObjectField)
                    .LdLoc(localMethodInfo)
                    .LdLoc(localArguments)
                    .CallVirt(invokeMethod);

                // Does the method have a return type?
                if (methodInfo.ReturnType != typeof(void))
                {
                    methodIL.Conv(typeof(object), methodInfo.ReturnType, false);
                }
                else
                {
                    // Remove the returned value
                    methodIL.Pop();
                }

                var parms = methodInfo.GetParameters();
                if (parms.Any() == true)
                {
                    int index = 0;
                    foreach (var parm in parms)
                    {
                        if (parm.IsOut == true)
                        {
                            methodIL
                                .LdArg(parm.Position + 1)
                                .LdLoc(localArguments)
                                .LdcI4(index)
                                .LdElemRef()
                                .StIndRef()
                                .Nop();
                        }

                        index++;
                    }
                }
            }

            methodIL.Ret();
        }

        /// <summary>
        /// Emits the IL for the <see cref="IProxiedObject"/> interfaces 'ProxiedObject' property.
        /// </summary>
        /// <param name="context">The proxy builder context.</param>
        private void EmitIProxiedObjectInterface(ProxyBuilderContext context)
        {
            var propertyProxiedObject = context
                .TypeBuilder
                .NewProperty<object>("ProxiedObject")
                .Getter(m => m
                    .Public()
                    .Virtual()
                    .HideBySig()
                    .NewSlot()
                    .Body()
                        .LdArg0()
                        .LdFld(context.BaseObjectField)
                        .Ret());
        }
    }
}