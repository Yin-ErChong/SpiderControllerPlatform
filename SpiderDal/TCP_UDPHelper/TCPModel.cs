using System;
using System.Collections.Generic;
using System.Text;

namespace SpiderUtil.TCP_UDPHelper
{
    public class TCP_RePonseModel
    {
        public bool IsSuccess { get; set; }
        public string ServerName { get; set; }
        public string ServerIp { get; set; }
        public string ServerPort { get; set; }
        public string ServerTime { get; set; }
        public string ReceivedMessage { get; set; }
        public string ClientName { get; set; }
        public string ClientIp { get; set; }
        public string ClientPort { get; set; }
    }
}
