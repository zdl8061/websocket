using System;
using System.Collections.Generic;
using System.Text;

namespace PingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener l = new Listener();
            l.Listen();
        }

        /// <summary>Handles when the ping completes.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void a_PingCompleted(object sender, System.Net.NetworkInformation.PingCompletedEventArgs e)
        {
            // If the operation was canceled, display a message to the user.
            if (e.Cancelled)
            {
                Console.WriteLine("Request timed out.");
                return;
            }

            // If an error occurred, display the exception to the user.
            if (e.Error != null)
            {
                Console.WriteLine("Ping Error: " + e.Error.ToString());
                return;
            }

            switch (e.Reply.Status)
            {
                case System.Net.NetworkInformation.IPStatus.TimedOut:
                    
                    Console.WriteLine("Request timed out.");
                    
                    break;

                case System.Net.NetworkInformation.IPStatus.Success:

                    Console.WriteLine("Reply from {0}: bytes={1} time={2}ms TTL={3}", e.Reply.Address.ToString(), e.Reply.Buffer.Length, e.Reply.RoundtripTime, e.Reply.Options.Ttl);
                                        
                    break;
            }
        }
    }
}
