using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ZDL.Net
{
    public class Server
    {
        private static Socket listener;
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public const int _bufferSize = 1024;
        public const int _port = 50000;
        public static bool _isRunning = true;

        class StateObject
        {
            public Socket workSocket = null;
            public byte[] buffer = new byte[_bufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        static string Between(string str, string str1, string str2)
        {
            int i1 = 0, i2 = 0;
            string rtn = "";

            i1 = str.IndexOf(str1, StringComparison.InvariantCultureIgnoreCase);
            if (i1 > -1)
            {
                i2 = str.IndexOf(str2, i1 + 1, StringComparison.InvariantCultureIgnoreCase);
                if (i2 > -1)
                {
                    rtn = str.Substring(i1 + str1.Length, i2 - i1 - str1.Length);
                }
            }
            return rtn;
        }


        static bool IsSocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }

        public void Start()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, _port);
            listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEP);

            while (_isRunning)
            {
                allDone.Reset();
                listener.Listen(10);
                listener.BeginAccept(new AsyncCallback(acceptCallback), listener);
                bool isRequest = allDone.WaitOne(new TimeSpan(12, 0, 0));

                if (!isRequest)
                {
                    allDone.Set();

                }
            }
            listener.Close();
        }

        static void acceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;

            if (listener != null)
            {
                Socket handler = listener.EndAccept(ar);

                allDone.Set();

                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, _bufferSize, 0, new AsyncCallback(readCallback), state);
            }
        }

        static void readCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            if (!IsSocketConnected(handler))
            {
                handler.Close();
                return;
            }

            int read = handler.EndReceive(ar);

            if (read > 0)
            {
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, read));

                if (state.sb.ToString().Contains("Sec-WebSocket-Key"))
                {
                    string key = string.Empty;
                    Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
                    Match m = reg.Match(state.sb.ToString());
                    if (m.Value != "")
                    {
                        key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
                    }

                    byte[] secKeyBytes = SHA1.Create().ComputeHash(
                                             Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
                    string secKey = Convert.ToBase64String(secKeyBytes);

                    var responseBuilder = new StringBuilder();
                    responseBuilder.AppendLine("HTTP/1.1 101 Switching Protocols");
                    responseBuilder.AppendLine("Upgrade: websocket");
                    responseBuilder.AppendLine("Connection: Upgrade");
                    responseBuilder.AppendLine("Sec-WebSocket-Accept: " + secKey);
                    responseBuilder.AppendLine();

                    byte[] bytesToSend = Encoding.UTF8.GetBytes(responseBuilder.ToString());
                    handler.BeginSend(bytesToSend, 0, bytesToSend.Length, SocketFlags.None
                        , new AsyncCallback(sendCallback), state);
                }
                else
                {
                    //handler.BeginReceive(state.buffer, 0, _bufferSize, 0
                    //        , new AsyncCallback(readCallback), state);

                    string recString = AnalyzeClientData(state.buffer, state.buffer.Length);


                    byte[] bytesToSend = PackageServerData(new SocketMessage());

                    handler.BeginSend(bytesToSend, 0, bytesToSend.Length, SocketFlags.None
                        , new AsyncCallback(sendCallback), state);
                }
            }
            else
            {
                handler.Close();
            }
        }

        static void sendCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            handler.EndSend(ar);

            StateObject newstate = new StateObject();
            newstate.workSocket = handler;
            handler.BeginReceive(newstate.buffer, 0, newstate.buffer.Length, 0, new AsyncCallback(readCallback), newstate);
        }


        private static string AnalyzeClientData(byte[] recBytes, int recByteLength)
        {
            if (recByteLength < 2) { return string.Empty; }

            bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
            if (!fin)
            {
                return string.Empty;// 超过一帧暂不处理 
            }

            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
            if (!mask_flag)
            {
                return string.Empty;// 不包含掩码的暂不处理
            }

            int payload_len = recBytes[1] & 0x7F; // 数据长度  

            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126)
            {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);

            }
            else if (payload_len == 127)
            {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                for (UInt64 i = 0; i < len; i++)
                {
                    payload_data[i] = recBytes[i + 14];
                }
            }
            else
            {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);

            }

            for (var i = 0; i < payload_len; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }

            return Encoding.UTF8.GetString(payload_data);
        }

        private static byte[] PackageServerData(SocketMessage sm)
        {
            //StringBuilder msg = new StringBuilder();
            //if (!sm.isLoginMessage)
            //{ //消息是login信息
            //    msg.AppendFormat("{0} @ {1}:\r\n    ", sm.Client.Name, sm.Time.ToShortTimeString());
            //    msg.Append(sm.Message);
            //}
            //else
            //{ //处理普通消息
            //    msg.AppendFormat("{0} login @ {1}", sm.Client.Name, sm.Time.ToShortTimeString());
            //}


            byte[] content = null;
            //byte[] temp = Encoding.UTF8.GetBytes(msg.ToString());

            byte[] temp = Encoding.UTF8.GetBytes("别乱发消息");

            if (temp.Length < 126)
            {
                content = new byte[temp.Length + 2];
                content[0] = 0x81;
                content[1] = (byte)temp.Length;
                Array.Copy(temp, 0, content, 2, temp.Length);
            }
            else if (temp.Length < 0xFFFF)
            {
                content = new byte[temp.Length + 4];
                content[0] = 0x81;
                content[1] = 126;
                content[2] = (byte)(temp.Length & 0xFF);
                content[3] = (byte)(temp.Length >> 8 & 0xFF);
                Array.Copy(temp, 0, content, 4, temp.Length);
            }
            else
            {
                // 暂不处理超长内容  
            }

            return content;
        }
    }

    public class SocketMessage
    {

        public bool isLoginMessage { get; set; }

        public char[] Message { get; set; }


        
    }
}
