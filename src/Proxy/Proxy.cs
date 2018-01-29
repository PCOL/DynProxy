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
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Represents an abstract proxy implementation.
    /// </summary>
    /// <typeparam name="T">The type of proxy required.</typeparam>
    public abstract class Proxy<T>
        : IProxy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Proxy{T}"/> class.
        /// </summary>
        protected Proxy()
        {
        }

        /// <summary>
        /// Implementation of the proxy invoke method.
        /// </summary>
        /// <param name="methodInfo">The details of the method being called.</param>
        /// <param name="arguments">The methods arguments.</param>
        /// <returns>The result of the method call.</returns>
        object IProxy.Invoke(MethodInfo methodInfo, object[] arguments)
        {
            return this.Invoke(methodInfo, arguments);
        }

        /// <summary>
        /// Returns the proxy object.
        /// </summary>
        /// <returns>The proxy object.</returns>
        public T GetProxyObject()
        {
            return this
                .GetProxyTypeGenerator()
                .CreateProxy<T>(this);
        }

        protected virtual IProxyTypeGenerator GetProxyTypeGenerator()
        {
            return new ProxyTypeGenerator();
        }

        /// <summary>
        /// Called when a method is invoked on the proxy.
        /// </summary>
        /// <param name="methodInfo">The method being called.</param>
        /// <param name="arguments">The methods arguments.</param>
        /// <returns>The result of the method call.</returns>
        protected abstract object Invoke(MethodInfo methodInfo, object[] arguments);
    }
}
