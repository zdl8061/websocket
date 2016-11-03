using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZDL.ConsoleCode
{
    class Program
    {
        static void Main(string[] args)
        {


            Socket socket = SocketNet.ConnectServer("192.168.1.45", 50000);

            byte[] body = Encoding.UTF8.GetBytes("<!--SOCKET-->Hi!<!--ENDSOCKET-->");

            SocketNet.SendData(socket, body, 2);

            byte[] bufer = new byte[1024];
            SocketNet.RecvData(socket, bufer, 1);
            string revString = System.Text.Encoding.UTF8.GetString(bufer);

            Console.ReadKey();
        }
    }
}
