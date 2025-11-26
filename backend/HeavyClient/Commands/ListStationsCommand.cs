using System;
using HeavyClient.Services;

namespace HeavyClient.Commands
{
    public class ListStationsCommand : ICommand
    {
        public string Name => "List stations";

        public void Execute()
        {
            Console.Write("Contract name: ");
            string contract = Console.ReadLine()?.Trim() ?? "";

            try
            {
                var proxy = new ProxyService();
                var stations = proxy.GetStationsAsync(contract).Result;

                Console.WriteLine("Stations: " + stations.Length);

                foreach (var s in stations)
                {
                    Console.WriteLine("#" + s.Number + " - " + s.Name +
                        " | Bikes: " + s.NbBikes +
                        " | Spots: " + s.NbSpots);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
