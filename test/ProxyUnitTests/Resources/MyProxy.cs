namespace ProxyUnitTests.Resources
{
    using System;
    using System.Reflection;
    using Proxy;

    public class MyProxy
        : Proxy<IMyProxy>
    {
        private string stringProperty;

        private bool? booleanProperty;

        protected override object Invoke(MethodInfo methodInfo, object[] arguments)
        {
            if (methodInfo.Name == "get_StringProperty")
            {
                return this.stringProperty;
            }
            else if (methodInfo.Name == "set_StringProperty")
            {
                this.stringProperty = (string)arguments[0];
                return null;
            }
            else if (methodInfo.Name == "get_BooleanProperty")
            {
                return this.stringProperty;
            }
            else if (methodInfo.Name == "set_BooleanProperty")
            {
                this.booleanProperty = (bool?)arguments[0];
                return null;
            }
            else if (methodInfo.Name == "TryGetStringProperty")
            {
                if (this.stringProperty != null)
                {
                    arguments[0] = this.stringProperty;
                    return true;
                }

                return false;
            }
            else if (methodInfo.Name == "TryGetBooleanProperty")
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