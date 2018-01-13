/*
MIT License

Copyright (c) 2018 P Collyer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Proxy.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Contains <see cref="TypeFactory"/> extension methods.
    /// </summary>
    internal static class TypeFactoryExtension
    {
        private static readonly MethodInfo GetTypeMethod = typeof(TypeFactory).GetMethod("GetType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(bool) }, null);

        /// <summary>
        /// Emits IL to load the type for a given type name onto the evaluation stack.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="typeName">The <see cref="LocalBuilder"/> containing the type name.</param>
        /// <param name="dynamicOnly">A value indicating whether or not to only check for dynamically generated types.</param>
        public static void EmitGetType(this ILGenerator ilGen, LocalBuilder typeName, bool dynamicOnly = false)
        {
            ilGen.Emit(OpCodes.Ldloc_S, typeName);
            ilGen.Emit(dynamicOnly == false ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            ilGen.Emit(OpCodes.Call, GetTypeMethod);
        }
    }
}
