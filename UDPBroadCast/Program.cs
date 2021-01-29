using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UDPBroadCast
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            LabelReceiveService.StartReceive("", 5000);

            Console.Read();
        }
    }

    public class LabelReceiveService
    {
        public static string IP { get; private set; }
        public static int Port { get; private set; }

        /// <summary>
        /// 用于UDP发送的网络服务类
        /// </summary>
        private static UdpClient udpcRecv = null;

        private static IPEndPoint localIpep = null;

        /// <summary>
        /// 开关：在监听UDP报文阶段为true，否则为false
        /// </summary>
        private static bool IsUdpcRecvStart = false;

        /// <summary>
        /// 线程：不断监听UDP报文
        /// </summary>
        private static Thread thrRecv;

        private static Thread thrBroadcast;

        public static void StartReceive(string ip, int port)
        {
            IP = ip;
            Port = port;
            if (!IsUdpcRecvStart) // 未监听的情况，开始监听
            {
                localIpep = new IPEndPoint(IPAddress.Parse("192.168.0.135"), Port); // 本机IP和监听端口号
                udpcRecv = new UdpClient(localIpep);
                thrRecv = new Thread(ReceiveMessage);
                thrRecv.Start();
                thrBroadcast = new Thread(SendBroadcast);
                thrBroadcast.Start();
                IsUdpcRecvStart = true;
                Console.WriteLine("UDP监听器已成功启动");
            }
        }

        public static void StopReceive()
        {
            if (IsUdpcRecvStart)
            {
                thrRecv.Abort(); // 必须先关闭这个线程，否则会异常
                udpcRecv.Close();
                IsUdpcRecvStart = false;
                Console.WriteLine("UDP监听器已成功关闭");
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="obj"></param>
        private static void ReceiveMessage(object obj)
        {
            while (IsUdpcRecvStart)
            {
                try
                {
                    var recvEP = new IPEndPoint(IPAddress.Any, Port);
                    byte[] bytRecv = udpcRecv.Receive(ref localIpep);
                    string message = Encoding.ASCII.GetString(bytRecv, 0, bytRecv.Length);
                    Console.WriteLine(string.Format("Receive port:{0} message:[{1}]", Port, message));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        /// <summary>
        /// 定时广播
        /// </summary>
        public static void SendBroadcast()
        {
            while (IsUdpcRecvStart)
            {
                try
                {
                    string message = DateTime.Now.ToString();
                    byte[] sendbytes = Encoding.ASCII.GetBytes(message);
                    var broadcastEP = new IPEndPoint(IPAddress.Broadcast, 5001);
                    udpcRecv.Send(sendbytes, sendbytes.Length, broadcastEP);
                    Console.WriteLine(string.Format("Timed broadcast:{0} message:[{1}]", 5001, message));
                    Thread.Sleep(2000);
                }
                catch { }
            }
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="obj"></param>
        public static void SendMessage(string obj)
        {
            try
            {
                string message = (string)obj;
                //byte[] sendbytes = Encoding.Unicode.GetBytes(message);
                byte[] sendbytes = Encoding.ASCII.GetBytes(message);
                //IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8899); // 发送到的IP地址和端口号
                udpcRecv.Send(sendbytes, sendbytes.Length, localIpep);
                //udpcSend.Close();
            }
            catch { }
        }
    }
}