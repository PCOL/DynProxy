namespace ProxyBenchmarks
{
    using System;
    using BenchmarkDotNet.Running;

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CreateProxyBenchmarks>();
            BenchmarkRunner.Run<CallProxyBenchmarks>();
        }
    }
}
