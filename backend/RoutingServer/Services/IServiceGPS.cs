using CoreWCF;
using CoreWCF.Web;

namespace RoutingServer
{
    [ServiceContract]
    public interface IServiceGPS
    {
        [OperationContract]
        [WebGet(UriTemplate = "/itinerary?fromLat={originLat}&fromLon={originLon}&toLat={destLat}&toLon={destLon}", ResponseFormat = WebMessageFormat.Json)]
        Task<string> GetItinerary(double originLat, double originLon, double destLat, double destLon);
    }
}
