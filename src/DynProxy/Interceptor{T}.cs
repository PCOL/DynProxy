namespace DynProxy
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
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

            this.Intercepted = new ProxyTypeGenerator().CreateProxy<T>(this);
        }

        /// <summary>
        /// Gets the intercepted instance.
        /// </summary>
        public T Intercepted { get; }

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
                var context = new InterceptingContext(methodInfo, arguments);
                foreach (var interceptor in interceptors)
                {
                    if (isAsync == true)
                    {
                        interceptor.BeforeMethodAsync(context).Wait();
                    }
                    else
                    {
                        interceptor.BeforeMethod(context);
                    }
                }
            }

            var returnObj = methodInfo.Invoke(this.interceptedInstance, arguments);

            if (interceptors.Any() == true)
            {
                var context = new InterceptedContext(methodInfo, arguments, returnObj);
                foreach (var interceptor in interceptors)
                {
                    if (isAsync == true)
                    {
                        interceptor.AfterMethodAsync(context).Wait();
                    }
                    else
                    {
                        interceptor.AfterMethod(context);
                    }
                }
            }

            return returnObj;
        }
    }
}