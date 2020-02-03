namespace DynProxy
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Diagnostics;

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
        /// Initialises a new instance of the <see cref="Interceptor"/> class.
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

            var interceptors = interceptorTypes.Distinct().Select(t => (IInterceptor) Activator.CreateInstance(t));
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
    }
}