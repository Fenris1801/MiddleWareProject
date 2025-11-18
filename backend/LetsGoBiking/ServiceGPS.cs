using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxyCache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace LetsGoBiking { 
    public class ServiceGPS : IServiceGPS 
    {
        public string GetItinerary(double originLat, double originLon, double destLat, double destLon)
        {
            Console.WriteLine("Début boucle");
            var from = new AddressPoint
            {
                label = "from",
                lat = originLat,
                lon = originLon
            };

            var to = new AddressPoint
            {
                label = "to",
                lat = destLat,
                lon = destLon
            };

            var proxy = new ProxyCache.ProxyCache();

            var contracts = proxy.GetContracts();
            var stations = new List<Station>();
            var stationsWithBikes = new List<Station>();
            var stationsWithSpots = new List<Station>();

            foreach (var contract in contracts.everyContracts)
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

            if (nearestBike == null || nearestSpot == null)
            {
                // Pas de solution vélo, on marche
                return proxy.GetRoute(false, from, to);
            }

            var getBike = proxy.GetRoute(false, from, nearestBike.Address);
            var getToSpot = proxy.GetRoute(true, nearestBike.Address, nearestSpot.Address);
            var getTodestination = proxy.GetRoute(false, nearestSpot.Address, to);

            var getTodestinationOnFoot = proxy.GetRoute(false, from, to);

            System.Diagnostics.Debug.WriteLine("getBike is null? " + (getBike == null));
            System.Diagnostics.Debug.WriteLine("getToSpot is null? " + (getToSpot == null));
            System.Diagnostics.Debug.WriteLine("getTodestination is null? " + (getTodestination == null));
            System.Diagnostics.Debug.WriteLine("getTodestinationOnFoot is null? " + (getTodestinationOnFoot == null));

            var TimeToWalk = CalcTime(getTodestinationOnFoot);

            var TimeToRide = CalcTime(getBike) + CalcTime(getToSpot) + CalcTime(getTodestination);

            if (TimeToWalk < TimeToRide)
            {
                return getTodestinationOnFoot;
            }

            var jBike = JObject.Parse(getBike);
            var jSpot = JObject.Parse(getToSpot);
            var jDest = JObject.Parse(getTodestination);

            JArray allCoords = new JArray();

            void AppendCoords(JObject segment, bool skipFirstPoint)
            {
                var coords = (JArray)segment["features"][0]["geometry"]["coordinates"];
                int startIndex = skipFirstPoint ? 1 : 0;

                for (int i = startIndex; i < coords.Count; i++)
                {
                    allCoords.Add(coords[i]);
                }
            }

            AppendCoords(jBike, false);
            AppendCoords(jSpot, true);
            AppendCoords(jDest, true);

            double totalDistance =
            (double)jBike["features"][0]["properties"]["summary"]["distance"] +
            (double)jSpot["features"][0]["properties"]["summary"]["distance"] +
            (double)jDest["features"][0]["properties"]["summary"]["distance"];

            

        var results = new JObject
        {
        ["type"] = "FeatureCollection",
        ["features"] = new JArray
        {
            new JObject
        {
            ["type"] = "Feature",
            ["properties"] = new JObject
            {
                ["segments"] = new JObject
                {
                    ["distance"] = totalDistance,
                    ["duration"] = TimeToRide,
                }
            },
            ["geometry"] = new JObject
            {
                ["type"] = "LineString",
                ["coordinates"] = allCoords
            }
                }
                }
                };

            return JsonConvert.SerializeObject(results);
        }

        private double CalcDistance(AddressPoint from, AddressPoint to)
        {
            double latDiff = from.lat - to.lat;
            double lonDiff = from.lon - to.lon;
            return Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
        }

        private double CalcTime(string itinetary)
        {
            if (string.IsNullOrWhiteSpace(itinetary))
            {
                throw new ArgumentException("L’itinéraire JSON est null ou vide.", nameof(itinetary));
            }

            return (double)JObject.Parse(itinetary)["features"][0]["properties"]["segments"][0]["duration"];
        }
    }
}