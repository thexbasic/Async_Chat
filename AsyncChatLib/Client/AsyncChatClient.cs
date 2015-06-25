using System;
using System.Net.Sockets;
using System.Text;

namespace AsyncChatLib.Client
{
    public class AsyncChatClient
    {
        #region Variables

        Pinger pinger = new Pinger();

        TcpClient tcpClient;
        NetworkStream stream;
        bool authenticated = false;
        string encryptKey = "";

        #endregion

        // ======== EVENTS ==========
        public delegate void EmptyVoid();
        public delegate void StringVoid(string s);
        public delegate void ClientVoid(Common.ChatClientWrapper c);
        public delegate void ClientMessagegVoid(Common.ChatClientMessageWrapper c);
        public delegate void ClientListVoid(Common.ChatClientListWrapper c);
        // ==========================
        public event StringVoid LogEvent;
        public event EmptyVoid AuthenticatedEvent;
        public event StringVoid DisconnectedEvent;
        public event ClientMessagegVoid GlobalMessageEvent;
        public event ClientListVoid ClientListEvent;
        public event ClientListVoid BanListEvent;
        public event ClientVoid ClientJoinedEvent;
        public event ClientMessagegVoid ClientLeftEvent;
        public event ClientVoid ClientUpdateEvent;
        public event ClientMessagegVoid PrivateMessageEvent;
        // ==========================

        #region Public Functions

        /// <summary>
        /// Will use the connectinfo trying to connect.
        /// </summary>
        /// <param name="connectInfo"></param>
        public bool Connect(Common.ConnectInfo connectInfo)
        {
            // if still connected, disconnect
            Disconnect("changing server", (tcpClient != null && tcpClient.Connected)); // condition to send is that the client is still connected
            tcpClient = new TcpClient();
            try {
                // try connecting
                tcpClient.Connect(connectInfo.IpAdr, connectInfo.Port);
                if (tcpClient.Connected)
                {
                    // Get & Set Stream
                    stream = tcpClient.GetStream();
                    // initiate Async reading of Stream
                    tcpClient.GetStream().BeginRead(new byte[] { 0 }, 0, 0, ReadPacket, null);
                    // Send auth
                    byte[] queue = Common.ObjectSerializer.Serialize(connectInfo);
                    SendPacket(1, queue);
                }
            }
            catch { }
            return tcpClient.Connected;
        }

        /// <summary>
        /// Will command the server broadcastin this message to all clients
        /// </summary>
        /// <param name="msg"></param>
        public void GlobalMessage(string msg)
        {
            SendPacket(4, StringToByte(msg));
        }

        /// <summary>
        /// Will request The List of currently connected Clients
        /// </summary>
        public void RequestClientList()
        {
            SendPacket(5, new byte[0]);
        }

        /// <summary>
        /// Will request of currently banned clients
        /// </summary>
        public void RequestBanList()
        {
            SendPacket(6, new byte[0]);
        }

        /// <summary>
        /// Will request kicking someone (only works with given permissions)
        /// </summary>
        /// <param name="target"></param>
        public void Kick(Common.ChatClientWrapper target, string reason="kicked from server")
        {
            Common.ChatClientMessageWrapper send = new Common.ChatClientMessageWrapper(target, reason);
            byte[] queue = Common.ObjectSerializer.Serialize(send);
            SendPacket(7, queue);
        }

        /// <summary>
        /// Will request banning someone (only works with given permissions)
        /// </summary>
        /// <param name="target"></param>
        public void Ban(Common.ChatClientWrapper target, string reason="banned from server")
        {
            Common.ChatClientMessageWrapper send = new Common.ChatClientMessageWrapper(target, reason);
            byte[] queue = Common.ObjectSerializer.Serialize(send);
            SendPacket(8, queue);
        }

        /// <summary>
        /// Will request unbanning someone (only works with given permissions)
        /// </summary>
        /// <param name="target"></param>
        public void Unban(Common.ChatClientWrapper target)
        {
            byte[] queue = Common.ObjectSerializer.Serialize(target);
            SendPacket(9, queue);
        }

        /// <summary>
        /// Will send a private message to the given target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="msg"></param>
        public void PrivateMessage(Common.ChatClientWrapper target, string msg)
        {
            Common.ChatClientMessageWrapper send = new Common.ChatClientMessageWrapper(target, msg);
            byte[] queue = Common.ObjectSerializer.Serialize(send);
            SendPacket(10, queue);
        }

