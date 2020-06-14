using SpiderUtil.TCP_UDPHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiderCore
{
    public class TCPManager
    {
        public static TCPManager manager = new TCPManager();

        public TcpHelper tcpHelper = new TcpHelper();

        public  MySession GetMySession(string IP)
        {
            return new MySession();
        }
    }
}
