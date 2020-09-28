
/*
 * This file is part of the GameshNet package.
 *
 * (a) zhang xiao bin <zhangxiaobin@ztgame.com>
 *
 *  2020/09/16
 */


/*
 * 网络使用示例
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomNet.NetWork;
using TomNet.Common;
using TomNet.Core;
using TomNet.Protocol;
using TomNet.Controller;

namespace TomNet
{
    public class Doraemon : IDispatchable
    {
        private EventDispatcher dispatcher;
        private INetWorkClient network;
        private bool inited = false;
        private bool isConnecting = false;
        private string lastHost;
        public EventDispatcher Dispatcher
        {
            get
            {
                return dispatcher;
            }
            set
            {
                dispatcher = value;
            }
        }
        public INetWorkClient NetWork => network;
        public string ConnectionMode => network.ConnectionMode;
        public int CompressionThreshold => network.CompressionThreshold;
        public int MaxMessageSize => network.MaxMessageSize;
        public bool IsConnected
        {
            get
            {
                bool result = false;
                if (network != null)
                {
                    result = network.Connected;
                }
                return result;
            }
        }
        public Doraemon()
        {
            Initialize(false);
        }

        public Doraemon(bool debug)
        {
            Initialize(debug);
        }

        private void Initialize(bool debug)
        {
            if(!inited)
            {
                dispatcher = new EventDispatcher(this);
                network = new TcpNetWorkClinet(new ProtobufProtocol());
                if(network!=null)
                {
                    network.Initialize();
                    network.Controller = new ProtobufController(network);
                    RegisterNetWorkEvent();
                    RegisterMsgCallback();
                }
                inited = true;
            }



        }

        private void RegisterNetWorkEvent()
        {
            network.Dispatcher.AddEventListener(NetWorkEvent.CONNECT, OnNetWorkConnect);
            network.Dispatcher.AddEventListener(NetWorkEvent.CONNECTFAILURE, OnNetWorkConnectFailure);
            network.Dispatcher.AddEventListener(NetWorkEvent.DISCONNECT, OnNetWorkClose);
            network.Dispatcher.AddEventListener(NetWorkEvent.RECONNECTION_TRY, OnNetWorkReconnectionTry);
            network.Dispatcher.AddEventListener(NetWorkEvent.IO_ERROR, OnNetWorkIOError);
            network.Dispatcher.AddEventListener(NetWorkEvent.DATA_ERROR, OnNetWorkDataError);

        }

        private void RegisterMsgCallback()
        {
            RegisterMsgCallback<Tom.LoginOk>(new RequestProtoBufDelegate(this.OnLoginOk));
            RegisterMsgCallback<Tom.ReqLogin>(new RequestProtoBufDelegate(this.OnReqLogin));
        }


        private void RegisterMsgCallback<T>(RequestProtoBufDelegate cb)
        {
            network.RegisterMsgCallback(typeof(T), cb);
        }

        public void Connect(string host, int port)
        {
            if (IsConnected)
            {
                return;
            }

            if (isConnecting)
            {
                return;
            }

            if (host == null)
            {
                return;
            }
            if (host == null || host.Length == 0)
            {
                throw new ArgumentException("Invalid connection host name / IP address");
            }
            if (port < 0 || port > 65535)
            {
                throw new ArgumentException("Invalid connection port");
            }
            lastHost = host;
            isConnecting = true;
            network.Connect(host, port);
        }

        private void OnNetWorkConnect(BaseEvent e)
        {
            Console.WriteLine("Doraemon OnNetWorkConnect");
            Tom.ReqLogin req = new Tom.ReqLogin();
            req.Account = "zxb-1";
            req.Passward = "123-456-11111";
            SendPbMsg(req);
        }

        private void OnNetWorkConnectFailure(BaseEvent e)
        {
            Console.WriteLine("Doraemon OnNetWorkConnectFailure");
        }

        private void OnNetWorkClose(BaseEvent e)
        {
            Console.WriteLine("Doraemon OnTcpReconnectionTry");
        }

        private void OnNetWorkReconnectionTry(BaseEvent e)
        {
            Console.WriteLine("Doraemon OnTcpReconnectionTry");
        }

        private void OnNetWorkIOError(BaseEvent e)
        {
            Console.WriteLine("Doraemon OnNetWorkIOError");

        }

        private void OnNetWorkDataError(BaseEvent e)
        {
            Console.WriteLine("Doraemon OnNetWorkDataError");
        }

        public void AddEventListener(string eventType, EventListenerDelegate listener)
        {
            dispatcher.AddEventListener(eventType, listener);
        }

        public void SendPbMsg(Google.Protobuf.IMessage message)
        {
            
            network.Send(message);
        }

        public void OnLoginOk(INetWorkClient conn, object message)
        {
            Tom.LoginOk rsp = (Tom.LoginOk)message;
            Console.WriteLine(rsp.PlayerBaseInfo.Name + "  " + rsp.PlayerBaseInfo.PlayerID);

            Tom.ReqLogin req = new Tom.ReqLogin();
            req.Account = "zxb-1";
            req.Passward = "123-456-11111";
            SendPbMsg(req);
        }


        public void OnReqLogin(INetWorkClient conn, object message)
        {
            Tom.ReqLogin rsp = (Tom.ReqLogin)message;
            Console.WriteLine(rsp.Account + "  " + rsp.Passward);

        }
    }
}
