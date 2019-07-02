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

namespace DynProxy
{
    using System;
    using System.Linq;
    using System.Reflection.Emit;
    using FluentIL;

    /// <summary>
    /// Represent contextual data used by the <see cref="ProxyTypeGenerator"/>.
    /// </summary>
    internal class ProxyBuilderContext
        : IProxyBuilderContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyBuilderContext"/> class.
        /// </summary>
        /// <param name="typeBuilder">The <see cref="TypeBuilder"/> being use to create the type.</param>
        /// <param name="newType">The new type being built.</param>
        /// <param name="baseType">The base type being built on.</param>
        /// <param name="currentDependencyScope">The current dependency injection scope</param>
        /// <param name="baseObjectField">The <see cref="FieldBuilder"/> that holds the base type instance.</param>
        /// <param name="dependencyResolverField">The <see cref="FieldBuilder"/> that holds the <see cref="IServiceProvider"/> instance.</param>
        /// <param name="ctorBuilder">The <see cref="ConstructorBuilder"/> for the types constructor.</param>
        public ProxyBuilderContext(
            ITypeBuilder typeBuilder,
            Type newType,
            Type baseType,
            IFieldBuilder baseObjectField,
            IConstructorBuilder ctorBuilder = null)
            : this(typeBuilder,
                newType,
                new Type[] { baseType },
                baseObjectField,
                ctorBuilder)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyBuilderContext"/> class.
        /// </summary>
        /// <param name="typeBuilder">The <see cref="TypeBuilder"/> being use to create the type.</param>
        /// <param name="newType">The new type being built.</param>
        /// <param name="baseTypes">The base types being built on.</param>
        /// <param name="baseObjectField">The <see cref="FieldBuilder"/> that holds the base type instance.</param>
        /// <param name="ctorBuilder">The <see cref="ConstructorBuilder"/> for the types constructor.</param>
        public ProxyBuilderContext(
            ITypeBuilder typeBuilder,
            Type newType,
            Type[] baseTypes,
            IFieldBuilder baseObjectField,
            IConstructorBuilder ctorBuilder = null)
        {
            this.TypeBuilder = typeBuilder;
            this.NewType = newType;
            this.BaseTypes = baseTypes;
            this.BaseObjectField = baseObjectField;
            this.ConstructorBuilder = ctorBuilder;
        }

        /// <summary>
        ///  Gets the <see cref="ITypeBuilder"/>
        /// </summary>
        public ITypeBuilder TypeBuilder { get; }

        /// <summary>
        /// Gets the new type.
        /// </summary>
        public Type NewType { get; }

        /// <summary>
        /// Gets the base type.
        /// </summary>
        public Type BaseType
        {
            get
            {
                return this.BaseTypes[0];
            }
        }

        /// <summary>
        /// Gets the base types.
        /// </summary>
        public Type[] BaseTypes { get; }

        /// <summary>
        /// Gets the <see cref="FieldBuilder"/> which will contain the base object instance.
        /// </summary>
        public IFieldBuilder BaseObjectField { get; }

        /// <summary>
        /// Gets the <see cref="ConstructorBuilder"/> used to construct the new object.
        /// </summary>
        public IConstructorBuilder ConstructorBuilder { get; }

        /// <summary>
        /// Does the type build implement a given interface type
        /// </summary>
        /// <param name="ifaceType">Interface type.</param>
        /// <returns>True if it does; otherwise false.</returns>
        public bool DoesTypeBuilderImplementInterface(Type ifaceType)
        {
            return this.TypeBuilder
                .Define()
                .GetInterfaces()
                .FirstOrDefault(type => ifaceType == type) != null;
        }
    }
}
