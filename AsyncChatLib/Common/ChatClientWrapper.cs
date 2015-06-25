using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncChatLib.Common
{
    public class ChatClientWrapper
    {
        #region Variables

        string uniqueID;
        string name;
        // Permisssions
        int permLevel = 0;  // 0 = nothing, 1 = kick, 2 = ban
        bool canSeeIPs = false; // Other ips-addresses visible for client

        #endregion

        #region Propertys

        public string UniqueID { get { return uniqueID; } set { uniqueID = value; } }
        public string Name { get { return name; } set { name = value; } }
        public int PermLevel { get { return permLevel; } set { permLevel = value; } }
        public bool CanSeeIPs { get { return canSeeIPs; } set { canSeeIPs = value; } }

        #endregion

        public ChatClientWrapper(string uniqueID, string name, int permLevel, bool canSeeIPs)
        {
            this.uniqueID = uniqueID;
            this.name = name;
            this.permLevel = permLevel;
            this.canSeeIPs = canSeeIPs;
        }

        public ChatClientWrapper Get()
        {
            return this;
        }
    }
}
