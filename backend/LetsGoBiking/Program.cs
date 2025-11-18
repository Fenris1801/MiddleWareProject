using LetsGoBiking;
using System;
using System.ServiceModel.Web;

namespace ServerGPS
{
    class Program
    {
        static void Main()
        {
            using (WebServiceHost host = new WebServiceHost(typeof(ServiceGPS), new Uri("http://localhost:8080/ServerGPS")))
            {
                host.Open();
            }
        }
    }
}