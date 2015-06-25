using AsyncChatLib.Client;
using AsyncChatLib.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncChat_Clientbackend
{
    public partial class BanForm : Form
    {
        AsyncChatClient client;
        ChatClientListWrapper bans;

        public BanForm(AsyncChatClient client, ChatClientListWrapper bans)
        {
            this.client = client;
            this.bans = bans;

            InitializeComponent();

            foreach (ChatClientWrapper ccw in bans.List)
            {
                listBox_bans.Items.Add(ccw.Name);
            }
        }

        private void entbannenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox_bans.SelectedItems.Count != 1) return;
            ChatClientWrapper wrapper = null;
            foreach (ChatClientWrapper ccw in bans.List)
            {
                if (ccw.Name == listBox_bans.SelectedItems[0])
                {
                    wrapper = ccw;
                    break;
                }
            }
            if(wrapper != null)
                client.Unban(wrapper);
        }
    }
}
