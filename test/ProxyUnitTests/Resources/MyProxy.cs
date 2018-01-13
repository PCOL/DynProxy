namespace ProxyUnitTests.Resources
{
    using System;
    using System.Reflection;
    using Proxy;

    public class MyProxy
        : Proxy<IMyProxy>
    {
        protected override object Invoke(MethodInfo methodInfo, object[] arguments)
        {
Console.WriteLine("Calling method: {0}", methodInfo.Name);
Console.WriteLine("Args count: {0}", arguments.Length);
Console.WriteLine("Arg_0: {0}", arguments[0]);
Console.WriteLine("Arg_1: {0}", arguments[1]);

            if (methodInfo.Name == "Add" &&
                methodInfo.ReturnType == typeof(int))
            {
                var returnValue = ProxyAdd((int)arguments[0], (int)arguments[1]);

Console.WriteLine("Return Value: {0}", returnValue);

                return returnValue;
            }

            throw new NotSupportedException();
        }


        private int ProxyAdd(int first, int second)
        {
            return first + second;
        }
    }
}