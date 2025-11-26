using System.Runtime.Serialization;

namespace ProxyCacheServer
{
    [DataContract]
    public class Contract
    {
        [DataMember] public required string ContractName;
        [DataMember] public required HashSet<string> Cities;
    }
}
