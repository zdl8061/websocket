using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZDL.ConsoleCode
{
    public sealed class SocketNet
    {
        private SocketNet() { }

        /**/
        /// <summary>
        /// 连接使用 tcp 协议的服务端
        /// </summary>
        /// <param name="ip">服务端的ip地址</param>
        /// <param name="port">服务端的端口号</param>
        /// <returns></returns>
        public static Socket ConnectServer(string ip, int port)
        {
            Socket s = null;

            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
                s = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                s.Connect(ipEndPoint);
                if (s.Connected == false)
                {
                    s = null;
                }
            }
            catch (Exception e)
            {
                
            }
            return s;
        }

        /**/
        /// <summary>
        /// 用主机名称连接使用Tcp协议的服务端
        /// </summary>
        /// <param name="hostName">在hosts 文件中存在的主机名称</param>
        /// <param name="port">服务端的端口号</param>
        /// <returns></returns>
        public static Socket ConnectServByHostName(string hostName, int port)
        {
            Socket s = null;
            IPHostEntry iphe = null;

            try
            {
                iphe = Dns.GetHostEntry(hostName);
                foreach (IPAddress ipad in iphe.AddressList)
                {
                    IPEndPoint ipe = new IPEndPoint(ipad, port);
                    Socket tmps = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    tmps.Connect(ipe);

                    if (tmps.Connected)
                    {
                        s = tmps;
                        break;
                    }
                    else
                        continue;
                }
            }
            catch (Exception e)
            {
                
            }
            return s;
        }

        /**/
        /// <summary>
        /// 向远程主机发送数据
        /// </summary>
        /// <param name="socket">要发送数据且已经连接到远程主机的 Socket</param>
        /// <param name="buffer">待发送的数据</param>
        /// <param name="outTime">发送数据的超时时间，以秒为单位，可以精确到微秒</param>
        /// <returns>0:发送数据成功；-1:超时；-2:发送数据出现错误；-3:发送数据时出现异常</returns>
        /// <remarks >
        /// 当 outTime 指定为-1时，将一直等待直到有数据需要发送
        /// </remarks>
        public static int SendData(Socket socket, byte[] buffer, int outTime)
        {
            if (socket == null || socket.Connected == false)
            {
                throw new ArgumentException("参数socket 为null，或者未连接到远程计算机");
            }
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("参数buffer 为null ,或者长度为 0");
            }

            int flag = 0;
            try
            {
                int left = buffer.Length;
                int sndLen = 0;

                while (true)
                {
                    if (socket.Poll(outTime * 1000000, SelectMode.SelectWrite))
                    {        // 收集了足够多的传出数据后开始发送
                        sndLen = socket.Send(buffer, sndLen, left, SocketFlags.None);
                        left -= sndLen;
                        if (left == 0)
                        {                                        // 数据已经全部发送
                            flag = 0;
                            break;
                        }
                        else
                        {
                            if (sndLen > 0)
                            {                                    // 数据部分已经被发送
                                continue;
                            }
                            else
                            {                                                // 发送数据发生错误
                                flag = -2;
                                break;
                            }
                        }
                    }
                    else
                    {                                                        // 超时退出
                        flag = -1;
                        break;
                    }
                }
            }
            catch (SocketException e)
            {
                
                flag = -3;
            }
            return flag;
        }


        /**/
        /// <summary>
        /// 向远程主机发送数据
        /// </summary>
        /// <param name="socket">要发送数据且已经连接到远程主机的 Socket</param>
        /// <param name="buffer">待发送的字符串</param>
        /// <param name="outTime">发送数据的超时时间，以秒为单位，可以精确到微秒</param>
        /// <returns>0:发送数据成功；-1:超时；-2:发送数据出现错误；-3:发送数据时出现异常</returns>
        /// <remarks >
        /// 当 outTime 指定为-1时，将一直等待直到有数据需要发送
        /// </remarks>
        public static int SendData(Socket socket, string buffer, int outTime)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("待发送的字符串长度不能为零.");
            }
            return (SendData(socket, System.Text.Encoding.UTF8.GetBytes(buffer), outTime));
        }

        //public static int SendDataByWeb(string ip, int port, string sendMsg,string recvMsg)
        //{
        //    var net = ConnectServer(ip, port);
        //    int sendStatus = SendData(net, sendMsg, 5);
        //    int recvStatus = RecvData(net, recvMsg, 1024, 5);

        //    net.Close();
        //    return sendStatus;
        //}

        /**/
        /// <summary>
        /// 接收远程主机发送的数据
        /// </summary>
        /// <param name="socket">要接收数据且已经连接到远程主机的 socket</param>
        /// <param name="buffer">接收数据的缓冲区</param>
        /// <param name="outTime">接收数据的超时时间，以秒为单位，可以精确到微秒</param>
        /// <returns>0:接收数据成功；-1:超时；-2:接收数据出现错误；-3:接收数据时出现异常</returns>
        /// <remarks >
        /// 1、当 outTime 指定为-1时，将一直等待直到有数据需要接收；
        /// 2、需要接收的数据的长度，由 buffer 的长度决定。
        /// </remarks>
        public static int RecvData(Socket socket, byte[] buffer, int outTime)
        {
            if (socket == null || socket.Connected == false)
            {
                throw new ArgumentException("参数socket 为null，或者未连接到远程计算机");
            }
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("参数buffer 为null ,或者长度为 0");
            }
            buffer.Initialize();
            int left = buffer.Length;
            int curRcv = 0;
            int flag = 0;

            try
            {
                while (true)
                {
                    if (socket.Poll(outTime * 1000000, SelectMode.SelectRead))
                    {        // 已经有数据等待接收
                        curRcv = socket.Receive(buffer, curRcv, left, SocketFlags.None);
                        left -= curRcv;
                        if (left == 0)
                        {                                    // 数据已经全部接收 
                            flag = 0;
                            break;
                        }
                        else
                        {
                            if (curRcv > 0)
                            {                                // 数据已经部分接收
                                continue;
                            }
                            else
                            {                                            // 出现错误
                                flag = -2;
                                break;
                            }
                        }
                    }
                    else
                    {                                                    // 超时退出
                        flag = -1;
                        break;
                    }
                }
            }
            catch (SocketException e)
            {
               
                flag = -3;
            }
            return flag;
        }

        /**/
        /// <summary>
        /// 接收远程主机发送的数据
        /// </summary>
        /// <param name="socket">要接收数据且已经连接到远程主机的 socket</param>
        /// <param name="buffer">存储接收到的数据的字符串</param>
        /// <param name="bufferLen">待接收的数据的长度</param>
        /// <param name="outTime">接收数据的超时时间，以秒为单位，可以精确到微秒</param>
        /// <returns>0:接收数据成功；-1:超时；-2:接收数据出现错误；-3:接收数据时出现异常</returns>
        /// <remarks >
        /// 当 outTime 指定为-1时，将一直等待直到有数据需要接收；
        /// </remarks>
        public static int RecvData(Socket socket, string buffer, int bufferLen, int outTime)
        {
            if (bufferLen <= 0)
            {
                throw new ArgumentException("存储待接收数据的缓冲区长度必须大于0");
            }
            byte[] tmp = new byte[bufferLen];
            int flag = 0;

            if ((flag = RecvData(socket, tmp, outTime)) == 0)
            {
                buffer = System.Text.Encoding.UTF8.GetString(tmp);
            }
            return flag;
        }


        /**/
        /// <summary>
        /// 向远程主机发送文件
        /// </summary>
        /// <param name="socket" >要发送数据且已经连接到远程主机的 socket</param>
        /// <param name="fileName">待发送的文件名称</param>
        /// <param name="maxBufferLength">文件发送时的缓冲区大小</param>
        /// <param name="outTime">发送缓冲区中的数据的超时时间</param>
        /// <returns>0:发送文件成功；-1:超时；-2:发送文件出现错误；-3:发送文件出现异常；-4:读取待发送文件发生错误</returns>
        /// <remarks >
        /// 当 outTime 指定为-1时，将一直等待直到有数据需要发送
        /// </remarks>
        public static int SendFile(Socket socket, string fileName, int maxBufferLength, int outTime)
        {
            if (fileName == null || maxBufferLength <= 0)
            {
                throw new ArgumentException("待发送的文件名称为空或发送缓冲区的大小设置不正确.");
            }

            int flag = 0;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                long fileLen = fs.Length;                        // 文件长度
                long leftLen = fileLen;                            // 未读取部分
                int readLen = 0;                                // 已读取部分
                byte[] buffer = null;

                if (fileLen <= maxBufferLength)
                {            /**//* 文件可以一次读取*/
                    buffer = new byte[fileLen];
                    readLen = fs.Read(buffer, 0, (int)fileLen);
                    flag = SendData(socket, buffer, outTime);
                }
                else
                {                                    /**//* 循环读取文件,并发送 */
                    buffer = new byte[maxBufferLength];
                    while (leftLen != 0)
                    {
                        readLen = fs.Read(buffer, 0, maxBufferLength);
                        if ((flag = SendData(socket, buffer, outTime)) < 0)
                        {
                            break;
                        }
                        leftLen -= readLen;
                    }
                }
                fs.Close();
            }
            catch (IOException e)
            {
               
                flag = -4;
            }
            return flag;
        }

        /**/
        /// <summary>
        /// 向远程主机发送文件
        /// </summary>
        /// <param name="socket" >要发送数据且已经连接到远程主机的 socket</param>
        /// <param name="fileName">待发送的文件名称</param>
        /// <returns>0:发送文件成功；-1:超时；-2:发送文件出现错误；-3:发送文件出现异常；-4:读取待发送文件发生错误</returns>
        public static int SendFile(Socket socket, string fileName)
        {
            return SendFile(socket, fileName, 2048, 1);
        }


        /**/
        /// <summary>
        /// 接收远程主机发送的文件
        /// </summary>
        /// <param name="socket">待接收数据且已经连接到远程主机的 socket</param>
        /// <param name="fileName">保存接收到的数据的文件名</param>
        /// <param name="fileLength" >待接收的文件的长度</param>
        /// <param name="maxBufferLength">接收文件时最大的缓冲区大小</param>
        /// <param name="outTime">接受缓冲区数据的超时时间</param>
        /// <returns>0:接收文件成功；-1:超时；-2:接收文件出现错误；-3:接收文件出现异常；-4:写入接收文件发生错误</returns>
        /// <remarks >
        /// 当 outTime 指定为-1时，将一直等待直到有数据需要接收
        /// </remarks>
        public static int RecvFile(Socket socket, string fileName, long fileLength, int maxBufferLength, int outTime)
        {
            if (fileName == null || maxBufferLength <= 0)
            {
                throw new ArgumentException("保存接收数据的文件名称为空或发送缓冲区的大小设置不正确.");
            }

            int flag = 0;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Create);
                byte[] buffer = null;

                if (fileLength <= maxBufferLength)
                {                /**//* 一次读取所传送的文件 */
                    buffer = new byte[fileLength];
                    if ((flag = RecvData(socket, buffer, outTime)) == 0)
                    {
                        fs.Write(buffer, 0, (int)fileLength);
                    }
                }
                else
                {                                        /**//* 循环读取网络数据，并写入文件 */
                    int rcvLen = maxBufferLength;
                    long leftLen = fileLength;                        //剩下未写入的数据
                    buffer = new byte[rcvLen];

                    while (leftLen != 0)
                    {
                        if ((flag = RecvData(socket, buffer, outTime)) < 0)
                        {
                            break;
                        }
                        fs.Write(buffer, 0, rcvLen);
                        leftLen -= rcvLen;
                        rcvLen = (maxBufferLength < leftLen) ? maxBufferLength : ((int)leftLen);
                    }
                }
                fs.Close();
            }
            catch (IOException e)
            {
               
                flag = -4;
            }
            return flag;
        }

        /**/
        /// <summary>
        /// 接收远程主机发送的文件
        /// </summary>
        /// <param name="socket">待接收数据且已经连接到远程主机的 socket</param>
        /// <param name="fileName">保存接收到的数据的文件名</param>
        /// <param name="fileLength" >待接收的文件的长度</param>
        /// <returns>0:接收文件成功；-1:超时；-2:接收文件出现错误；-3:接收文件出现异常；-4:写入接收文件发生错误</returns>
        public static int RecvFile(Socket socket, string fileName, long fileLength)
        {
            return RecvFile(socket, fileName, fileLength, 2048, 1);
        }
    }
}