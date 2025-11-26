using CoreWCF;

namespace ProxyCacheServer
{
    [ServiceContract]
    public interface IProxyCacheService
    {
        [OperationContract]
        Task<List<Station>> GetStationsAsync(string contract);
        [OperationContract]
        Task<Contracts> GetContractsAsync();
        [OperationContract]
        Task<string> GetRouteAsync(bool isBike, AddressPoint from, AddressPoint to);
    }
}
