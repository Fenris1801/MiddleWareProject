using System;
using HeavyClient.Commands;

namespace HeavyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Heavy SOAP Client - ProxyCache");
            Console.WriteLine("Endpoint: http://localhost:8733/ProxyCacheService");
            Console.WriteLine();

            var commands = new CommandDispatcher();
            commands.Register("1", new TestConnectionCommand());
            commands.Register("2", new ListContractsCommand());
            commands.Register("3", new ListStationsCommand());
            commands.Register("4", new ComputeRouteCommand());

            while (true)
            {
                Console.WriteLine("Select an option:");
                Console.WriteLine("1. Test SOAP connection");
                Console.WriteLine("2. List contracts");
                Console.WriteLine("3. List stations from contract");
                Console.WriteLine("4. Compute route");
                Console.WriteLine("0. Exit");
                Console.Write("> ");

                var choice = Console.ReadLine()?.Trim();

                if (choice == "0")
                {
                    Console.WriteLine("Exiting.");
                    return;
                }

                if (!commands.Execute(choice))
                {
                    Console.WriteLine("Invalid command.");
                }

                Console.WriteLine();
            }
        }
    }
}
