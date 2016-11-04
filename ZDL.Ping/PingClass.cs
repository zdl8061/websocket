using System;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace PingServer
{
    public class PingClass
    {
        #region Variables

        private int vReceivedTotal;
        private int vLostTotal;
        private List<long> vPacketTimes;

        private StateObject vCurrentState;
        private bool vCancelPing;
        private string vPingAddress;
        private int vTotalPings = 0;
        private System.Timers.Timer vPingTimer;
        private PingCompletedEventHandler vPingCompleteHandler;

        #endregion Variables

        #region Properties

        /// <summary>The total received.</summary>
        public int ReceivedTotal
        {
            get { return vReceivedTotal; }
            set { vReceivedTotal = value; }
        }

        /// <summary>The total lost.</summary>
        public int LostTotal
        {
            get { return vLostTotal; }
            set { vLostTotal = value; }
        }

        /// <summary>The packet times.</summary>
        public List<long> PacketTimes
        {
            get { return vPacketTimes; }
            set { vPacketTimes = value; }
        }

        /// <summary>The current state object.</summary>
        public StateObject CurrentState
        {
            get { return vCurrentState; }
            set { vCurrentState = value; }
        }

        /// <summary>When set cancels the ping.</summary>
        public bool CancelPing
        {
            get { return vCancelPing; }
            set { vCancelPing = value; }
        }

        /// <summary>The address to ping.</summary>
        public string PingAddress
        {
            get { return vPingAddress; }
            set { vPingAddress = value; }
        }

        /// <summary>The total pings currently done.</summary>
        public int TotalPings
        {
            get { return vTotalPings; }
            set { vTotalPings = value; }
        }

        /// <summary>The ping timer.</summary>
        public System.Timers.Timer PingTimer
        {
            get { return vPingTimer; }
            set { vPingTimer = value; }
        }

        /// <summary>Handler for attaching the ping handler.</summary>
        public PingCompletedEventHandler PingCompleteHandler
        {
            get { return vPingCompleteHandler; }
            set { vPingCompleteHandler = value; }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>Send a ping for times to an address.</summary>
        /// <param name="_NameOrIP">The name or ip to ping.</param>
        public void Connect(string _NameOrIP, StateObject _CurrentState)
        {
            vPacketTimes = new List<long>();
            vReceivedTotal = 0;
            vLostTotal = 0;
            vCurrentState = _CurrentState;
            vPingAddress = _NameOrIP;
            vTotalPings = 0;
            vPingTimer = new System.Timers.Timer();
            vPingTimer.Interval = 1000;
            vPingTimer.Elapsed += new System.Timers.ElapsedEventHandler(vPingTimer_Elapsed);
            vPingTimer.Start();
        }

        #endregion Public Methods

        #region Private Methods

        private void PingThis()
        {
            //Declare Variables---------------
            Ping Pinger = null;
            PingOptions Options = null;
            byte[] Buffer = null;
            int Timeout = 4000;
            //--------------------------------

            try
            {
                //Create new ping-------
                Pinger = new Ping();
                //----------------------

                //Attach Callback-------
                Pinger.PingCompleted += new PingCompletedEventHandler(Pinger_PingCompleted);
                //----------------------

                //The Ping Options------
                Options = new PingOptions(64, true);
                //----------------------

                //Create Buffer---------
                Buffer = Encoding.Default.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                //----------------------

                //Send Async Ping-------
                Pinger.SendAsync(this.PingAddress, Timeout, Buffer, Options, this);
                //----------------------
            }
            catch { }
            finally
            {
                if (Buffer != null) { Buffer = null; }
                if (Options != null) { Options = null; }
                if (Pinger != null) { Pinger.Dispose(); Pinger = null; }
            }
        }

        /// <summary>The ping timer.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vPingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            vPingTimer.Stop();

            //Check for cancelled ping--
            if (CancelPing) { return; }
            //--------------------------

            PingThis();
        }

        /// <summary>Handles when the ping completes.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pinger_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            //Check for cancelled ping--
            if (CancelPing) { return; }
            //--------------------------

            //Increase total pings------
            vTotalPings += 1;
            //--------------------------

            if (e.Reply != null)
            {
                switch (e.Reply.Status)
                {
                    case System.Net.NetworkInformation.IPStatus.TimedOut:

                        LostTotal++;

                        break;

                    case System.Net.NetworkInformation.IPStatus.Success:

                        ReceivedTotal++;

                        //Add Time to list----------
                        vPacketTimes.Add(e.Reply.RoundtripTime);
                        //--------------------------

                        break;
                }
            }

            //Invoke other event------------------------------
            this.PingCompleteHandler.Invoke(sender, e);
            //------------------------------------------------

            //Check to see if needs more ping-----------------
            if (vTotalPings < 4) { this.PingTimer.Start(); }
            //------------------------------------------------
        }

        #endregion Private Methods
    }
}