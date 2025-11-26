using System;
using HeavyClient.Services;

namespace HeavyClient.Commands
{
    public class TestConnectionCommand : ICommand
    {
        public string Name => "Test SOAP connection";

        public void Execute()
        {
            Console.WriteLine("Testing SOAP connection...");

            try
            {
                var proxy = new ProxyService();
                var contracts = proxy.GetContractsAsync().Result;

                Console.WriteLine("Connection OK.");
                Console.WriteLine("Contracts received: " + contracts.Items.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection failed:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
