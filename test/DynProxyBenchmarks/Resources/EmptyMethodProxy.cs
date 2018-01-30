namespace DynProxyBenchmarks.Resources
{
    using System.Reflection;
    using DynProxy;

    public class EmptyMethodProxy
        : Proxy<IEmptyMethodProxy>
    {
        protected override object Invoke(MethodInfo methodInfo, object[] arguments)
        {
            return null;
        }
    }
}