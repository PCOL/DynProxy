namespace DynProxyUnitTests
{
    public interface IGenericProxy<T>
    {
        T Property { get; }

        void SetProperty(T value );
    }
}