/*
MIT License

Copyright (c) 2020 P Collyer

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
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a type interceptor.
    /// </summary>
    /// <typeparam name="T">The type to be intercepted.</typeparam>
    public class Interceptor<T>
        : IProxy
        where T : class
    {
        /// <summary>
        /// The intercepted type instance.
        /// </summary>
        private readonly T interceptedInstance;

        /// <summary>
        /// A list of type level interceptor types.
        /// </summary>
        private readonly IEnumerable<Type> baseInterceptorTypes;

        /// <summary>
        /// The inteceptor.
        /// </summary>
        private readonly T interceptor;

        /// <summary>
        /// A dictionary of method interceptor types.
        /// </summary>
        private Dictionary<int, IEnumerable<Type>> methodInterceptorTypes = new Dictionary<int, IEnumerable<Type>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Interceptor{T}"/> class.
        /// </summary>
        /// <param name="instance">The instance to intercept.</param>
        public Interceptor(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (typeof(T).IsInterface == false)
            {
                throw new NotSupportedException("Only interface types are supported!");
            }

            this.interceptedInstance = instance;
            var attrs = typeof(T).GetCustomAttributes<InterceptorAttribute>(true);
            if (attrs.Any() == true)
            {
                this.baseInterceptorTypes = attrs.Select(a => a.InterceptorType);
            }

            this.interceptor = new ProxyTypeGenerator().CreateProxy<T>(this);
        }

        /// <summary>
        /// Builds the interceptor.
        /// </summary>
        /// <returns>The interceptor.</returns>
        public T Build()
        {
            return this.interceptor;
        }

        /// <inheritdoc />
        public object Invoke(MethodInfo methodInfo, object[] arguments)
        {
            if (this.methodInterceptorTypes.TryGetValue(methodInfo.GetMetadataToken(), out IEnumerable<Type> interceptorTypes) == false)
            {
                interceptorTypes = Enumerable.Empty<Type>();
                var attrs = methodInfo.GetCustomAttributes<InterceptorAttribute>(true);
                if (attrs.Any() == true)
                {
                    interceptorTypes = attrs.Select(a => a.InterceptorType);
                    this.methodInterceptorTypes.Add(methodInfo.GetMetadataToken(), interceptorTypes);
                }
            }

            if (this.baseInterceptorTypes != null)
            {
                interceptorTypes = this.baseInterceptorTypes.Union(interceptorTypes);
            }

            var isAsync = methodInfo.ReturnType.IsGenericType == true &&
                (methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task) ||
                methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

            var interceptors = interceptorTypes.Distinct().Select(t => (IInterceptor)Activator.CreateInstance(t));
            if (interceptors.Any() == true)
            {
                var interceptingContext = new InterceptingContext(methodInfo, arguments);
                foreach (var interceptor in interceptors)
                {
                    if (isAsync == true)
                    {
                        interceptor.BeforeMethodAsync(interceptingContext).Wait();
                    }
                    else
                    {
                        interceptor.BeforeMethod(interceptingContext);
                    }
                }

                var sw = Stopwatch.StartNew();
                var returnObj = methodInfo.Invoke(this.interceptedInstance, arguments);

                var interceptedContext = new InterceptedContext(methodInfo, arguments, returnObj, sw.Elapsed, interceptingContext.Properties);
                foreach (var interceptor in interceptors)
                {
                    if (isAsync == true)
                    {
                        interceptor.AfterMethodAsync(interceptedContext).Wait();
                    }
                    else
                    {
                        interceptor.AfterMethod(interceptedContext);
                    }
                }

                return returnObj;
            }

            return methodInfo.Invoke(this.interceptedInstance, arguments);
        }

        /// <inheritdoc />
        public Task<object> InvokeAsync(MethodInfo methodInfo, object[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}