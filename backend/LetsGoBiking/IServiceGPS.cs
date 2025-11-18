using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using ProxyCache;


namespace LetsGoBiking
{
   [ServiceContract]
    public interface IServiceGPS
    {
        [OperationContract]
        [WebGet(UriTemplate = "/itinerary/{from}/{to}", ResponseFormat = WebMessageFormat.Json)]

        string GetItinerary(AddressPoint from, AddressPoint to);
    }
}
