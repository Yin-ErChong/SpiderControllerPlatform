using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static SpiderUtil.TCP_UDPHelper.SocketDelegate;

namespace SpiderUtil.TCP_UDPHelper
{
    /// <summary>
    /// 会话端(暂不支持处理粘包问题，正常速度发送往来信息不会发生粘包)
    /// </summary>
    public class MySession
    {
        public Socket TcpSocket;//socket对象

        public event ReceivedMessage ReceivedAfter;
        public event SenddMessage SendBefore;
        public event SenddMessage SendAfter;
        public string clientName { get; set; }
        public List<byte> m_Buffer = new List<byte>();//数据缓存区

        public MySession()
        {

        }
        public virtual void  GetBuffer()
        {

        }
        public void ClearBuffer()
        {

        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sendMessage"></param>
        public void Send(object sendMessage)
        {
            SendBefore?.Invoke(sendMessage, this);
            byte[] bytes;
            if (sendMessage is string)
            {
                bytes = Encoding.UTF8.GetBytes((string)sendMessage);
            }
            else
            {
                bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendMessage));
            }
            Send(bytes);
            SendAfter?.Invoke(sendMessage, this);
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buf"></param>
        public void Send(byte[] buf)
        {
            if (buf != null)
            {
                TcpSocket.Send(buf);
            }
        }
        /// <summary>
        /// 获取连接的ip
        /// </summary>
        /// <returns></returns>
        public string GetIp()
        {
            IPEndPoint clientipe = (IPEndPoint)TcpSocket.RemoteEndPoint;
            string _ip = clientipe.Address.ToString();
            return _ip;
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            TcpSocket.Shutdown(SocketShutdown.Both);
        }
        /// <summary>
        /// 提取正确数据包
        /// </summary>
        public byte[] GetBuffer(int startIndex, int size)
        {           
            byte[] buf = new byte[size];
            m_Buffer.CopyTo(startIndex, buf, 0, size);
            m_Buffer.RemoveRange(0, startIndex + size);
            return buf;
        }

        /// <summary>
        /// 添加队列数据
        /// </summary>
        /// <param name="buffer"></param>
        public void AddQueue(byte[] buffer)
        {
            m_Buffer.AddRange(buffer);
        }
        public void ReceivedAfterBegin()
        {
            //触发接收数据后的事件
            ReceivedAfter?.Invoke(TcpSocket.RemoteEndPoint.ToString());
        }
        public void AddEvent(ReceivedMessage receivedMessage)
        {
            ReceivedAfter += receivedMessage;
        }
        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearQueue()
        {
            m_Buffer.Clear();
        }
    }
}
