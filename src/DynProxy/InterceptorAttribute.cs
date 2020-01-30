namespace DynProxy
{
    using System;

    /// <summary>
    /// An attribute for marking methods for interception
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public class InterceptorAttribute
        : Attribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="InterceptorAttribute"/> class.
        /// </summary>
        /// <param name="interceptorType"></param>
        public InterceptorAttribute(Type interceptorType)
        {
            this.InterceptorType = interceptorType;
        }

        /// <summary>
        /// Gets the interceptor type.
        /// </summary>
        /// <value></value>
        public Type InterceptorType { get; }
    }
}