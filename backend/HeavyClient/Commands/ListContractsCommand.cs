using System;
using HeavyClient.Services;

namespace HeavyClient.Commands
{
    public class ListContractsCommand : ICommand
    {
        public string Name => "List contracts";

        public void Execute()
        {
            Console.WriteLine("Fetching contracts...");

            try
            {
                var proxy = new ProxyService();
                var result = proxy.GetContractsAsync().Result;

                foreach (var c in result.Items)
                {
                    var cities = c.Cities != null ? string.Join(", ", c.Cities) : "";
                    Console.WriteLine("- " + c.ContractName + " (" + cities + ")");
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
