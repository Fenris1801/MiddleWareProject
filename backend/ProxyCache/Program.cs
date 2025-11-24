using System;
using System.ServiceModel;

namespace ProxyCache
{
    class Program
    {
        static void Main()
        {

            ServiceHost host = new ServiceHost(typeof(ProxyCache));

            host.Open();

            Console.WriteLine("Service is host at " + DateTime.Now.ToString());
            Console.WriteLine("Host is running... Press <Enter> key to stop");
            Console.ReadLine();

        }
    }
}