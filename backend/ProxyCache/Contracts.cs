using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace ProxyCache
{
    public class Contracts
    {
        // api key jdeco: 212c5b75d766b9bbc8571d0499a2c1612a7a88c0
        [DataMember] public List<Contract> AllContracts = new List<Contract>();
        public Contracts(string address)
        {
            var url = $"https://api.jcdecaux.com/vls/v3/contracts?apiKey=212c5b75d766b9bbc8571d0499a2c1612a7a88c0";

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

                AllContracts.Add(new Contract
                {
                    ContractName = name,
                    Cities = (cities != null) ? new HashSet<string>(cities, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                });
            }
        }
    }
}
