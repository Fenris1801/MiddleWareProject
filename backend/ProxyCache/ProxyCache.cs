using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ProxyCache
{
    public class ProxyCache : IProxyCache
    {

        private static GenericProxyCache<Stations> CacheStations = new GenericProxyCache<Stations>("Stations");
        private static GenericProxyCache<Contracts> CacheContracts = new GenericProxyCache<Contracts>("Contracts");


        public List<Station> GetStations(string contract)
        {
            
            var stations = CacheStations.Get(contract, 60);
            
            return stations.everyStations;
        }

        public Contracts GetContracts()
        {
            return CacheContracts.Get("ContractsJCDecaux");
        }

        public string GetRoute(bool isBike, AddressPoint from, AddressPoint to)
        {

            string mode = isBike ? "cycling-regular" : "foot-walking";
            string fromUrl = from.lon.ToString(CultureInfo.InvariantCulture) + "," + from.lat.ToString(CultureInfo.InvariantCulture);
            string toUrl = to.lon.ToString(CultureInfo.InvariantCulture) + "," + to.lat.ToString(CultureInfo.InvariantCulture);
            var apiKey = ConfigurationManager.AppSettings["OpenRouteApiKey"];
            string url = $"https://api.openrouteservice.org/v2/directions/{mode}?api_key={apiKey}&start={fromUrl}&end={toUrl}";

            var req = new HttpClient();

            var response = req.GetStringAsync(url).Result;

            return response;
        }
    }
}
