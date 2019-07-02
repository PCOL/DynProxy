namespace DynProxy
{
    using System;
    using FluentIL;

    public interface IProxyBuilderContext
    {
        /// <summary>
        ///  Gets the <see cref="ITypeBuilder"/>
        /// </summary>
        ITypeBuilder TypeBuilder { get; }

        /// <summary>
        /// Gets the base type.
        /// </summary>
        Type BaseType { get; }

        /// <summary>
        /// Gets the <see cref="FieldBuilder"/> which will contain the base object instance.
        /// </summary>
        IFieldBuilder BaseObjectField { get; }
    }
}