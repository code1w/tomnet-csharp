using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using TomNet.Common;
using System.Threading;
using System.Timers;
using Google.Protobuf;

using Tom;

namespace server
{
    class Server
    {
        static public ByteBuffer buf = new ByteBuffer();
        static public int buflen = 32;
        static public int Packlen = 100;
        static public AutoResetEvent autoEvent = new AutoResetEvent(false);
        static public Socket client = null;
        static public bool stop = false;
        static public bool buildpacket = false;
        static public bool initpacket = false;
        static int[] randomlen = new int[10]{ 4, 5, 20, 4, 6, 53,46, 2, 3,7};
        static int sendcount = 0;

        Google.Protobuf.IMessage msg = null;

        static string GenerateChar()
        {
            Random random = new Random();
            return Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))).ToString();
        }

        static string GenerateChar(int count)
        {
            string randomString = "";

            for (int i = 0; i < count; i++)
            {
                randomString += GenerateChar();
            }
            return randomString;
        }

        static void  GenerateProtobuf()
        {
            if (initpacket)
            {
                buildpacket = true;
                buf.Rollback(0);
                buildpacket = false;
                int len = buf.Readable();
                return ;
            }
            for (int i = 1; i <= Packlen; i++)
            {
                ReqLogin req = new ReqLogin();
                req.Account = "zxb-" + i;
                req.Passward = "123-456-" + i;
                byte[] data = req.ToByteArray();

                int len = sizeof(int) + sizeof(int) + ReqLogin.Descriptor.FullName.Length + data.Length;
                buf.AppendInt32(len);
                buf.AppendInt32(88);
                buf.AppendInt32(ReqLogin.Descriptor.FullName.Length);
                byte[] NameBytes = Encoding.UTF8.GetBytes(ReqLogin.Descriptor.FullName);
                buf.WriteBytes(NameBytes);
                buf.WriteBytes(data);
                initpacket = true;
            }
        }

        

        static int  BuildPacket(int packetnum)
        {
            if(initpacket)
            {
                buildpacket = true;
                buf.Rollback(0);
                buildpacket = false;
                int len = buf.Readable();
                return len;
            }
            else
            {
                buildpacket = true;
                int totallen = 0;
                for(int j = 0; j < packetnum; j++)
                {
                    Random msgrandom = new Random();
                    int msglen = msgrandom.Next(4, 10*1024);
                    if((msglen % sizeof(int)) != 0)
                    {
                        msglen = msglen * sizeof(int);
                    }

                    buf.AppendInt32(msglen); // packetlen 
                    buf.AppendInt32(88);      // packetopt
                    string msg = GenerateChar(msglen);
                    byte[] msgbyte = System.Text.Encoding.Default.GetBytes(msg);
                    buf.Write(msgbyte, 0 , msg.Length);
                    totallen += msglen + sizeof(int);
                }
                buildpacket = false;
                initpacket = true;
                return totallen;
            }
        }

        static void TimerInvokes(Object stateInfo)
        {
            if(stop)
            {
                Console.WriteLine("Stop Timer!!!!!");
                return;
            }

            if(buf.Readable() > 0)
            {
                if(buf.Readable() <= 200)
                {
                    SendData(buf.Readable());
                }
                else
                {
                    //Random random = new Random();
                    //int len = random.Next(1, buf.Readable());
                    SendData(200);
                }
            }
            else
            {
                // 不要停
                //BuildPacket(Packlen);
                GenerateProtobuf();
            }
        }

        /*
         * 手动发包
         */
        static void HandSend()
        {
            Console.WriteLine("输入本次要发送的数据长度====>");
            string input = Console.ReadLine();
            int sendlen = 1;
            if (input.IsNormalized())
            {
                sendlen = int.Parse(input);
            }

            while (!(input.ToLower() == "q"))
            {
                SendData(sendlen);
                Console.WriteLine("输入本次要发送的数据长度====>");
                input = Console.ReadLine();
                if (input.IsNormalized())
                {
                    sendlen = int.Parse(input);
                }
            }

        }

        /*
         * 自动发包模式
         * 一次构造N个数据包,每个包的长度随机
         * N个数据包按顺序存放在一个Buffer中
         * 每次从Buffer中截取一个随机长度的字节流片段发出去
         * 接受方收到的数据或属于一个数据包 或属于M个数据包,
         * 无论怎样拆包逻辑都要能正确的从流中拆解出完整的数据包！！！
         * 数据包格式 len[4字节] + paylod[随机长度] ,len的值只包含payload的长度
         * * */

        static void AutoSend()
        {
            while(true)
            {
                TimerInvokes(null);
                //Thread.Sleep(1);
            }
        }

        static void SendData(int sendlen)
        {
            byte[] data = buf.ReadBytes(sendlen);
            if (data != null)
            {
                int rawsendlen = client.Send(data);
                if(rawsendlen != sendlen)
                {
                    Console.WriteLine("Send Error");
                    stop = true;
                    return;
                }
                Console.WriteLine("Send len : " + sendlen);

            }
        }

        static void TestHashTable()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.AppendInt32(32); // 长度
            Hashtable hashtable = new Hashtable();
            hashtable["data"] = buffer;

            ByteBuffer copy = (ByteBuffer)hashtable["data"];
            var i = copy.ReadInt32();
            copy.AppendInt32(64);
            i = copy.ReadInt32();
            FunctionDealBuf(ref buffer);

        }

        static void FunctionDealBuf(ref ByteBuffer data)
        {
            Console.WriteLine("FunctionDealBuf");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("[Server]");
            try
            {
                TestHashTable();
                Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));
                socketServer.Listen(int.MaxValue);
                Console.WriteLine("服务端已启动， 127.0.0.1:8888...");
                Socket c = null;

                do
                {
                    Console.WriteLine("等待连接...");
                    c = socketServer.Accept();
                    Console.WriteLine("新连接...");
                    //Thread.Sleep(5000);
                    //c.Close();
                    //Console.WriteLine("主动断开客户端连接...");
                } while (c == null);

                client = c;



                //BuildPacket(Packlen);
                GenerateProtobuf();
                AutoSend();
                
                Console.ReadKey();


            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("按任意键退出");
                Console.ReadKey();
            }

        }
    }
}
