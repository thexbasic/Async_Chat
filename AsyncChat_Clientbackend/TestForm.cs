using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AsyncChatLib.Client;
using AsyncChatLib.Common;

namespace AsyncChat_Clientbackend
{
    public partial class TestForm : Form
    {

        AsyncChatClient client = new AsyncChatClient();

        public TestForm()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectInfo info = new ConnectInfo(textBox_username.Text,
                                                   textBox_address.Text,
                                                   (int)numericUpDown_port.Value,
                                                   textBox_password.Text);
                if (client.Connect(info))
                {
                    client.AuthenticatedEvent += client_AuthenticatedEvent;
                    client.BanListEvent += client_BanListEvent;
                    client.ClientJoinedEvent += client_ClientJoinedEvent;
                    client.ClientLeftEvent += client_ClientLeftEvent;
                    client.ClientListEvent += client_ClientListEvent;
                    client.ClientUpdateEvent += client_ClientUpdateEvent;
                    client.DisconnectedEvent += client_DisconnectedEvent;
                    client.GlobalMessageEvent += client_GlobalMessageEvent;
                    client.LogEvent += client_LogEvent;
                    client.PrivateMessageEvent += client_PrivateMessageEvent;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region Client-Events

        /// <summary>
        /// Private message from another user
        /// </summary>
        /// <param name="c"></param>
        void client_PrivateMessageEvent(ChatClientMessageWrapper c)
        {
            Log(string.Format("WHISPER: {0} - {1}", c.Name, c.Message));
        }

        /// <summary>
        /// Some Log
        /// </summary>
        /// <param name="s"></param>
        void client_LogEvent(string s)
        {
            Log("LOG: " + s);
        }

        /// <summary>
        /// Global message from another user
        /// </summary>
        /// <param name="c"></param>
        void client_GlobalMessageEvent(ChatClientMessageWrapper c)
        {
            Log(string.Format("CHAT: {0} - {1}", c.Name, c.Message));
        }

        /// <summary>
        /// We got disconnecetd
        /// </summary>
        /// <param name="s"></param>
        void client_DisconnectedEvent(string s)
        {
            MessageBox.Show("Disconnected.\n\n" + s);
        }

        /// <summary>
        /// A client got updated.. permissions, name w/e
        /// </summary>
        /// <param name="c"></param>
        void client_ClientUpdateEvent(ChatClientWrapper c)
        {
            Log("UPDATE: " + c.UniqueID);
        }

        /// <summary>
        /// Client-List was received
        /// </summary>
        /// <param name="c"></param>
        void client_ClientListEvent(ChatClientListWrapper c)
        {
            listBox_clients.Items.Clear();
            foreach (ChatClientWrapper ccw in c.List)
            {
                listBox_clients.Items.Add(ccw.Name);
            }
            Log("Clientlist-Update");
        }

        /// <summary>
        /// Another client disconnected
        /// </summary>
        /// <param name="c"></param>
        void client_ClientLeftEvent(ChatClientMessageWrapper c)
        {
            listBox_clients.Items.Remove(c.Name);
            Log("DISCONNECT: " + c.Name);
        }

        /// <summary>
        /// Another client joined
        /// </summary>
        /// <param name="c"></param>
        void client_ClientJoinedEvent(ChatClientWrapper c)
        {
            listBox_clients.Items.Add(c.Name);
            Log("CONNECTED: " + c.Name);
        }

        /// <summary>
        /// Banlist was received
        /// </summary>
        /// <param name="c"></param>
        void client_BanListEvent(ChatClientListWrapper c)
        {
            new BanForm(client, c).Show();
        }

        /// <summary>
        /// Client is conected and ready for commands
        /// </summary>
        void client_AuthenticatedEvent()
        {
            Log("AUTH: Success");
        }

        #endregion

        /// <summary>
        /// Disconnect from da servur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Log something to chat
        /// </summary>
        /// <param name="msg"></param>
        private void Log(string msg)
        {
            richTextBox_chat.Text += string.Format("[{0}] - {1}\n", DateTime.Now.ToString("hh:mm:ss"), msg);
            
        }
    }
}
