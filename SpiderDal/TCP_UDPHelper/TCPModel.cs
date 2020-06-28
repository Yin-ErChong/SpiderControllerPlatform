using System;
using System.Collections.Generic;
using System.Text;

namespace SpiderUtil.TCP_UDPHelper
{
    public class TCP_Reponse<T>: RePonse
    {
        public TCP_Reponse()
        {
            IsSuccess = true;
            ServerPort = Configer.TCP_Port;
            ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  fff");
        }
        public T Data { get; set; }
    }
    public class RePonse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServerName { get; set; }
        /// <summary>
        /// 服务IP地址
        /// </summary>
        public string ServerIp { get; set; }
        /// <summary>
        /// 服务端口号
        /// </summary>
        public int ServerPort { get; set; }
        /// <summary>
        /// 服务时间
        /// </summary>
        public string ServerTime { get; set; }
        /// <summary>
        /// 收到的信息
        /// </summary>
        public string ReceivedMessage { get; set; }
        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// 客户端IP
        /// </summary>
        public string ClientIp { get; set; }
        /// <summary>
        /// 客户端端口号
        /// </summary>
        public string ClientPort { get; set; }
    }
}
