using SpiderUtil.TCP_UDPHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiderCore
{
    public  class TCPManager
    {
        public static TCPManager Instance = new TCPManager();

        public TcpHelper tcpHelper = new TcpHelper();

        public  MySession GetMySession(string ip_Port)
        {
            MySession mySession;
            tcpHelper.dic_ClientSocket.TryGetValue(ip_Port, out mySession);
            return mySession;
        }
    }
}
