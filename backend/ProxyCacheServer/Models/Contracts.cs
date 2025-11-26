using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace ProxyCacheServer
{
    [DataContract]
    public class Contracts : IProxyCacheItem
    {
        public static HttpClient Client { get; set; }

        [DataMember] public List<Contract> Items { get; set; } = new();

        public async Task FillFromWebAsync(params string[] args)
        {
            var apiKey = Environment.GetEnvironmentVariable("JcDecauxApiKey")
                       ?? throw new NullReferenceException("JcDecauxApiKey env variable not found");

            var url = $"https://api.jcdecaux.com/vls/v3/contracts?apiKey={apiKey}";

            var Array = JArray.Parse(await Client.GetStringAsync(url));

            foreach (var c in Array)
            {
                var name = (string)c["name"];
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var cities = c["cities"]?.Select(v => (string)v).ToList();
                var country = (string)c["country_code"];

                if (!string.IsNullOrWhiteSpace(country) && !string.Equals(country, "FR"))
                    continue;

                Items.Add(new Contract
                {
                    ContractName = name,
                    Cities = (cities != null) ? new HashSet<string>(cities, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                });
            }
        }
    }
}
