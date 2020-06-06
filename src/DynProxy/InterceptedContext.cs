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
    using System.Reflection;

    /// <summary>
    /// Represents the intercepted context.
    /// </summary>
    public class InterceptedContext
    {
        /// <summary>
        /// Gets the properties.
        /// </summary>
        private readonly Dictionary<string, object> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptedContext"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="returnValue">The return value.</param>
        /// <param name="timeTaken">The amount of time taken.</param>
        /// <param name="properties">A dictionary of custom properties.</param>
        internal InterceptedContext(
            MemberInfo memberInfo,
            object[] arguments,
            object returnValue,
            TimeSpan timeTaken,
            Dictionary<string, object> properties)
        {
            this.MemberInfo = memberInfo;
            this.Arguments = arguments;
            this.ReturnValue = returnValue;
            this.TimeTaken = timeTaken;
            this.properties = properties;
        }

        /// <summary>
        /// Gets the member info.
        /// </summary>
        public MemberInfo MemberInfo { get; }

        /// <summary>
        /// Gets the member name.
        /// </summary>
        public string Name => this.MemberInfo.Name;

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Gets the return value.
        /// </summary>
        public object ReturnValue { get; }

        /// <summary>
        /// Gets the time the methods took to execute.
        /// </summary>
        public TimeSpan TimeTaken { get; }

        /// <summary>
        /// Gets custom properties.
        /// </summary>
        /// <param name="key">A property key.</param>
        public object this[string key]
        {
            get
            {
                if (this.properties != null &&
                    this.properties.TryGetValue(key, out object value) == true)
                {
                    return value;
                }

                return null;
            }
        }
    }
}