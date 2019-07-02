namespace DynProxyUnitTests
{
    using System;
    using DynProxy;
    using DynProxyUnitTests.Resources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProxyTypeGeneratorUnitTests
    {
        [TestMethod]
        public void ProxyTypeGenerator_GeneratorTypeWithInjection()
        {
            var implementation = new MyProxy();

            var proxy = new ProxyTypeGenerator()
                .CreateProxy(
                    typeof(IMyProxy),
                    implementation,
                    (context) =>
                    {
                        context.TypeBuilder.InheritsFrom<MarshalByRefObject>();
                    },
                    new ProxyOptions()
                    {
                        TypeName = Guid.NewGuid().ToString()
                    });

            Assert.IsNotNull(proxy);
            Assert.IsTrue(proxy is MarshalByRefObject);
        }
    }
}