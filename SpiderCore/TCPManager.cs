using SpiderUtil;
using SpiderUtil.TCP_UDPHelper;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SpiderCore
{
    public class TCPManager
    {
        public static TCPManager Instance = new TCPManager();

        public TcpHelper tcpHelper = new TcpHelper();

        public TCPManager(){
            
        }
        public void InitTCPServer()
        {
            tcpHelper.OpenServer(Configer.TCP_Port);
            tcpHelper.ReceivedAfter += Receive;
        }
        public void Receive(string ip_Port)
        {
            MySession mySession;
            tcpHelper.dic_ClientSocket.TryGetValue(ip_Port,out mySession);
            if (mySession == null)
            {
                return;
            }
            TCP_Reponse<string> tCP_Reponse = new TCP_Reponse<string>();
            tCP_Reponse.ClientIp = mySession.TcpSocket.RemoteEndPoint.ToString();
            tCP_Reponse.ServerIp = mySession.TcpSocket.LocalEndPoint.ToString();
            tCP_Reponse.ServerName = "测试服务";
            tCP_Reponse.Data = $"Hello,服务端共有连接{tcpHelper.dic_ClientSocket.Count}个,分别为{string.Join(",", tcpHelper.dic_ClientSocket.Keys)}";
            tCP_Reponse.ReceivedMessage = Encoding.UTF8.GetString(mySession.m_Buffer.ToArray());
            mySession.Send(tCP_Reponse);
            mySession.m_Buffer = new List<byte>();
        }
        public void OpenServer()
        {
            tcpHelper.OpenServer(Configer.TCP_Port);
        }

        public  MySession GetMySession(string ip_Port)
        {
            MySession mySession;
            tcpHelper.dic_ClientSocket.TryGetValue(ip_Port, out mySession);
            return mySession;
        }
    }
}
