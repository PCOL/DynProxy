namespace DynProxyUnitTests.Resources
{
    public interface IMyProxy
    {
        string StringProperty { get; set; }

        bool? BooleanProperty { get; set; }

        int Add(int first, int second);


        bool TryGetStringProperty(out string value);

        bool TryGetBooleanProperty(out bool value);
    }
}