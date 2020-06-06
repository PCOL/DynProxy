namespace DynProxyUnitTests.Resources
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using DynProxy;

    public class AsyncProxy
        : Proxy<IAsyncProxy>
    {
        protected override object Invoke(MethodInfo methodInfo, object[] arguments)
        {
            throw new NotImplementedException();
        }

        protected override Task<object> InvokeAsync(MethodInfo methodInfo, object[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}