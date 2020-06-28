using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using static SpiderUtil.TCP_UDPHelper.SocketDelegate;
using Newtonsoft.Json;

namespace SpiderUtil.TCP_UDPHelper
{
    /// <summary>
    /// 服务端
    /// </summary>
    public class TcpHelper
    {
        private Socket ServerSocket = null;//服务端  
        public Dictionary<string, MySession> dic_ClientSocket = new Dictionary<string, MySession>();//tcp客户端字典
        private Dictionary<string, Thread> dic_ClientThread = new Dictionary<string, Thread>();//线程字典,每新增一个连接就添加一条线程
        private bool Flag_Listen = true;//监听客户端连接的标志
        public event ReceivedMessage ReceivedAfter;
        public event OpenServerBefore OpenServerBefore;
        public event OpenServerAfter OpenServerAfter;
        #region 启动/关闭服务
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="port">端口号</param>
        public bool OpenServer(int port)
        {
            try
            {
                Flag_Listen = true;
                // 创建负责监听的套接字，注意其中的参数；
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // 创建包含ip和端口号的网络节点对象；
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                try
                {
                    Logger.Info($"绑定端口到{endPoint.Address}:{endPoint.Port}");
                    // 将负责监听的套接字绑定到唯一的ip和端口上；
                    ServerSocket.Bind(endPoint);
                }
                catch (Exception ee)
                {
                    Logger.Error("绑定端口发生异常", ee);
                    return false;
                }
                OpenServerBefore?.Invoke(ServerSocket);
                // 设置监听队列的长度；
                ServerSocket.Listen(100);
                // 创建负责监听的线程；
                Thread Thread_ServerListen = new Thread(ListenConnecting);
                Thread_ServerListen.IsBackground = true;
                Thread_ServerListen.Start();
                OpenServerAfter?.Invoke(ServerSocket);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 关闭服务
        /// </summary>
        public void CloseServer()
        {
            lock (dic_ClientSocket)
            {
                foreach (var item in dic_ClientSocket)
                {
                    item.Value.Close();//关闭每一个连接
                }
                dic_ClientSocket.Clear();//清除字典
            }
            lock (dic_ClientThread)
            {
                foreach (var item in dic_ClientThread)
                {
                    item.Value.Abort();//停止线程
                }
                dic_ClientThread.Clear();
            }
            Flag_Listen = false;
            //ServerSocket.Shutdown(SocketShutdown.Both);//服务端不能主动关闭连接,需要把监听到的连接逐个关闭
            if (ServerSocket != null)
                ServerSocket.Close();

        }
        #endregion

        #region 侦听接口消息
        /// <summary>
        /// 监听客户端请求的方法；
        /// </summary>
        private void ListenConnecting()
        {
            while (Flag_Listen)  // 持续不断的监听客户端的连接请求；
            {
                try
                {
                    Socket sokConnection = ServerSocket.Accept(); // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字

                    MySession myTcpClient = new MySession() { TcpSocket = sokConnection };
                    myTcpClient.ReceivedAfter += ReceivedAfter;

                    Thread th_ReceiveData = new Thread(ReceiveData);//创建线程接收数据
                    th_ReceiveData.IsBackground = true;
                    th_ReceiveData.Start(myTcpClient);
                    //添加通信对象
                    AddSocketObject(myTcpClient, th_ReceiveData);
                }
                catch (Exception ee)
                {
                    Logger.Error("侦听客户端发生异常", ee);
                }
                Thread.Sleep(200);
            }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sokConnectionparn"></param>
        private void ReceiveData(object sokConnectionparn)
        {
            MySession tcpClient = sokConnectionparn as MySession;
            Socket socketClient = tcpClient.TcpSocket;
            bool Flag_Receive = true;

            while (Flag_Receive)
            {
                try
                {
                    // 定义一个1M的缓存区；
                    byte[] arrMsgRec = new byte[1024 * 1024 * 1];
                    // 将接受到的数据存入到输入  arrMsgRec中；
                    int length = -1;
                    try
                    {
                        length = socketClient.Receive(arrMsgRec); // 接收数据，并返回数据的长度；
                    }
                    catch (Exception ee)
                    {
                        string keystr = socketClient.RemoteEndPoint.ToString();
                        Logger.Error($"{keystr}接收发生异常，线程将结束", ee);
                        Flag_Receive = false;
                        // 从通信线程集合中删除被中断连接的通信线程对象；                       
                        RemoveSocketObject(keystr);
                    }
                    byte[] buf = new byte[length];
                    Array.Copy(arrMsgRec, buf, length);
                    lock (tcpClient.m_Buffer)
                    {
                        tcpClient.AddQueue(buf);
                    }
                    //触发接收后事件
                    tcpClient.ReceivedAfterBegin();
                }
                catch (Exception ex)
                {
                    Logger.Error("ReceiveData发生异常", ex);
                }
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 增加Socket对象
        /// </summary>
        /// <param name="myTcpClient"></param>
        /// <param name="th_ReceiveData"></param>
        private void AddSocketObject(MySession myTcpClient, Thread th_ReceiveData)
        {
            string str_EndPoint = myTcpClient.TcpSocket.RemoteEndPoint.ToString();
            // 把线程及客户连接加入字典
            dic_ClientThread.Add(str_EndPoint, th_ReceiveData);
            dic_ClientSocket.Add(str_EndPoint, myTcpClient);
        }
        /// <summary>
        /// 删除Socket对象
        /// </summary>
        /// <param name="str_EndPoint"></param>
        private void RemoveSocketObject(string str_EndPoint)
        {
            dic_ClientSocket.Remove(str_EndPoint);//删除客户端字典中该socket
            dic_ClientThread[str_EndPoint].Abort();//关闭线程
            dic_ClientThread.Remove(str_EndPoint);//删除字典中该线程
        }


        #endregion

        #region 发送消息
        /// <summary>
        /// 发送数据给指定的客户端
        /// </summary>
        /// <param name="_endPoint">客户端套接字</param>
        /// <param name="_buf">发送的数组</param>
        /// <returns></returns>
        public bool SendData(string _endPoint, byte[] _buf)
        {
            MySession myT = new MySession();
            if (dic_ClientSocket.TryGetValue(_endPoint, out myT))
            {
                myT.Send(_buf);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 发送数据给指定的客户端
        /// </summary>
        /// <param name="_endPoint">客户端套接字</param>
        /// <param name="_buf">发送的数组</param>
        /// <returns></returns>
        public bool SendData(string _endPoint, string sendMessage)
        {
            MySession myT = new MySession();
            if (dic_ClientSocket.TryGetValue(_endPoint, out myT))
            {
                myT.Send(sendMessage);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

    }
    public class SocketDelegate
    {
        public delegate void ReceivedMessage(string ip_Port);
        public delegate void OpenServerBefore(Socket socket);
        public delegate void OpenServerAfter(Socket socket);
    }
    /// <summary>
    /// 会话端
    /// </summary>
    public class MySession
    {
        public Socket TcpSocket;//socket对象
        
        public event ReceivedMessage ReceivedAfter;

        public string clientName { get; set; }
        public List<byte> m_Buffer = new List<byte>();//数据缓存区

        public MySession()
        {

        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sendMessage"></param>
        public void Send(object sendMessage)
        {
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