using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCache
{
    [DataContract]
    public class Contract
    {
        [DataMember] public string ContractName;
        [DataMember] public HashSet<string> Cities;
    }
}
