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
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// An async proxy method executor.
    /// </summary>
    /// <typeparam name="TReturn">The return type.</typeparam>
    public class AsyncTaskExecutor<TReturn>
    {
        /// <summary>
        /// The proxy to execute the method on.
        /// </summary>
        private readonly IProxy proxy;

        /// <summary>
        /// The method to execute.
        /// </summary>
        private readonly MethodInfo methodInfo;

        /// <summary>
        /// The methods arguments.
        /// </summary>
        private readonly object[] arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskExecutor{TReturn}"/> struct.
        /// </summary>
        /// <param name="proxy">The proxy to execute on.</param>
        /// <param name="methodInfo">The method to execute.</param>
        /// <param name="arguments">The methods arguments.</param>
        public AsyncTaskExecutor(IProxy proxy, MethodInfo methodInfo, object[] arguments)
        {
            this.proxy = proxy;
            this.methodInfo = methodInfo;
            this.arguments = arguments;
        }

        /// <summary>
        /// Executes the method asynchronously.
        /// </summary>
        /// <returns>The result.</returns>
        public async Task<TReturn> ExecuteAsync()
        {
            var task = this.proxy.InvokeAsync(this.methodInfo, this.arguments);
            return await task.AsTask<object, TReturn>();

        }
    }
}