namespace ProxyBenchmarks
{
    using BenchmarkDotNet;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;
    using ProxyBenchmarks.Resources;

    [CoreJob()]
    public class CreateProxyBenchmarks
    {
        [Benchmark]
        public void CreateProxy()
        {
            var proxy = new MyProxy();
            var realProxy = proxy.GetProxyObject();
        }
    }
}