using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace ProxyCache
{
    [DataContract]
    public class Contracts
    {
        [DataMember] public List<Contract> everyContracts = new List<Contract>();
        public Contracts(string ContractName)
        {

            var apiKey = ConfigurationManager.AppSettings["JcDecauxApiKey"];
            var url = $"https://api.jcdecaux.com/vls/v3/contracts?apiKey={apiKey}";

            var req = new HttpClient();

            var Array = JArray.Parse(req.GetStringAsync(url).Result);

            foreach (var c in Array)
            {
                var name = (string)c["name"];
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var cities = c["cities"]?.Select(v => (string)v).ToList();
                var country = (string)c["country_code"];

                if (!string.IsNullOrWhiteSpace(country) && !string.Equals(country, "FR"))
                    continue;

                everyContracts.Add(new Contract
                {
                    ContractName = name,
                    Cities = (cities != null) ? new HashSet<string>(cities, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                });
            }
        }
    }
}
