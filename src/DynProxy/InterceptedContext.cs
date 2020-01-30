namespace DynProxy
{
    using System.Reflection;

    /// <summary>
    /// Represents the intercepted context.
    /// </summary>
    public class InterceptedContext
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="InterceptedContext"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="returnValue">The return value.</param>
        internal InterceptedContext(MemberInfo memberInfo, object[] arguments, object returnValue)
        {
            this.MemberInfo = memberInfo;
            this.Arguments = arguments;
            this.ReturnValue = returnValue;
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
    }
}