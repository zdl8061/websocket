using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

namespace PingServer
{
    /// <summary>Class for creating a tcp socket listener.</summary>
    public class Listener
    {
        #region Variables

        private ManualResetEvent vManualReset = null;
        private Dictionary<Guid, StateObject> vStateObjects = null;
        private List<Guid> vStateIDS = null;
        private Socket vListenerSocket = null;

        #endregion Variables

        #region Properties

        /// <summary>Holds the current state ids.</summary>
        public List<Guid> StateIDS
        {
            get { return vStateIDS; }
            set { vStateIDS = value; }
        }

        /// <summary>Holds all currently working socket stateobject information.</summary>
        internal Dictionary<Guid, StateObject> StateObjects
        {
            get { return vStateObjects; }
        }

        /// <summary>The main listener socket.</summary>
        public Socket ListenerSocket
        {
            get { return vListenerSocket; }
        }

        #endregion Properties

        #region Constructor

        /// <summary>Constructor</summary>
        public Listener()
        {

        }

        #endregion Constructor

        #region Server Methods

        /// <summary>Listen for connections.</summary>
        public void Listen()
        {
            //Declare variables--------------
            IPHostEntry Host = null;
            IPEndPoint EP = null;
            IPAddress IP = null;
            //-------------------------------

            //Set Host and Endpoint----------
            IP = IPAddress.Parse("127.0.0.1");
            if (IPAddress.TryParse(ConfigurationManager.AppSettings["ListeningAddress"], out IP))
            {
                EP = new IPEndPoint(IP, int.Parse(ConfigurationManager.AppSettings["ListeningPort"]));
            }
            else
            {
                Host = (ConfigurationManager.AppSettings["ListeningAddress"] == string.Empty) ? Dns.GetHostEntry(ConfigurationManager.AppSettings["ListeningAddress"]) : Dns.GetHostEntry(Dns.GetHostName());
                EP = new IPEndPoint(Host.AddressList[0], int.Parse(ConfigurationManager.AppSettings["ListeningPort"]));
            }
            //-------------------------------

            //Create listener and stateobject holder----------------
            vManualReset = new ManualResetEvent(false);
            vListenerSocket = new Socket(EP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //------------------------------------------------------

            try
            {
                //Bind Socket and start listening----
                vListenerSocket.Bind(EP);
                vListenerSocket.Listen(int.Parse(ConfigurationManager.AppSettings["TotalAllowedConnections"]));
                //-----------------------------------

                Console.WriteLine("Ping Server Listening to " + EP.ToString());

                while (true)
                {
                    vManualReset.Reset();


                    vListenerSocket.BeginAccept(new AsyncCallback(ConnectionAccept), vListenerSocket);
                    vManualReset.WaitOne();
                }
            }
            catch { }
            finally
            {

            }
        }

        /// <summary>Accepts the connection and transfers it and calls listening thread to repeat</summary>
        /// <param name="_AR">The async result.</param>
        private void ConnectionAccept(IAsyncResult _AR)
        {
            //Declare Variables-----------
            Socket ListSocket = null;
            Socket CurrentSocket = null;
            StateObject CurrentStateObject = null;
            //----------------------------

            //Set Sockets-----------------
            ListSocket = (Socket)_AR.AsyncState;
            CurrentSocket = ListSocket.EndAccept(_AR);
            //----------------------------

            //Set ManualEvent-------------
            vManualReset.Set();
            //----------------------------

            //Check if there is stateobjects and add to it----------
            if (vStateObjects == null) { vStateObjects = new Dictionary<Guid, StateObject>(); }
            //------------------------------------------------------

            //Check if stateids is null and add to it---------------
            if (vStateIDS == null) { vStateIDS = new List<Guid>(); }
            //------------------------------------------------------

            //Set Current State Object------------
            CurrentStateObject = new StateObject(CurrentSocket, int.Parse(ConfigurationManager.AppSettings["BufferSize"]));
            //------------------------------------

            //Add StateObject to generic list-----
            vStateObjects.Add(CurrentStateObject.UniqueID, CurrentStateObject);
            //------------------------------------

            //Add ID------------------------------
            vStateIDS.Add(CurrentStateObject.UniqueID);
            //------------------------------------

            Console.WriteLine("Incoming Connection from " + CurrentStateObject.WorkSocket.RemoteEndPoint.ToString().Split(':')[0]);

            try
            {
                //Start Receiving Data----------------
                CurrentSocket.BeginReceive(CurrentStateObject.Buffer, 0, CurrentStateObject.BufferSize, 0, new AsyncCallback(ReadCallback), CurrentStateObject);
                //------------------------------------
            }
            catch { CloseSocket(CurrentStateObject); return; }
        }

        /// <summary>Handles the receiving of data from a current socket.</summary>
        /// <param name="_AR">The async result.</param>
        private void ReadCallback(IAsyncResult _AR)
        {
            //Declare Variables-----------
            StateObject CurrentState = null;
            int BytesRead = 0;
            string Command = string.Empty;
            string r = string.Empty;
            Guid[] CurrentIDS = new Guid[vStateIDS.Count];
            //----------------------------

            //Copy IDS--------------------
            vStateIDS.CopyTo(CurrentIDS);
            //----------------------------

            //Set StateObject-------------
            CurrentState = (StateObject)_AR.AsyncState;
            //----------------------------

            //Get the total bytes read----
            try
            {
                BytesRead = CurrentState.WorkSocket.EndReceive(_AR);
            }
            catch { CloseSocket(CurrentState); return; }
            //----------------------------

            if (BytesRead > 0)
            {
                //Check for XML Domain Policy Request-------
                Command = Encoding.Default.GetString(CurrentState.Buffer, 0, BytesRead);
                //------------------------------------------

                switch (Command.ToUpper())
                {
                    case "<POLICY-FILE-REQUEST/>\0":

                        Console.WriteLine("Sending cross domain policy to " + CurrentState.WorkSocket.RemoteEndPoint.ToString().Split(':')[0]);

                        //Send Cross Domain Policy--------------------------------------------
                        XmlDocument XD = new XmlDocument();
                        Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                        ConfigurationSection CS = (ConfigurationSection)Config.GetSection("CrossDomainPolicy/Policy");

                        XD.LoadXml(CS.SectionInformation.GetRawXml());
                        XmlNode XN = XD.SelectSingleNode("Policy");
                        Send(XN.InnerText.Replace("\r\n", "") + "\0", CurrentState, true);
                        //--------------------------------------------------------------------

                        break;

                    default:

                        string[] cmd = Command.Split(':');
                        if (cmd.Length == 2) { cmd[1] = cmd[1].Trim(); }

                        if (cmd[0].ToUpper() == "PING")
                        {
                            Console.WriteLine("Ping request by " + CurrentState.WorkSocket.RemoteEndPoint.ToString().Split(':')[0] + " for " + cmd[1]);

                            //Declare variables--------------
                            IPAddress IP = null;
                            //-------------------------------

                            //Set Host and Endpoint----------
                            IP = IPAddress.Parse("127.0.0.1");
                            if (IPAddress.TryParse(cmd[1], out IP))
                            {
                                Send("Pinging " + cmd[1] + " with 32 bytes of data:\n\n", CurrentState, false);
                            }
                            else
                            {
                                IP = Dns.GetHostEntry(cmd[1]).AddressList[0];
                                Send("Pinging " + cmd[1] + " [" + IP.ToString() + "] with 32 bytes of data:\n\n", CurrentState, false);
                            }
                            //-------------------------------                           

                            //Create ping class
                            PingClass Pinger = new PingClass();
                            Pinger.PingCompleteHandler += new System.Net.NetworkInformation.PingCompletedEventHandler(Pinger_PingCompleted);
                            Pinger.Connect(IP.ToString(), CurrentState);
                        }

                        break;
                }

                try
                {
                    //Get more data from socket-----------------
                    CurrentState.WorkSocket.BeginReceive(CurrentState.Buffer, 0, CurrentState.BufferSize, 0, new AsyncCallback(ReadCallback), CurrentState);
                    //------------------------------------------
                }
                catch { CloseSocket(CurrentState); return; }
            }
            else
            {
                //Close the connection and remove the object-----
                CloseSocket(CurrentState);
                //-----------------------------------------------
            }
        }

        /// <summary>Handles when the ping completes.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pinger_PingCompleted(object sender, System.Net.NetworkInformation.PingCompletedEventArgs e)
        {
            PingClass Pinger = (PingClass)e.UserState;

            // If the operation was canceled, display a message to the user.
            if (e.Cancelled)
            {
                Send("Request was cancelled by user.\n", Pinger.CurrentState, true);
                
                return;
            }

            // If an error occurred, display the exception to the user.
            if (e.Error != null)
            {
                Send("Ping error occured.\n", Pinger.CurrentState, true);

                return;
            }

            switch (e.Reply.Status)
            {
                case System.Net.NetworkInformation.IPStatus.TimedOut:

                    Send("Request timed out.\n", Pinger.CurrentState, false);

                    break;

                case System.Net.NetworkInformation.IPStatus.Success:

                    Send(string.Format("Reply from {0}: bytes={1} time={2}ms TTL={3}\n", 
                        e.Reply.Address.ToString(), 
                        e.Reply.Buffer.Length, 
                        e.Reply.RoundtripTime, 
                        e.Reply.Options.Ttl),Pinger.CurrentState, false);
                   
                    break;
            }

            if (Pinger.TotalPings == 4)
            {
                long Min = 0;
                long Max = 0;
                long Avg = 0;

                if (Pinger.PacketTimes.Count > 0)
                {
                    Pinger.PacketTimes.Sort();
                    Min = Pinger.PacketTimes[0];
                    Max = Pinger.PacketTimes[Pinger.PacketTimes.Count - 1];

                    foreach (long t in Pinger.PacketTimes)
                    {
                        Avg += t;
                    }

                    Avg = Avg / Pinger.PacketTimes.Count ;
                }

                Send(string.Format("\nPing statistics for {0}:\n\tPackets Sent = 4, Received = {1}, Lost = {2} ({3}% loss),\nApproximate round trip times in milli-seconds:\n\tMinimum = {4}ms, Maximum = {5}ms, Average = {6}ms\n\n", 
                    e.Reply.Address.ToString(), 
                    Pinger.ReceivedTotal, 
                    Pinger.LostTotal, 
                    (Pinger.LostTotal==0)? 0 : (Pinger.LostTotal*25),
                    Min,
                    Max,
                    Avg), Pinger.CurrentState, true);
            }
        }

        /// <summary>Closes the socket and disposes of the state object and removes it from the generic dictonary.</summary>
        /// <param name="_StateObject">The stateobject to remove.</param>
        private void CloseSocket(StateObject _StateObject)
        {
            //Declare Variables---------------
            Guid UniqueID = _StateObject.UniqueID;
            //--------------------------------

            //Close Connection----------------
            if (_StateObject.WorkSocket != null)
            {
                try
                {
                    _StateObject.WorkSocket.Shutdown(SocketShutdown.Both);
                    _StateObject.WorkSocket.Close();
                }
                catch { }
                finally
                {
                    _StateObject.WorkSocket = null;
                    _StateObject = null;
                }

            }
            //--------------------------------

            //Remove object from dictonary----
            vStateIDS.Remove(UniqueID);
            vStateObjects.Remove(UniqueID);
            //--------------------------------
        }

        /// <summary>Send string data to the current socket.</summary>
        /// <param name="_StringData">The string to send.</param>
        /// <param name="_StateObject">The stateobject to send the string to.</param>
        /// <param name="_CloseConnection">Closes the connection.</param>
        public void Send(string _StringData, StateObject _StateObject, bool _CloseConnection)
        {
            Send(Encoding.Default.GetBytes(_StringData), _StateObject, _CloseConnection);
        }

        /// <summary>Send string data to the current socket.</summary>
        /// <param name="_ByteData">The byte data to send.</param>
        /// <param name="_StateObject">The stateobject to send the data to.</param>
        /// <param name="_CloseConnection">Closes the connection.</param>
        public void Send(byte[] _ByteData, StateObject _StateObject, bool _CloseConnection)
        {
            _StateObject.Terminate = _CloseConnection;
            try
            {
                _StateObject.WorkSocket.BeginSend(_ByteData, 0, _ByteData.Length, 0, new AsyncCallback(SendCallback), _StateObject);
            }
            catch { CloseSocket(_StateObject); return; }
        }

        /// <summary>Handles the send callback.</summary>
        /// <param name="ar">The async result.</param>
        private void SendCallback(IAsyncResult _AR)
        {
            //Declare Variables-----------
            StateObject CurrentState = null;
            int BytesSent = 0;
            //----------------------------

            //Set StateObject-------------
            CurrentState = (StateObject)_AR.AsyncState;
            //----------------------------

            try
            {
                BytesSent = CurrentState.WorkSocket.EndSend(_AR);
            }
            catch { CloseSocket(CurrentState); return; }

            //Check to see if terminated---
            if (CurrentState.Terminate) { CloseSocket(CurrentState); }
            //-----------------------------
        }

        #endregion Server Methods
    }
}