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
        [WebGet(UriTemplate = "/itinerary?fromLat={originLat}&fromLon={originLon}&toLat={destLat}&toLon={destLon}", ResponseFormat = WebMessageFormat.Json)]

        string GetItinerary(double originLat, double originLon, double destLat, double destLon);
    }
}
