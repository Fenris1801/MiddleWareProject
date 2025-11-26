using System;
using HeavyClient.Services;
using ServiceReference;

namespace HeavyClient.Commands
{
    public class ComputeRouteCommand : ICommand
    {
        public string Name => "Compute route";

        public void Execute()
        {
            Console.Write("Mode (bike or walk): ");
            string mode = Console.ReadLine()?.Trim().ToLower() ?? "";
            bool isBike = mode == "bike";

            Console.WriteLine("Enter origin:");
            var from = ReadPoint();

            Console.WriteLine("Enter destination:");
            var to = ReadPoint();

            try
            {
                var proxy = new ProxyService();
                var route = proxy.GetRouteAsync(isBike, from, to).Result;

                Console.WriteLine("Route:");
                Console.WriteLine(route);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(ex.Message);
            }
        }

        private AddressPoint ReadPoint()
        {
            Console.Write("Label: ");
            string label = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Latitude: ");
            double lat = double.Parse(Console.ReadLine()?.Replace(",", ".") ?? "0");

            Console.Write("Longitude: ");
            double lon = double.Parse(Console.ReadLine()?.Replace(",", ".") ?? "0");

            return new AddressPoint
            {
                Label = label,
                Lat = lat,
                Lon = lon
            };
        }
    }
}
