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
            tcpHelper.OpenServerBefore += test;
        }
        public void test(Socket socket)
        {
            int a = 0;
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
