using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using TrackerPackage;

namespace server
{
    class Program
    {

        // Propierties
        static Queue<Byte[]> buffer;            // Queue of "streams"
        static Queue<IPEndPoint> remoteSockets; // Queue of streams's sockets
        //static string IP;
        static int Port;

        static void Main(string[] args)
        {
            // Initialization
            buffer = new Queue<byte[]>();
            remoteSockets = new Queue<IPEndPoint>();
            //IP = "127.0.0.1";
            Port = 7777;

            // Threads
            var sav = new Thread(new ThreadStart(saver));
            var lis = new Thread(new ThreadStart(listener));

            // Start
            Console.WriteLine("Starting Server.");
            lis.Start();
            sav.Start();
            

            // End
            Console.WriteLine("Server up!");
        }

        // Listener
        static void listener()
        {
            while(true) {
                UdpClient client = new UdpClient(Port);
                IPEndPoint remoteIp = new IPEndPoint(IPAddress.Any,0);
                byte[] receivedBytes = client.Receive(ref remoteIp);
                buffer.Enqueue(receivedBytes);
                remoteSockets.Enqueue(remoteIp);
                Console.WriteLine("\t{0} byte(s) received from {1}:{2}", receivedBytes.Length, remoteIp.Address.ToString(), remoteIp.Port.ToString());
                client.Close();
            }
        }

        // Saver
        static void saver()
        {
            UdpClient client = new UdpClient();

            while(true) {
                if(buffer.Count>0) {
                    // Decoding the stream
                    TrackerPkg package = new TrackerPkg(buffer.Dequeue());
                    IPEndPoint remoteIp = remoteSockets.Dequeue();

                    // Printing the location
                    Console.WriteLine("\tLocation received from device {0}: {1}.", package.device, package.message);

                    // Sending confirmation
                    TrackerPkg response = new TrackerPkg(1, 0, package.device.ToString(), "");
                    byte[] resBytes = response.GetBytes();
                    client.Send(resBytes,resBytes.Length, remoteIp);
                }
            }
        }


    }
}
