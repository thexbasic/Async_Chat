using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncChatLib.Server
{
    public class ChatClient : Common.ChatClientWrapper
    {
        #region Variables

        TcpClient tcpClient;
        NetworkStream stream;
        string encryptKey = "";
        bool authenticated = false;
        DateTime lastPing;

        #endregion

        #region Propertys

        public TcpClient TcpClient { get { return tcpClient; } }
        public string IPAddress { get { return ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();  } }

        #endregion

        // ======== EVENTS ==========
        public delegate void EmptyVoid(ChatClient me);
        public delegate void StringVoid(ChatClient me, string s);
        public delegate void ClientVoid(ChatClient me, Common.ChatClientWrapper target);
        public delegate void MessageVoid(ChatClient me, Common.ChatClientMessageWrapper target);
        // ==========================
        public event EmptyVoid AuthenticatedEvent;
        public event StringVoid DisconnectedEvent;
        public event StringVoid GlobalMessageEvent;
        public event EmptyVoid ClientListRequest;
        public event EmptyVoid BanListRequest;
        public event MessageVoid KickRequest;
        public event MessageVoid BanRequest;
        public event ClientVoid UnbanRequest;
        public event MessageVoid PrivateMessageEvent;
        // ==========================

        /// <summary>
        /// Constructor ............... THX CPTN OBVIOUS
        /// starts async reading the stream..
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="uniqueID"></param>
        /// <param name="name"></param>
        /// <param name="permLevel"></param>
        /// <param name="canSeeIPs"></param>
        public ChatClient(TcpClient tcpClient, string uniqueID, string name, int permLevel, bool canSeeIPs)
            : base(uniqueID, name, permLevel, canSeeIPs)
        {
            this.tcpClient = tcpClient;
            this.stream = tcpClient.GetStream();

            lastPing = DateTime.Now;
            tcpClient.GetStream().BeginRead(new byte[] { 0 }, 0, 0, ReadPacket, null);
        }

        #region Public 

        /// <summary>
        /// Returns the instance of the base Class
        /// </summary>
        /// <returns></returns>
        public Common.ChatClientWrapper GetBase()
        {
            return base.Get();
        }

        /// <summary>
        /// Will perform a check if the Client answered in the last 5 seconds
        /// </summary>
        public void PingCheck()
        {
            TimeSpan lastping = new TimeSpan(DateTime.Now.Ticks - lastPing.Ticks);
            if (lastping.TotalSeconds > 5)
                Disconnect("timeout");
        }

        /// <summary>
        /// A client sent a Public Message 
        /// </summary>
        /// <param name="msg"></param>
        public void PublicMessage(Common.ChatClientMessageWrapper client)
        {
            byte[] queue = Common.ObjectSerializer.Serialize(client);
            SendPacket(4, queue);
        }

        /// <summary>
        /// Client requested the client-list
        /// </summary>
        /// <param name="send"></param>
        public void SendClientList(Common.ChatClientListWrapper send)
        {
            byte[] queue = Common.ObjectSerializer.Serialize(send);
            SendPacket(5, queue);
        }

        /// <summary>
        /// Client requested the ban list
        /// </summary>
        /// <param name="send"></param>
        public void SendbanList(Common.ChatClientListWrapper send)
        {
            byte[] queue = Common.ObjectSerializer.Serialize(send);
            SendPacket(6, queue);
        }

        /// <summary>
        /// CLient Joined <.< Dont want to write this shit anymore
        /// </summary>
        /// <param name="client"></param>
        public void ClientJoined(Common.ChatClientWrapper client)
        {
            byte[] queue = Common.ObjectSerializer.Serialize(client);
            SendPacket(7, queue);
        }

        /// <summary>
        /// GUESS WHAT, A CLIENT LEFT
        /// </summary>
        /// <param name="client"></param>
        public void ClientLeft(Common.ChatClientWrapper client)
        {
            byte[] queue = Common.ObjectSerializer.Serialize(client);
            SendPacket(8, queue);
        }

        /// <summary>
        /// Permission / Name Update
        /// </summary>
        /// <param name="client"></param>
        public void ClientUpdate(Common.ChatClientWrapper client)
        {
            byte[] queue = Common.ObjectSerializer.Serialize(client);
            SendPacket(9, queue);
        }

        /// <summary>
        /// Private Message from a client
        /// </summary>
        /// <param name="client"></param>
        public void PrivateMessage(Common.ChatClientMessageWrapper client)
        {
            byte[] queue = Common.ObjectSerializer.Serialize(client);
            SendPacket(10, queue);
        }

        /// <summary>
        /// Some error occured (mostly permissions)
        /// </summary>
        /// <param name="error"></param>
        public void SendError(string error)
        {
            SendPacket(11, StringToByte(error));
        }

        /// <summary>
        /// Will clsoe the Connection & Rise a Event
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="send"></param>
        public void Disconnect(string reason, bool send=true)
        {
            if (send)
                SendPacket(2, StringToByte(reason));

            tcpClient.Close();
            DisconnectedEvent.Invoke(this, reason);
        }

        #endregion

        #region Private

        /// <summary>
        /// This void will wait until Data is available and then read it
        /// </summary>
        /// <param name="ar"></param>
        private void ReadPacket(IAsyncResult ar)
        {
            try
            {
                int buffer = tcpClient.ReceiveBufferSize;
                if (buffer >= 16)
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
                            case 2: // Disconnect
                                Disconnect(ByteToString(content), false);
                                break;
                            case 3: // Ping
                                lastPing = DateTime.Now;
                                SendPacket(3, content);
                                break;
                            case 4: // Message
                                GlobalMessageEvent.Invoke(this, ByteToString(content));
                                break;
                            case 5: // Client List
                                ClientListRequest.Invoke(this);
                                break;
                            case 6: // Ban List
                                BanListRequest.Invoke(this);
                                break;
                            case 7: // Kick
                                KickRequest.Invoke(this,
                                    (Common.ChatClientMessageWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClientMessage, content));
                                break;
                            case 8: // Ban
                                BanRequest.Invoke(this,
                                    (Common.ChatClientMessageWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClientMessage, content));
                                break;
                            case 9: // Unban
                                UnbanRequest.Invoke(this,
                                    (Common.ChatClientWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClient, content));
                                break;
                            case 10: // Whisper
                                PrivateMessageEvent.Invoke(this,
                                    (Common.ChatClientMessageWrapper)Common.ObjectSerializer.Deserialize
                                      (Common.ObjectSerializer.WrapperType.ChatClientMessage, content));
                                break;
                        }
                    }
                    else
                    {
                        // Serialize Packet -> should be ConnectInfo
                        // Check for bans n shit -> Disconnect
                        // Create Auth-Key
                        // Send Auth-Key "Success"
                        AuthenticatedEvent.Invoke(this);
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
            catch { Disconnect("error writing stream", false); /* Disconnect without sending reason since sending just failed ^ lol */  }
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
