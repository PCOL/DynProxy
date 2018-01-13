namespace ProxyUnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ProxyUnitTests.Resources;

    [TestClass]
    public class CreateProxyUnitTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var proxy = new MyProxy();
            var realProxy = proxy.GetProxyObject();

            Assert.IsNotNull(realProxy);

            Assert.AreEqual(100, realProxy.Add(40, 60));
        }
    }
}
