using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace ProxyCacheServer
{
    [DataContract]
    public class Stations : IProxyCacheItem
    {
        public static HttpClient Client { get; set; }

        [DataMember]
        public List<Station> Items { get; set; } = new();

        public async Task FillFromWebAsync(params string[] args)
        {
            var apiKey = Environment.GetEnvironmentVariable("JcDecauxApiKey")
                       ?? throw new NullReferenceException("JcDecauxApiKey env variable not found");

            var url = $"https://api.jcdecaux.com/vls/v3/stations?contract={Uri.EscapeDataString(args[0])}&apiKey={apiKey}";

            var array = JArray.Parse(await Client.GetStringAsync(url));

            foreach (var s in array)
            {
                int number = s["number"].Value<int>();
                string contractName = s["contractName"].Value<string>();
                string name = s["name"].Value<string>();

                var addr = new AddressPoint
                {
                    Label = name,
                    Lat = s["position"]["latitude"].Value<double>(),
                    Lon = s["position"]["longitude"].Value<double>()
                };

                var availableBikes = s["totalStands"]["availabilities"]["bikes"].Value<int>();
                var availableSpots = s["totalStands"]["availabilities"]["stands"].Value<int>();

                Items.Add(new Station(number, name, contractName, addr, availableBikes, availableSpots));
            }
        }
    }
}
