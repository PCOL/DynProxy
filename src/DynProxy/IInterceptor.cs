namespace DynProxy
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the method interceptor interface.
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// Called before a method is called.
        /// </summary>
        /// <param name="context">The intercepting context.</param>
        void BeforeMethod(InterceptingContext context);

        /// <summary>
        /// Called after a method is called.
        /// </summary>
        /// <param name="context">The intercepted context.</param>
        void AfterMethod(InterceptedContext context);

        /// <summary>
        /// Called before an async method is called.
        /// </summary>
        /// <param name="context">The intercepting context.</param>
        /// <returns>A task.</returns>
        Task BeforeMethodAsync(InterceptingContext context);

        /// <summary>
        /// Called after an async method is called.
        /// </summary>
        /// <param name="context">The intercepted context.</param>
        /// <returns>A task.</returns>
        Task AfterMethodAsync(InterceptedContext context);
    }
}