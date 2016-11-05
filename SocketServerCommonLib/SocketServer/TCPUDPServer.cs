/********************************************************************
 * * ʹ����ĿԴ��ǰ����ϸ�Ķ�����Э�����ݣ������ͬ������Э�����ʹ�ñ���Ŀ���еĹ���,
 * * ���������Υ��������Э�飬�п������뷨�ɾ��׺��⳥�����߱���׷���������ε�Ȩ����
 * *
 * * Copyright (C) 2014-? cskin Corporation All rights reserved.
 * * ���ߣ� Amos Li    QQ��443061626   .Net��Ŀ������Ⱥ:Amos Li ��Ʒ
 * * ��վ�� CSkin����� http://www.cskin.net ����:�ǿ�˹ ��л���֧��
 * * �뱣�����ϰ�Ȩ��Ϣ���������߽�����׷���������Ρ�
 * * ����ʱ�䣺2014-08-05
********************************************************************/
using CCWin;
using CCWin.SkinControl;
using System;
using System.Windows.Forms;
using SocketServerCommonLib;
using System.Threading;
using System.Drawing;
using System.IO;

namespace SocketServer
{
    public partial class TCPUDPServer : CCSkinMain
    {
        AsyncSocketServer TcpServer;

        AsyncUDPServer UdpServer;

        /// <summary>
        /// �û����Ӷ��ٴ�
        /// </summary>
        int TCPUserCount = 0;
        int TCPDeviceCount = 0;
        public TCPUDPServer()
        {
            InitializeComponent();
            DelegateState.ServerStateInfo = ServerShowStateInfo;
            DelegateState.TeartbeatServerStateInfo = TeartbeatShowStateInfo;
            DelegateState.AddTCPuserStateInfo = AddTCPuser;
            DelegateState.AddTCPdeviceStateInfo = AddTCPdevice;
            DelegateState.ReomveTCPStateInfo = ReomveTCP;
            DelegateState.ServerConnStateInfo = ConnStateInfo;
        }

        #region  AmosLi produce <��������ģ��>

        /// <summary>
        /// ����TCP����
        /// </summary>
        private void btnTCP_Click(object sender, EventArgs e)
        {
            btnTCP.Enabled = false;

            if (TcpServer == null)
                TcpServer = new AsyncSocketServer();
            if (!TcpServer.IsStartListening)
            {
                TcpServer.Start();
                txtMsg.AppendText(DateTime.Now + Environment.NewLine + "TCP����������" + Environment.NewLine);
                lblTCP.Text = "TCP��������ַ:" + TcpServer.serverconfig.ListenIp + ":" + TcpServer.serverconfig.ListenPort;
                PicBoxTCP.BackgroundImage = Properties.Resources._07822_48x48x8BPP_;
                btnTCP.Text = "TCPֹͣ����";
            }
            else
            {
                TcpServer.Stop();
                PicBoxTCP.BackgroundImage = Properties.Resources._07821_48x48x8BPP_;
                txtMsg.AppendText(DateTime.Now + Environment.NewLine + "TCP������ֹͣ" + Environment.NewLine);
            }
            btnTCP.Enabled = true;
        }


        /// <summary>
        /// UDP����
        /// </summary>
        private void btnUDP_Click(object sender, EventArgs e)
        {
            btnUDP.Enabled = false;
            if (UdpServer == null)
                UdpServer = new AsyncUDPServer();

            if (!UdpServer.IsStartListening)
            {
                UdpServer.Start();
                txtMsg.AppendText(DateTime.Now + Environment.NewLine + "UDP����������" + Environment.NewLine);
                lblUDP.Text = "UDP��������ַ:" + HelpCommonLib.NetworkAddress.GetIPAddress() + ":" + UdpServer.ListenProt;
                PicBoxUDP.BackgroundImage = Properties.Resources._07822_48x48x8BPP_;
                btnUDP.Text = "UDPֹͣ����";
                btnUDP.Enabled = true;
            }
            else
            {
                txtMsg.AppendText(DateTime.Now + Environment.NewLine + "UDP������ֹͣ" + Environment.NewLine);
                UdpServer.Close();
                PicBoxUDP.BackgroundImage = Properties.Resources._07821_48x48x8BPP_;
                btnUDP.Text = "UDP�˿ڴ���";
            }
        }
        #region

        #region TCP�ص���������
        void AddTCPuser(SocketUserToken userToken)
        {
            this.Invoke(new ThreadStart(delegate
            {
                TCPUserCount++;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = TCPUserCount.ToString();
                lvi.SubItems.Add(userToken.ConnectSocket.RemoteEndPoint.ToString());
                lvi.SubItems.Add(userToken.UserName);
                lvi.SubItems.Add(userToken.ConnectDateTime.ToString());
                lvi.SubItems.Add("TCP");
                tpe3list1.Items.Add(lvi);
            }));
        }

