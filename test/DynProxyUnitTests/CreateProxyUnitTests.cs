namespace DynProxyUnitTests
{
    using DynProxyUnitTests.Resources;
    using FluentIL;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CreateProxyUnitTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            //DebugOutput.Output = new ConsoleOutput();
        }

        [TestMethod]
        public void CreateProxy_CallAddMethod_AddsTwoIntegers()
        {
            var proxy = new MyProxy();
            var realProxy = proxy.GetProxyObject();

            Assert.IsNotNull(realProxy);
            Assert.AreEqual(100, realProxy.Add(40, 60));
        }

        [TestMethod]
        public void CreateProxy_WithProperty_GetsAndSetsProperty()
        {
            var proxy = new MyProxy();
            var realProxy = proxy.GetProxyObject();

            Assert.IsNotNull(realProxy);

            realProxy.StringProperty = "Test";
            Assert.AreEqual("Test", realProxy.StringProperty);
        }

        [TestMethod]
        public void CreateProxy_WithMethodContainingAReferenceOutParameter_ReturnsOutParameter()
        {
            var proxy = new MyProxy();
            var realProxy = proxy.GetProxyObject();

            Assert.IsNotNull(realProxy);

            realProxy.StringProperty = "Test";
            var result = realProxy.TryGetStringProperty(out string value);

            Assert.IsTrue(result);
            Assert.AreEqual("Test", value);
        }

        [TestMethod]
        public void CreateProxy_WithMethodContainingAValueOutParameter_ReturnsOutParameter()
        {
            var proxy = new MyProxy();
            var realProxy = proxy.GetProxyObject();

            Assert.IsNotNull(realProxy);

            realProxy.BooleanProperty = true;
            var result = realProxy.TryGetBooleanProperty(out bool value);

            Assert.IsTrue(result);
            Assert.IsTrue(value);
        }

        [TestMethod]
        public void CreateGenericProxy_OfTypeString()
        {
            var proxy = new GenericProxy<string>();
            var realProxy = proxy.GetProxyObject();

            Assert.IsNotNull(realProxy);

            realProxy.SetProperty("Test");

            Assert.AreEqual("Test", realProxy.Property);
        }
    }
}
