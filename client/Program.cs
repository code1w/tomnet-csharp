using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using TomNet.Common;
using TomNet.Sockets;
using TomNet;
using TomNet.NetWork;

namespace client
{
    class Program
    {
        
        public static Doraemon dora = new Doraemon();


        static void Main(string[] args)
        {
            Console.WriteLine("[Client]");

            try
            {
                dora.Connect("127.0.0.1", 8888);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);

            }
            Console.ReadLine();
        }
    }
}
