using System.Threading.Tasks;
using ServiceReference;

namespace HeavyClient.Services
{
    public class ProxyService
    {
        private readonly ProxyCacheServiceClient _client;

        public ProxyService()
        {
            _client = new ProxyCacheServiceClient(
                ProxyCacheServiceClient.EndpointConfiguration.BasicHttpBinding_IProxyCacheService
            );
        }

        public Task<Contracts> GetContractsAsync()
        {
            return _client.GetContractsAsync();
        }

        public Task<Station[]> GetStationsAsync(string contract)
        {
            return _client.GetStationsAsync(contract);
        }

        public Task<string> GetRouteAsync(bool isBike, AddressPoint from, AddressPoint to)
        {
            return _client.GetRouteAsync(isBike, from, to);
        }
    }
}
