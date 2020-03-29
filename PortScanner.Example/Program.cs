using System;

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
            Console.WriteLine($"Closed Port: {port}");
        }

        static void Main(string[] args)
        {
            PortScanner scanner = new PortScanner("google.com", 1, 65535);
            scanner.OpenPortScanned += OnOpenPortScanned;
            scanner.ClosedPortScanned += OnClosedPortScanned;

            scanner.ScanAsync();

            Console.ReadLine();
        }
    }
}
