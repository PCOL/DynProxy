namespace ProxyBenchmarks.Resources
{
    using System.Reflection;
    using Proxy;

    public class EmptyMethodProxy
        : Proxy<IEmptyMethodProxy>
    {
        protected override object Invoke(MethodInfo methodInfo, object[] arguments)
        {
            return null;
        }
    }
}