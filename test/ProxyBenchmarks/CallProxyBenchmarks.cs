namespace ProxyBenchmarks
{
    using BenchmarkDotNet.Attributes;
    using ProxyBenchmarks.Resources;

    public class CallProxyBenchmarks
    {
        private IMyProxy realProxy;

        private IEmptyMethodProxy realEmptyMethodProxy;

        [GlobalSetup(Target = nameof(CallSimpleMethod))]
        public void GlobalSetupSimple()
        {
            var proxy = new MyProxy();
            this.realProxy = proxy.GetProxyObject();
        }

        [GlobalSetup(Target = nameof(CallEmptyMethod))]
        public void GlobalSetupEmpty()
        {
            var emptyMethodproxy = new EmptyMethodProxy();
            this.realEmptyMethodProxy = emptyMethodproxy.GetProxyObject();
        }

        [Benchmark]
        public void CallEmptyMethod()
        {
            this.realEmptyMethodProxy.EmptyMethod();
        }

        [Benchmark]
        public void CallSimpleMethod()
        {
            this.realProxy.Add(10, 10);
        }
    }
}