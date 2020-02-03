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
        /// Initialises a new instance of the <see cref="InterceptedContext"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="returnValue">The return value.</param>
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