using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
            
            return stations.toList;
        }

        public Contracts GetContracts()
        {
            return CacheContracts.Get("ContractsJCDecaux");
        }
    }
}