        /// <summary>
        /// ɾ���û������豸
        /// </summary>
        /// <param name="userToken"></param>
        void ReomveTCP(SocketUserToken userToken)
        {
            this.Invoke(new ThreadStart(delegate
            {
                if (userToken.isDevice)
                {
                    for (int i = 0; i < tpe2list1.Items.Count; i++)
                    {
                        if (userToken.UserName == tpe2list1.Items[i].SubItems[2].Text)
                        {
                            tpe2list1.Items.Remove(tpe2list1.Items[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tpe3list1.Items.Count; i++)
                    {
                        if (userToken.UserName == tpe3list1.Items[i].SubItems[2].Text)
                        {
                            tpe3list1.Items.Remove(tpe3list1.Items[i]);
                        }
                    }
                }
            }));
        }

        void AddTCPdevice(SocketUserToken userToken)
        {
            this.Invoke(new ThreadStart(delegate
            {
                TCPDeviceCount++;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = TCPDeviceCount.ToString();
                lvi.SubItems.Add(userToken.ConnectSocket.RemoteEndPoint.ToString());
                lvi.SubItems.Add(userToken.UserName);
                lvi.SubItems.Add(userToken.ConnectDateTime.ToString());
                lvi.SubItems.Add("TCP");
                tpe2list1.Items.Add(lvi);
            }));
        }
        #endregion

        void ConnStateInfo(string RemoteIp, string TCPUDP)
        {
            this.Invoke(new ThreadStart(delegate
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = listAllView.Items.Count.ToString();
                lvi.SubItems.Add(RemoteIp);
                lvi.SubItems.Add(DateTime.Now.ToString());
                lvi.SubItems.Add(TCPUDP);
                listAllView.Items.Add(lvi);
            }));
        }


        /// <summary>
        /// ��Ϣ���
        /// </summary>
        /// <param name="msg"></param>
        void ServerShowStateInfo(string msg)
        {
            this.Invoke(new ThreadStart(delegate
            {
                tpe2txtMsg.AppendText(DateTime.Now + ":" + msg + Environment.NewLine);
            }));
        }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        void TeartbeatShowStateInfo(int num)
        {
            this.Invoke(new ThreadStart(delegate
            {
                txtMsg.AppendText(DateTime.Now + ":" + num + "���Ӽ��");
                lblNum1.NormlBack = ImageListAllUpdate(num / 10 % 10);
                lblNum2.NormlBack = ImageListAllUpdate(num % 10);
            }));
        }
        #endregion
        /// <summary>
        /// ͼƬ����
        /// </summary>
        Image ImageListAllUpdate(int Num)
        {
            switch (Num)
            {
                case 0:
                    return Properties.Resources._00034_17x25x8BPP_;
                case 1:
                    return Properties.Resources._00035_17x25x8BPP_;
                case 2:
                    return Properties.Resources._00036_17x25x8BPP_;
                case 3:
                    return Properties.Resources._00037_17x25x8BPP_;
                case 4:
                    return Properties.Resources._00038_17x25x8BPP_;
                case 5:
                    return Properties.Resources._00039_17x25x8BPP_;
                case 6:
                    return Properties.Resources._00040_17x25x8BPP_;
                case 7:
                    return Properties.Resources._00041_17x25x8BPP_;
                case 8:
                    return Properties.Resources._00042_17x25x8BPP_;
                case 9:
                    return Properties.Resources._00043_17x25x8BPP_;
                default:
                    return null;
            }
        }
        #endregion
        /// <summary>
        /// ˢ���豸�б�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkdeviceRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (TcpServer == null)
                return;

            linkdeviceRefresh.Enabled = false;
            tpe2list1.Items.Clear();
            SocketUserToken[] userTokenArray = null;
            TcpServer.AsyncSocketDeviceList.CopyList(ref userTokenArray);
            int num = 0;
            for (int i = 0; i < userTokenArray.Length; i++)
            {
                if (userTokenArray[i].LoginFlag)
                {
                    AddTCPdevice(userTokenArray[i]);
                }
                num++;
            }
            lbldevice.Text = "�ϴ�ˢ������ " + DateTime.Now.Hour + " �㣬������ " + userTokenArray.Length + " ���ͻ���";
            linkdeviceRefresh.Enabled = true;
        }

        private void linkuserRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (TcpServer == null)
                return;

            linkuserRefresh.Enabled = false;
            tpe3list1.Items.Clear();
            SocketUserToken[] userTokenArray = null;
            TcpServer.AsyncSocketUserList.CopyList(ref userTokenArray);
            int num = 0;
            for (int i = 0; i < userTokenArray.Length; i++)
            {
                if (userTokenArray[i].LoginFlag)
                {
                    AddTCPuser(userTokenArray[i]);
                    num++;
                }
            }
            lbluser.Text = "�ϴ�ˢ������ " + DateTime.Now.Hour + " �㣬������ " + num + " ���ͻ���";
            linkuserRefresh.Enabled = true;
        }
        /// <summary>
        /// ������Ϣ
        /// </summary>
        private void linkSaveMsg_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter write = new StreamWriter(saveFileDialog1.FileName))
                {
                    write.WriteLine(tpe2txtMsg.Text);
                }
            }
        }
    }
}
