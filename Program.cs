using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SQLite;


namespace Attempt2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a Http server and start listening for incoming connections
            HttpServerInSQL.listener = new HttpListener();
            HttpServerInSQL.listener.Prefixes.Add(HttpServerInSQL.url);
            HttpServerInSQL.listener.Start();
            Console.WriteLine("Listening for connections on {0}", HttpServerInSQL.url);

            // Handle requests
            Task listenTask = HttpServerInSQL.HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            HttpServerInSQL.listener.Close();
        }
    }
}
