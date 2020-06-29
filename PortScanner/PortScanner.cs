using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PortScanner
{
    public class PortScanner
    {
        public delegate void ScannedPortEventHandler(ushort port);

        public event ScannedPortEventHandler ClosedPortScanned;
        public event ScannedPortEventHandler OpenPortScanned;
        public event ScannedPortEventHandler FailedPortScanned;

        public delegate void ScannerFinishedEventHandler();
        public event ScannedPortEventHandler ScannerFinished;

        private List<ushort> _openPorts;
        private List<ushort> _closedPorts;

        private string _host;
        private ushort _scanPortMin;
        private ushort _scanPortMax;

        private ushort _portsScanned;

        public PortScanner(string host, ushort rangeMin, ushort rangeMax)
        {
            if (rangeMin > rangeMax)
                throw new ArgumentException($"rangeMind({rangeMin}) can not be higher than rangeMax({rangeMax})!");

            _host = host;
            _scanPortMin = rangeMin;
            _scanPortMax = rangeMax;
        }

        public void Scan()
        {
            _openPorts = new List<ushort>();
            _closedPorts = new List<ushort>();

            for (ushort i = _scanPortMin; i <= _scanPortMax; i++)
            {
                CheckPort(i);
            }
        }

        private void CheckPort(ushort port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(_host, port);

                //Connect succeeded
                _openPorts.Add(port);
                OpenPortScanned?.Invoke(port);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionRefused ||
                    e.SocketErrorCode == SocketError.TimedOut)
                {
                    _closedPorts.Add(port);
                    ClosedPortScanned?.Invoke(port);
                }
                else if (e.SocketErrorCode == SocketError.AccessDenied)
                {
                    _openPorts.Add(port);
                    OpenPortScanned?.Invoke(port);
                }
                else
                {
                    Console.Write("Unhandled SocketError: ");
                    Console.Write(Enum.GetName(typeof(SocketError), e.SocketErrorCode));
                    Console.Write("\n");
                }
            }
            finally
            {
                if (socket.Connected)
                    socket.Disconnect(false);

                socket.Close();
            }
        }

        public async Task ScanAsync(int ConcurrentTaskCount)
        {
            _openPorts = new List<ushort>();
            _closedPorts = new List<ushort>();

            ushort counter = _scanPortMin;
            List<Task> tasks = new List<Task>(ConcurrentTaskCount);

            //Init the list
            for (int a = 0; a < ConcurrentTaskCount; a++)
                tasks.Add(Task.Run(() => { }));

            while (counter <= _scanPortMax)
            {
                for(int a = 0; a < ConcurrentTaskCount; a++)
                {
                    if (tasks[a] == null || tasks[a].IsCompleted)
                    {
                        if (counter <= _scanPortMax)
                        {
                            tasks[a] = CheckPortAsync(counter);
                            counter++;
                        }
                    }
                }
                await Task.WhenAny(tasks.ToArray());
            } 
        }

        private async Task CheckPortAsync(ushort port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(_host, port);

                //Connect succeeded
                _openPorts.Add(port);
                OpenPortScanned?.Invoke(port);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionRefused ||
                    e.SocketErrorCode == SocketError.TimedOut)
                {
                    _closedPorts.Add(port);
                    ClosedPortScanned?.Invoke(port);
                }
                else if (e.SocketErrorCode == SocketError.AccessDenied)
                {
                    _openPorts.Add(port);
                    OpenPortScanned?.Invoke(port);
                }
                else
                {
                    Console.Write("Unhandled SocketError: ");
                    Console.Write(Enum.GetName(typeof(SocketError),e.SocketErrorCode));
                    Console.Write("\n");
                }
            }
            finally
            {
                if(socket.Connected)
                    socket.Disconnect(false);

                socket.Close();
            }
        }
    }
}
