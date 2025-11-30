
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceReference;

namespace RoutingServer
{
    public class ServiceGPS : IServiceGPS
    {
        public async Task<string> GetItinerary(double originLat, double originLon, double destLat, double destLon)
        {
            var from = new AddressPoint
            {
                Label = "from",
                Lat = originLat,
                Lon = originLon
            };

            var to = new AddressPoint
            {
                Label = "to",
                Lat = destLat,
                Lon = destLon
            };

            var proxy = new ProxyCacheServiceClient();

            var contracts = await proxy.GetContractsAsync();
            var stations = new List<Station>();
            var stationsWithBikes = new List<Station>();
            var stationsWithSpots = new List<Station>();

            foreach (var contract in contracts.Items)
            {
                stations.AddRange(await proxy.GetStationsAsync(contract.ContractName));
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
                string routes = await proxy.GetRouteAsync(false, from, to);
                proxy.Close();
                return routes;
            }

            var getBike = await proxy.GetRouteAsync(false, from, nearestBike.Address);
            var getToSpot = await proxy.GetRouteAsync(true, nearestBike.Address, nearestSpot.Address);
            var getTodestination = await proxy.GetRouteAsync(false, nearestSpot.Address, to);

            var getTodestinationOnFoot = await proxy.GetRouteAsync(false, from, to);

            System.Diagnostics.Debug.WriteLine("getBike is null? " + (getBike == null));
            System.Diagnostics.Debug.WriteLine("getToSpot is null? " + (getToSpot == null));
            System.Diagnostics.Debug.WriteLine("getTodestination is null? " + (getTodestination == null));
            System.Diagnostics.Debug.WriteLine("getTodestinationOnFoot is null? " + (getTodestinationOnFoot == null));

            var TimeToWalk = CalcTime(getTodestinationOnFoot);

            var TimeToRide = CalcTime(getBike) + CalcTime(getToSpot) + CalcTime(getTodestination);

            if (TimeToWalk < TimeToRide)
            {
                var obj = JObject.Parse(getTodestinationOnFoot);
                obj["usebike"] = false;
                proxy.Close();
                return obj.ToString();
            }

            var jBike = JObject.Parse(getBike);
            var jSpot = JObject.Parse(getToSpot);
            var jDest = JObject.Parse(getTodestination);

            JArray allCoords = new JArray();

            void AppendCoords(JObject baseSegments, JObject addedSegment)
            {
                var baseCoords = (JArray)baseSegments["features"][0]["geometry"]["coordinates"];
                var coords = (JArray)addedSegment["features"][0]["geometry"]["coordinates"];

                for (int i = 1; i < coords.Count; i++)
                {
                    baseCoords.Add(coords[i]);
                }
            }

            void Merge(JObject baseSegments, JObject addedSegment)
            {
                var baseArray = (JArray)baseSegments["features"][0]["properties"]["segments"];
                var addedArray = (JArray)addedSegment["features"][0]["properties"]["segments"];

                foreach (var seg in addedArray)
                {
                    baseArray.Add(seg);
                }
            }

            var result = jBike;

            AppendCoords(result, jSpot);
            AppendCoords(result, jDest);

            result["features"][0]["properties"]["summary"]["distance"] =
            (double)jBike["features"][0]["properties"]["summary"]["distance"] +
            (double)jSpot["features"][0]["properties"]["summary"]["distance"] +
            (double)jDest["features"][0]["properties"]["summary"]["distance"];
            jBike["features"][0]["properties"]["summary"]["duration"] = TimeToRide;
            result["features"][0]["pickupStation"] = JToken.FromObject(new
            {
                Address = new
                {
                    lat = nearestBike.Address.Lat,
                    lon = nearestBike.Address.Lon
                }
            });

            result["features"][0]["dropoffStation"] = JToken.FromObject(new
            {
                Address = new
                {
                    lat = nearestSpot.Address.Lat,
                    lon = nearestSpot.Address.Lon
                }
            });

            result["usebike"] = true;

            Merge(result, jSpot);
            Merge(result, jDest);

            proxy.Close();
            return JsonConvert.SerializeObject(result);
        }

        private double CalcDistance(AddressPoint from, AddressPoint to)
        {
            double latDiff = from.Lat - to.Lat;
            double lonDiff = from.Lon - to.Lon;
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
