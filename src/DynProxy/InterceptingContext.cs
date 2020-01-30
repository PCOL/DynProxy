namespace DynProxy
{
    using System.Reflection;

    /// <summary>
    /// Represents the intercepted context.
    /// </summary>
    public class InterceptingContext
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="InterceptingContext"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="arguments">The arguments.</param>
        internal InterceptingContext(MemberInfo memberInfo, object[] arguments)
        {
            this.MemberInfo = memberInfo;
            this.Arguments = arguments;
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
    }
}