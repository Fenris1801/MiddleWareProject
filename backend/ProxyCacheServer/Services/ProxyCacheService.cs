using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace ProxyCacheServer
{
    public class ProxyCacheService : IProxyCacheService
    {
        private readonly GenericProxyCache<Stations> StationsCache;
        private readonly GenericProxyCache<Contracts> ContractsCache;

        private readonly HttpClient http;

        const int STATIONS_CACHE_SECONDS = 60;
        const int CONTRACTS_CACHE_SECONDS = 600;

        public ProxyCacheService(IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            StationsCache = new GenericProxyCache<Stations>(cache);
            ContractsCache = new GenericProxyCache<Contracts>(cache);

            this.http = httpClientFactory.CreateClient();

            Stations.Client = httpClientFactory.CreateClient();
            Contracts.Client = httpClientFactory.CreateClient();
        }

        public async Task<List<Station>> GetStationsAsync(string contract)
        {
            var stations = await StationsCache.GetAsync(contract, STATIONS_CACHE_SECONDS, contract);
            return stations.Items;
        }

        public async Task<Contracts> GetContractsAsync()
        {
            return await ContractsCache.GetAsync("ContractsJCDecaux");
        }

        public async Task<string> GetRouteAsync(bool isBike, AddressPoint from, AddressPoint to)
        {
            string mode = isBike ? "cycling-regular" : "foot-walking";
            string fromUrl = from.Lon.ToString(CultureInfo.InvariantCulture) + "," + from.Lat.ToString(CultureInfo.InvariantCulture);
            string toUrl = to.Lon.ToString(CultureInfo.InvariantCulture) + "," + to.Lat.ToString(CultureInfo.InvariantCulture);

            var apiKey = Environment.GetEnvironmentVariable("OpenRouteApiKey")
                       ?? throw new NullReferenceException("OpenRouteApiKey env variable not found");

            string url = $"https://api.openrouteservice.org/v2/directions/{mode}?api_key={apiKey}&start={fromUrl}&end={toUrl}";

            var response = await http.GetStringAsync(url);

            return response;
        }
    }
}
