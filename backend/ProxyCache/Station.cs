using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCache
{
    [DataContract]
    public class Station
    {
        [DataMember] public int Number { get; set; }
        [DataMember] public string ContractName { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public AddressPoint Address { get; set; }
        [DataMember] public int NbBikes { get; set; }
        [DataMember] public int NbSpots { get; set; }

        public Station(int number, string contractName, string name, AddressPoint address, int nbBikes, int nbSpots)
        {
            Number = number;
            ContractName = contractName;
            Name = name;
            Address = address;
            NbBikes = nbBikes;
            NbSpots = nbSpots;
        }
    }
}
