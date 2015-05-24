using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace PLAY.API
{
    class Service : IDisposable
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<TcpClient> Clients;
        private bool isListening;

        public Service()
        {
            tcpListener = new TcpListener(IPAddress.Any, 3000);
            Clients = new List<TcpClient>();
        }

        public void Start()
        {
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
            isListening = true;
        }

        private void ListenForClients()
        {
            tcpListener.Start();
            while (isListening)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Clients.Add(client);
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient) client;
            NetworkStream clientStream = tcpClient.GetStream();

            BinaryReader reader = new BinaryReader(clientStream);
            BinaryWriter writer = new BinaryWriter(clientStream);

            int BufferSize;

            while (true)
            {
                try
                {
                    BufferSize = reader.ReadInt32();
                }
                catch
                {
                    break;
                }

                byte[] buffer = reader.ReadBytes(BufferSize);
                string command = System.Text.Encoding.ASCII.GetString(buffer);
                string response = string.Empty;

                switch (command)
                {
                    case "gh":
                        Application.Current.Dispatcher.Invoke(
                            () => response = new WindowInteropHelper(Application.Current.MainWindow).Handle.ToString());
                        break;
                        
                }

                byte[] responsebuffer = System.Text.Encoding.ASCII.GetBytes(response);
                writer.Write(responsebuffer.Length);
                writer.Write(responsebuffer);
            }

            tcpClient.Close();
        }



        public void Dispose()
        {
            foreach (TcpClient c in Clients)
            {
                c.Close();
            }
            tcpListener.Stop();
            isListening = false;
        }
    }
}
