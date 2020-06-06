namespace DynProxyUnitTests
{
    using System.Threading.Tasks;
    using DynProxyUnitTests.Resources;
    using FluentIL;
    using Microsoft.VisualStudio.TestTools.UnitTesting;


    [TestClass]
    public class AsyncProxyUnitTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            DebugOutput.Output = new ConsoleOutput();
        }

        [TestMethod]
        public async Task AsyncProxy_CallAsyncMethod()
        {
            var proxyObj = new AsyncProxy();
            var proxy = proxyObj.GetProxyObject();

            var result = await proxy.GetTokenAsync();

            Assert.IsNotNull(result);
        }
    }
}