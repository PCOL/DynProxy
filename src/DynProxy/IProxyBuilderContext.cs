namespace DynProxy
{
    using System;
    using FluentIL;

    /// <summary>
    /// Defines the proxy builder context.
    /// </summary>
    public interface IProxyBuilderContext
    {
        /// <summary>
        ///  Gets the <see cref="ITypeBuilder"/>.
        /// </summary>
        ITypeBuilder TypeBuilder { get; }

        /// <summary>
        /// Gets the base type.
        /// </summary>
        Type BaseType { get; }

        /// <summary>
        /// Gets the <see cref="IFieldBuilder"/> which will contain the base object instance.
        /// </summary>
        IFieldBuilder BaseObjectField { get; }
    }
}