namespace DynProxyUnitTests
{
    using System;
    using System.Reflection;
    using DynProxy;

    public class GenericProxy<T>
        : Proxy<IGenericProxy<T>>
    {
        private T property;

        protected override object Invoke(MethodInfo methodInfo, object[] arguments)
        {
            if (methodInfo.Name == nameof(IGenericProxy<string>.SetProperty))
            {
                this.property = (T)arguments[0];
            }
            else if (methodInfo.Name == $"get_{nameof(IGenericProxy<string>.Property)}")
            {
                return property;
            }

            return null;
        }
    }
}