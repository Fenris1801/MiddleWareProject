using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ProxyCache
{
    internal class Stations
    {
        [DataMember] public List<Station> everyStations = new List<Station>();
        public Stations(string contract)
        {
            var apiKey = ConfigurationManager.AppSettings["JcDecauxApiKey"];

            var url = $"https://api.jcdecaux.com/vls/v3/stations?contract={Uri.EscapeDataString(contract)}&apiKey={apiKey}";

            var req = new HttpClient();

            var array = JArray.Parse(req.GetStringAsync(url).Result);

            foreach (var s in array)
            {
                int number = s["number"].Value<int>();
                string contractName = s["contractName"].Value<string>();
                string name = s["name"].Value<string>();

                var addr = new AddressPoint
                {
                    lat = s["position"]["latitude"].Value<double>(),
                    lon = s["position"]["longitude"].Value<double>()
                };

                var availableBikes = s["totalStands"]["availabilities"]["bikes"].Value<int>();
                var availableSpots = s["totalStands"]["availabilities"]["stands"].Value<int>();

                everyStations.Add(new Station(number, name, contractName, addr, availableBikes, availableSpots));
            }
        }
    }
}
