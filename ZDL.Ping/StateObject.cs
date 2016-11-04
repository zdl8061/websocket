using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace PingServer
{
    /// <summary>Holds socket current state.</summary>
    public class StateObject
    {
        #region Variables

        private Guid vUniqueID;
        private DateTime vConnectionDate;
        private DateTime vLastUsed;
        private Socket vWorkSocket;
        private int vBufferSize;
        private Byte[] vBuffer;
        private bool vTerminate;

        #endregion Variables

        #region Properties

        /// <summary>The last time the connection was used.</summary>
        public DateTime LastUsed
        {
            get { return vLastUsed; }
            set { vLastUsed = value; }
        }

        /// <summary>The unique id for the state object.</summary>
        public Guid UniqueID
        {
            get { return vUniqueID; }
            set { vUniqueID = value; }
        }

        /// <summary>The connection date of the socket.</summary>
        public DateTime ConnectionDate
        {
            get { return vConnectionDate; }
            set { vConnectionDate = value; }
        }

        /// <summary>Currently working socket.</summary>
        public Socket WorkSocket
        {
            get { return vWorkSocket; }
            set { vWorkSocket = value; }
        }

        /// <summary>Allowed buffersize.</summary>
        public int BufferSize
        {
            get { return vBufferSize; }
        }

        /// <summary>Current socket buffer.</summary>
        public Byte[] Buffer
        {
            get { return vBuffer; }
            set { vBuffer = value; }
        }

        /// <summary>Terminates the connection after send.</summary>
        public bool Terminate
        {
            get { return vTerminate; }
            set { vTerminate = value; }
        }

        #endregion Properties

        #region Constructor

        /// <summary>Contructor</summary>
        public StateObject(Socket _WorkSocket, int _BufferSize)
        {
            vUniqueID = Guid.NewGuid();
            vConnectionDate = DateTime.Now;
            vLastUsed = DateTime.Now;
            vWorkSocket = _WorkSocket;
            vBufferSize = _BufferSize;
            vBuffer = new Byte[_BufferSize];
        }

        #endregion Constructor
    }
}