namespace Proxy.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Reflection extension methods.
    /// </summary>
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// Gets a value indicating whether or not the <see cref="MethodInfo"/> is a property method.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>True if the <see cref="MethodInfo"/> is a property method; otherwise false.</returns>
        public static bool IsProperty(this MethodInfo methodInfo)
        {
            return methodInfo.IsPropertyGet() || methodInfo.IsPropertySet();
        }

        /// <summary>
        /// Gets a value indicating whether or not the <see cref="MethodInfo"/> is a property get method.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>True if the <see cref="MethodInfo"/> is a property get method; otherwise false.</returns>
        public static bool IsPropertyGet(this MethodInfo methodInfo)
        {
            return methodInfo != null && methodInfo.Name.StartsWith("get_");
        }

        /// <summary>
        /// Gets a value indicating whether or not the <see cref="MethodInfo"/> is a property set method.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>True if the <see cref="MethodInfo"/> is a property set method; otherwise false.</returns>
        public static bool IsPropertySet(this MethodInfo methodInfo)
        {
            return methodInfo != null && methodInfo.Name.StartsWith("set_");
        }

        /// <summary>
        /// Returns the name of the get method if the given <see cref="MemberInfo"/> is a property.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The name of the get method if the <see cref="MemberInfo"/> is a property; otherwise null.</returns>
        public static string PropertyGetName(this MemberInfo memberInfo)
        {
            if (memberInfo != null &&
                memberInfo is PropertyInfo)
            {
                return string.Format("get_{0}", memberInfo.Name);
            }

            return null;
        }

        /// <summary>
        /// Returns the name of the set method if the given <see cref="MemberInfo"/> is a property.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The name of the set method if the <see cref="MemberInfo"/> is a property; otherwise null.</returns>
        public static string PropertySetName(this MemberInfo memberInfo)
        {
            if (memberInfo != null &&
                memberInfo is PropertyInfo)
            {
                return string.Format("set_{0}", memberInfo.Name);
            }

            return null;
        }

        /// <summary>
        /// Returns the <see cref="PropertyInfo"/> if the given <see cref="MethodInfo"/> is a property method.
        /// </summary>
        /// <param name="methodInfo">A method info.</param>
        /// <returns>The <see cref="PropertyInfo"/> if the <see cref="MethodInfo"/> is a property method; otherwise null.</returns>
        public static PropertyInfo GetPropertyInfo(this MethodInfo methodInfo)
        {
            if (methodInfo.IsProperty() == true)
            {
                return methodInfo.ReflectedType.GetProperty(methodInfo.Name.Substring(4));
            }

            return null;
        }
    }
}
