using System;
using System.Threading.Tasks;

namespace PortScanner.Example
{
    class Program
    {
        static void OnOpenPortScanned(ushort port)
        {
            Console.WriteLine($"Open Port: {port}");
        }

        static void OnClosedPortScanned(ushort port)
        {
            //Console.WriteLine($"Closed Port: {port}");
        }

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }
        private static async Task MainAsync()
        {
            PortScanner scanner = new PortScanner("google.com", 1, 65535);
            scanner.OpenPortScanned += OnOpenPortScanned;
            scanner.ClosedPortScanned += OnClosedPortScanned;

            await scanner.ScanAsync(1000);

            Console.ReadLine();
        }
    }
}
