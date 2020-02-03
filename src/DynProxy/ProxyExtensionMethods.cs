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
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Proxy extension methods.
    /// </summary>
    public static class ProxyExtensionMethods
    {
        /// <summary>
        /// Adds proxy services to a <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddProxyServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IProxyTypeGenerator, ProxyTypeGenerator>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="services">A <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddScopedInterceptor<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddScoped<TService>(
                sp =>
                {
                    var impl = sp.GetOrCreateInstance<TImplementation>();
                    var interceptor = new Interceptor<TService>(impl);
                    return interceptor.Build();
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="services">A <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationFactory">An implementation factory.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddScopedInterceptor<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return services.AddScoped<TService>(
                sp =>
                {
                    var impl = implementationFactory(sp);
                    var interceptor = new Interceptor<TService>(impl);
                    return interceptor.Build();
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="services">A <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddTransientInterceptor<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddTransient<TService>(
                sp =>
                {
                    var impl = sp.GetOrCreateInstance<TImplementation>();
                    var interceptor = new Interceptor<TService>(impl);
                    return interceptor.Build();
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="services">A <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationFactory">An implementation factory.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddTransientInterceptor<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return services.AddTransient<TService>(
                sp =>
                {
                    var impl = implementationFactory(sp);
                    var interceptor = new Interceptor<TService>(impl);
                    return interceptor.Build();
                });
        }

       /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="services">A <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddSingletonInterceptor<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddSingleton<TService>(
                sp =>
                {
                    var impl = sp.GetOrCreateInstance<TImplementation>();
                    var interceptor = new Interceptor<TService>(impl);
                    return interceptor.Build();
                });
        }

        /// <summary>
        /// Adds a singleton interceptor.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="services">A <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationFactory">An implementation factory.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddSingletonInterceptor<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return services.AddSingleton<TService>(
                sp =>
                {
                    var impl = implementationFactory(sp);
                    var interceptor = new Interceptor<TService>(impl);
                    return interceptor.Build();
                });
        }

        /// <summary>
        /// Gets or creates an instance of the implementation.
        /// </summary>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="serviceProvider">A service provider.</param>
        /// <returns>The service provider.</returns>
        private static TImplementation GetOrCreateInstance<TImplementation>(this IServiceProvider serviceProvider)
        {
            var impl = serviceProvider.GetService<TImplementation>();
            if (impl == null)
            {
                impl = serviceProvider.CreateInstance<TImplementation>();
            }

            return impl;
        }

        /// <summary>
        /// Creates an instance of the implementation.
        /// </summary>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="serviceProvider">A service provider.</param>
        /// <returns>The service provider.</returns>
        private static TImplementation CreateInstance<TImplementation>(this IServiceProvider serviceProvider)
        {
            TImplementation impl = default(TImplementation);
            var ctors = typeof(TImplementation).GetConstructors();
            var ctor = ctors.FirstOrDefault();
            var parms = ctor?.GetParameters();
            if (parms.Any() == true)
            {
                object[] values = new object[parms.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = serviceProvider.GetRequiredService(parms[i].ParameterType);
                }

                impl = (TImplementation)Activator.CreateInstance(typeof(TImplementation), values);
            }
            else
            {
                impl = Activator.CreateInstance<TImplementation>();
            }

            return impl;
        }
    }
}