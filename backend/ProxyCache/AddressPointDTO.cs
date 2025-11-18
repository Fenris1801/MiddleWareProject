using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCache
{
    [DataContract]
    public class AddressPoint
    {
        [DataMember] public string label { get; set; }
        [DataMember] public double lat { get; set; }
        [DataMember] public double lon { get; set; }
    }
}