        /// <summary>
        /// Will try sending the reason as message and then force close the connection.
        /// After that a DisconnectedEvent will be thrown in order for the instance to know.
        /// </summary>
        /// <param name="reason"></param>
        public void Disconnect(string reason, bool sendReason=true)
        {
            pinger.TimeOutEvent -= pinger_TimeOutEvent;
            pinger.PingEvent -= pinger_PingEvent;
            pinger.Kill();

            // Send the reason why youre disconnecing -> default true
            if (sendReason)
                SendPacket(2, StringToByte(reason));

            tcpClient.Close();
            tcpClient = null;
            DisconnectedEvent(reason);
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// This void will wait until Data is available and then read it
        /// </summary>
        /// <param name="ar"></param>
        private void ReadPacket(IAsyncResult ar)
        {
            try
            {
                int buffer = tcpClient.ReceiveBufferSize;
                if (buffer > 16)
                {
                    // read all data
                    byte[] read = new byte[buffer];
                    stream.Read(read, 0, buffer);
                    // reconstruct package
                    long packLeng = BitConverter.ToInt64(read, 0);
                    long packID = BitConverter.ToInt64(read, 8);
                    byte[] content = new byte[packLeng - 16];
                    Array.Copy(read, 16, content, 0, content.Length);

                    if (authenticated)
                    {
                        // Decrypt
                        content = Common.Encryption.DecryptBytes(content, encryptKey);
                        switch (packID)
                        {
                            case 1: // New Encryption key
                                encryptKey = ByteToString(content);
                                break;
                            case 2: // Disconnect
                                Disconnect(ByteToString(content), false);
                                break;
                            case 3: // Ping Answer -> will be handed to the pinger
                                pinger.PingAnswer(BitConverter.ToInt32(content, 0));
                                break;
                            case 4: // Global Message
                                GlobalMessageEvent.Invoke(
                                    (Common.ChatClientMessageWrapper) Common.ObjectSerializer.Deserialize(Common.ObjectSerializer.WrapperType.ChatClientMessage,content));
                                break;
                            case 5: // CLient List
                                ClientListEvent.Invoke(
                                    (Common.ChatClientListWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClientList, content));
                                break;
                            case 6: // Ban List
                                BanListEvent.Invoke(
                                    (Common.ChatClientListWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClientList, content));
                                break;
                            case 7: // Client join
                               ClientJoinedEvent.Invoke(
                                   (Common.ChatClientWrapper)Common.ObjectSerializer.Deserialize
                                     (Common.ObjectSerializer.WrapperType.ChatClient, content));
                                break;
                            case 8: // Client leave
                                ClientLeftEvent.Invoke(
                                    (Common.ChatClientMessageWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClientMessage, content));
                                break;
                            case 9: // Client Update
                                ClientUpdateEvent.Invoke(
                                    (Common.ChatClientWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClient, content));
                                break;
                            case 10: // Private Message / Whisper
                                PrivateMessageEvent.Invoke(
                                    (Common.ChatClientMessageWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClientMessage, content));
                                break;
                            case 11: // Error Message

                                break;
                        }
                    }
                    else {
                        string reason = "invalid auth response";
                        switch (packID)
                        {
                            case 1:
                                // set the encryption key for future use
                                encryptKey = ByteToString(content);
                                authenticated = true;
                                // start pinging in order to avoid timeouts
                                pinger.TimeOutEvent += pinger_TimeOutEvent;
                                pinger.PingEvent += pinger_PingEvent;
                                pinger.Start();
                                AuthenticatedEvent.Invoke();
                                break;
                            case 2: // Connect failed for whatever reason by Server -> ban ?
                                reason = ByteToString(content);
                                break;
                        }
                        Disconnect(reason, false);
                    }
                }

                if (tcpClient != null && tcpClient.Connected)
                    tcpClient.GetStream().BeginRead(new byte[] { 0 }, 0, 0, ReadPacket, null);
            }
            catch { Disconnect("error reading stream"); }
        }

        /// <summary>
        /// Send the given packet with the given content to the networkstream
        /// </summary>
        /// <param name="packetId"></param>
        /// <param name="content"></param>
        private void SendPacket(long packetId, byte[] content)
        {
            try
            {
                // if connection established -> encrypt content
                if (authenticated)
                    content = Common.Encryption.EncryptBytes(content, encryptKey);

                byte[] outID = BitConverter.GetBytes(packetId);
                long byteLength = content.Length + 16; // 16 since packet is always [8bit-byteLength-long][8bit-packetID-long][content]
                byte[] outLen = BitConverter.GetBytes(byteLength);
                // Write to netstream
                stream.Write(outLen, 0, 8);
                stream.Write(outID, 0, 8);
                stream.Write(content, 0, content.Length);
                stream.Flush();
            }
            catch { Disconnect("", false); /* Disconnect without sending reason since sending just failed ^ lol */  }
        }

        #endregion

        #region Pinger-Events

        /// <summary>
        /// Ping-Event
        /// </summary>
        /// <param name="number"></param>
        void pinger_PingEvent(int number)
        {
            SendPacket(3, BitConverter.GetBytes(number));
        }

        /// <summary>
        /// Will rise if the server didnt answer properly to the ping in 5 seconds
        /// </summary>
        void pinger_TimeOutEvent()
        {
            Disconnect("server didnt answer for 5 seconds.", false);
        }

        #endregion

        #region Utils

        private byte[] StringToByte(string str)
        {
            return ASCIIEncoding.ASCII.GetBytes(str);
        }

        private string ByteToString(byte[] byt)
        {
            return ASCIIEncoding.ASCII.GetString(byt);
        }

        #endregion
    }
}
