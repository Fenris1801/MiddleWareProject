using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Newtonsoft.Json;
using ProxyCache;

namespace LetsGoBiking { 
    public class ServiceGPS : IServiceGPS 
    {
        public string GetItinerary(AddressPoint from, AddressPoint to)
        { 

            if (from == null || to == null)
                return "Invalid request";

            double originLat = from.lat;
            double originLon = from.lon;
            double destLat = to.lat;
            double destLon = to.lon;

            //TODO pas encore connecté au proxy
            proxy = new ProxyCache.ProxyCache();

            var contracts = proxy.GetContracts();
            var stations = new List<Station>();
            var stationsWithBikes = new List<Station>();
            var stationsWithSpots = new List<Station>();

            foreach (var contract in contracts.AllContracts)
            {
                stations.AddRange(proxy.GetStations(contract.ContractName));
            }

            foreach (var station in stations)
            {
                if (station != null && station.NbBikes > 0)
                {
                    stationsWithBikes.Add(station);
                }
                if (station != null && station.NbSpots > 0)
                {
                    stationsWithSpots.Add(station);
                }
            }

            Station nearestBike = null;
            double minDist = double.MaxValue;
            for (int i = 0; i < stationsWithBikes.Count; i++)
            {
                double dist = CalcDistance(stationsWithBikes[i].Address, from);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestBike = stationsWithBikes[i];
                }
            }

            Station nearestSpot = null;
            minDist = double.MaxValue;
            for (int i = 0; i < stationsWithSpots.Count; i++)
            {
                double dist = CalcDistance(stationsWithSpots[i].Address, to);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestSpot = stationsWithSpots[i];
                }
            }
            //TODO faire le json de a pied a la station, de la station a la station, de la station a destination
            var exemplepiedAStation = createJson(false, from, nearestBike.Address);

            //TODO checker les temps,si plus rapide a velo ou a pied


            return JsonConvert.SerializeObject(results);
        }

        private string createJson(bool isBike, AddressPoint from, AddressPoint to)
        {
            return $@"
            {{
                ""isBike"": ""{isBike}"",
                ""from"": {{
                    ""Latitude"": {from.lat.ToString(CultureInfo.InvariantCulture)},
                    ""Longitude"": {from.lon.ToString(CultureInfo.InvariantCulture)}
                }},
                ""to"": {{
                    ""Latitude"": {to.lat.ToString(CultureInfo.InvariantCulture)},
                    ""Longitude"": {to.lon.ToString(CultureInfo.InvariantCulture)}
                }}
            }}";
        }

        private double CalcDistance(AddressPoint from, AddressPoint to)
        {
            double latDiff = from.lat - to.lat;
            double lonDiff = from.lon - to.lon;
            return Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
        }
    }
}