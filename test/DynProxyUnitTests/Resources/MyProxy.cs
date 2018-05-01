namespace DynProxyUnitTests.Resources
{
    using System;
    using System.Reflection;
    using DynProxy;

    public class MyProxy
        : Proxy<IMyProxy>
    {
        private string stringProperty;

        private bool? booleanProperty;

        protected override object Invoke(MethodInfo methodInfo, object[] arguments)
        {
            if (methodInfo.Name == $"get_{nameof(IMyProxy.StringProperty)}")
            {
                return this.stringProperty;
            }
            else if (methodInfo.Name == $"set_{nameof(IMyProxy.StringProperty)}")
            {
                this.stringProperty = (string)arguments[0];
                return null;
            }
            else if (methodInfo.Name == $"get_{nameof(IMyProxy.BooleanProperty)}")
            {
                return this.stringProperty;
            }
            else if (methodInfo.Name == $"set_{nameof(IMyProxy.BooleanProperty)}")
            {
                this.booleanProperty = (bool?)arguments[0];
                return null;
            }
            else if (methodInfo.Name == nameof(IMyProxy.TryGetStringProperty))
            {
                if (this.stringProperty != null)
                {
                    arguments[0] = this.stringProperty;
                    return true;
                }

                return false;
            }
            else if (methodInfo.Name == nameof(IMyProxy.TryGetBooleanProperty))
            {
                if (this.booleanProperty.HasValue == true)
                {
                    arguments[0] = this.booleanProperty;
                    return true;
                }

                return false;
            }
            else if (methodInfo.Name == "Add" &&
                methodInfo.ReturnType == typeof(int))
            {
                return ProxyAdd((int)arguments[0], (int)arguments[1]);
            }

            throw new NotSupportedException();
        }


        private int ProxyAdd(int first, int second)
        {
            return first + second;
        }
    }
}