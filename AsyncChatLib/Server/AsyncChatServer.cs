using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncChatLib.Server
{
    public class AsyncChatServer
    {
        #region Variables

        Thread pingThread;
        Thread listenThread;
        List<TcpListener> listeners = new List<TcpListener>();

        int[] ports = new int[0];
        int maxClients = 50;
        string password = "";

        List<ChatClient> clients = new List<ChatClient>();
        List<Common.ChatClientWrapper> bans = new List<Common.ChatClientWrapper>();

        #endregion

        #region Propertys

        internal List<ChatClient> Clients { get { return clients; } }
        internal List<Common.ChatClientWrapper> Bans { get { return bans; } }

        #endregion

        // ======== EVENTS ==========
        public delegate void EmptyVoid(ChatClient cc);
        public delegate void StringVoid(ChatClient cc, string s);
        public delegate void DoubleVoid(ChatClient cc, ChatClient ce);
        public delegate void WrapperVoid(ChatClient cc, Common.ChatClientWrapper cw);
        // ==========================
        public event EmptyVoid ConnectedEvent;
        public event EmptyVoid AuthenticatedEvent;
        public event StringVoid DisconnectedEvent;
        public event StringVoid GlobalMessageEvent;
        public event DoubleVoid BanEvent;
        public event DoubleVoid KickEvevnt;
        public event WrapperVoid UnbanEvent;
        public event DoubleVoid PrivateMessageEvent;
        // ==========================

        #region Public


        /// <summary>
        /// Start the Server including listeners Etc.
        /// </summary>
        /// <param name="ports"></param>
        /// <param name="maxClients"></param>
        /// <param name="password"></param>
        public void StartServer(int[] ports, int maxClients, string password="")
        {
            StopServer();

            this.ports = ports;
            this.maxClients = maxClients;
            this.password = password;

            // Start Auto-Accepting thread
            listenThread = new Thread(ListenThread);
            listenThread.Start();
            // Start auto-ping-checking the clients
            pingThread = new Thread(PingThread);
            pingThread.Start();
            // Start listener for each port
            foreach (int i in ports)
            {
                TcpListener listener = new TcpListener(IPAddress.Any, i);
                listener.Start();
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Broadcast a message to all clients as "Server"
        /// </summary>
        /// <param name="msg"></param>
        public void Broadcast(string msg)
        {
            foreach (ChatClient cc in clients)
            {
                
            }
        }

        /// <summary>
        /// Disconnects a client with optional reason
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="reason"></param>
        public void Kick(ChatClient cc, string reason="kicked from server")
        {
            clients.Remove(cc);
            cc.AuthenticatedEvent -= c_AuthenticatedEvent;
            cc.DisconnectedEvent -= c_DisconnectedEvent;
            cc.GlobalMessageEvent -= c_GlobalMessageEvent;
            cc.ClientListRequest -= c_ClientListRequest;
            cc.BanListRequest -= c_BanListRequest;
            cc.KickRequest -= c_KickRequest;
            cc.BanRequest -= c_BanRequest;
            cc.UnbanRequest -= c_UnbanRequest;
            cc.PrivateMessageEvent -= c_PrivateMessageEvent;
            cc.Disconnect(reason);
        }

        /// <summary>
        /// Will ban a given user with optional reason
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="reason"></param>
        public void Ban(ChatClient cc, string reason="banned from server")
        {
            bans.Add(cc.GetBase());
            Kick(cc, reason);
        }

        /// <summary>
        /// Will Remove a Client from the banned-list
        /// </summary>
        /// <param name="cc"></param>
        public void Unban(Common.ChatClientWrapper cc)
        {
            bans.Remove(cc);
        }

        /// <summary>
        /// Used to call an client-update if permissions were changed
        /// </summary>
        /// <param name="cc"></param>
        public void ClientUpdate(ChatClient me)
        {
            foreach (ChatClient cc in clients)
            {
                cc.ClientUpdate(me.GetBase());
            }
        }

        /// <summary>
        /// Stop the Server including listeners etc.
        /// </summary>
        public void StopServer()
        {
            // kill listeners
            if (listenThread != null && listenThread.IsAlive)
                listenThread.Abort();
            // stop pinging clients
            if(pingThread != null && pingThread.IsAlive)
                pingThread.Abort();
            // stop each listener
            foreach (TcpListener tcpl in listeners)
                tcpl.Stop();
            listeners.Clear();
            // Disconnect all clients
            while (clients.Count > 0)
                Kick(clients[0], "server shutdown");
        }

        #endregion

        #region Private

        /// <summary>
        /// The Thread that will keep accepting Tcp Clients
        /// </summary>
        private void ListenThread()
        {
            while (true)
            {
                try
                {
                    foreach (TcpListener l in listeners)
                        while (l.Pending())
                        {
                            ChatClient c = new ChatClient(l.AcceptTcpClient(), "", "", 0, false);
                            c.AuthenticatedEvent += c_AuthenticatedEvent;
                            c.DisconnectedEvent += c_DisconnectedEvent;
                            c.GlobalMessageEvent += c_GlobalMessageEvent;
                            c.ClientListRequest += c_ClientListRequest;
                            c.BanListRequest += c_BanListRequest;
                            c.KickRequest += c_KickRequest;
                            c.BanRequest += c_BanRequest;
                            c.UnbanRequest += c_UnbanRequest;
                            c.PrivateMessageEvent += c_PrivateMessageEvent;
                            clients.Add(c);
                        }
                }
                catch (ThreadAbortException) { break; }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// This Thread will check when each client pinged the last time
        /// if the time is > 5 seconds, it will disconnect
        /// </summary>
        private void PingThread()
        {
            while (true)
            {
                try
                {
                    foreach (ChatClient cc in clients)
                        cc.PingCheck();
                    Thread.Sleep(5000);
                }
                catch (ThreadAbortException) { break; }
                catch (Exception) { }
            }
        }

        #endregion

        #region Client-Events

        /// <summary>
        /// Client Successfully authenticated as client
        /// </summary>
        /// <param name="me"></param>
        void c_AuthenticatedEvent(ChatClient me)
        {
            AuthenticatedEvent.Invoke(me);
            foreach (ChatClient cc in clients)
            {
                cc.ClientJoined(me.GetBase());
            }
        }

        /// <summary>
        /// Client Disconnected for whatever reason
        /// </summary>
        /// <param name="me"></param>
        /// <param name="s"></param>
        void c_DisconnectedEvent(ChatClient me, string s)
        {
            DisconnectedEvent.Invoke(me, s);
            foreach (ChatClient cc in clients)
            {
                cc.ClientLeft(me.GetBase());
            }
        }

        /// <summary>
        /// A client sent a public chatmessage
        /// </summary>
        /// <param name="me"></param>
        /// <param name="s"></param>
        void c_GlobalMessageEvent(ChatClient me, string s)
        {
            GlobalMessageEvent.Invoke(me, s);
            Common.ChatClientMessageWrapper ccmw = new Common.ChatClientMessageWrapper(me.GetBase(), s);
            foreach (ChatClient cc in clients)
            {
                cc.PublicMessage(ccmw);
            }
        }

        /// <summary>
        /// Client requested Client List
        /// </summary>
        /// <param name="me"></param>
        void c_ClientListRequest(ChatClient me)
        {
            Common.ChatClientListWrapper send = new Common.ChatClientListWrapper();
            send.List.AddRange(clients);
            me.SendClientList(send);
        }

        /// <summary>
        /// Client requests ban-list
        /// </summary>
        /// <param name="me"></param>
        void c_BanListRequest(ChatClient me)
        {
            if (me.PermLevel < 2) {
                me.SendError("no permissions");
                return;
            }
            Common.ChatClientListWrapper send = new Common.ChatClientListWrapper();
            send.List.AddRange(bans);
            me.SendbanList(send);
        }

        /// <summary>
        /// A client-side Kick request
        /// </summary>
        /// <param name="me"></param>
        /// <param name="target"></param>
        void c_KickRequest(ChatClient me, Common.ChatClientMessageWrapper target)
        {
            if(me.PermLevel < 1) {
                me.SendError("no permissions");
                return;
            }

            string targetUID = target.UniqueID;
            ChatClient targ = null;
            foreach (ChatClient cc in clients)
                if (cc.UniqueID == targetUID)
                    targ = cc;
            if (targ == null)
                me.SendError("invalid client");
            else
                Kick(targ, target.Message);
        }

        /// <summary>
        /// A client-side ban request
        /// </summary>
        /// <param name="me"></param>
        /// <param name="target"></param>
        void c_BanRequest(ChatClient me, Common.ChatClientMessageWrapper target)
        {
            if (me.PermLevel < 2) {
                me.SendError("no permissions");
                return;
            }

            string targetUID = target.UniqueID;
            ChatClient targ = null;
            foreach (ChatClient cc in clients)
                if (cc.UniqueID == targetUID)
                    targ = cc;
            if (targ == null)
                me.SendError("invalid client");
            else
            {
                Ban(targ, target.Message);
                BanEvent.Invoke(me, targ);
            }
        }

        /// <summary>
        /// A client requested unban of another client
        /// </summary>
        /// <param name="me"></param>
        /// <param name="target"></param>
        void c_UnbanRequest(ChatClient me, Common.ChatClientWrapper target)
        {
            if (me.PermLevel < 2)
            {
                me.SendError("no permissions");
                return;
            }
            string targetUID = target.UniqueID;
            Common.ChatClientWrapper targ = null;
            foreach (Common.ChatClientWrapper cc in bans)
                if (cc.UniqueID == targetUID)
                    targ = cc;
            if (targ == null)
                me.SendError("invalid client");
            else
            {
                bans.Remove(targ);
                UnbanEvent.Invoke(me, targ);
            }

        }

        /// <summary>
        /// Client requests Private Message to another client
        /// </summary>
        /// <param name="me"></param>
        /// <param name="target"></param>
        void c_PrivateMessageEvent(ChatClient me, Common.ChatClientMessageWrapper target)
        {
            string targetUID = target.UniqueID;
            ChatClient targ = null;
            foreach (ChatClient cc in clients)
                if (cc.UniqueID == targetUID)
                    targ = cc;
            if (targ == null)
                me.SendError("invalid client");
            else
            {
                Common.ChatClientMessageWrapper ccmw = new Common.ChatClientMessageWrapper(me.GetBase(), target.Message);
                targ.PrivateMessage(ccmw);
                PrivateMessageEvent.Invoke(me, targ);
            }
        }

        #endregion
    }
}
