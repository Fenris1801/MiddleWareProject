using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ProxyCache
{
    [ServiceContract]
    public interface IProxyCache
    {
        [OperationContract]
        List<Station> GetStations(string contract);

        [OperationContract]
        Contracts GetContracts();

        [OperationContract]
        string GetRoute(bool isBike,AddressPoint from, AddressPoint to);
    }
}
