namespace DynProxyUnitTests.Resources
{
    using System.Threading.Tasks;

    public interface IAsyncProxy
    {
        Task<string> GetTokenAsync();
    }
}