using System;

namespace AsyncChatLib.Common
{
    public class ConnectInfo
    {
        #region Variables

        string uniqueID = string.Empty;
        string user = string.Empty;
        string ipAdr = string.Empty;
        int port = 9999;
        string pass = string.Empty;

        #endregion

        #region Propertys

        public string UniqueID { get { return uniqueID; } }
        public string IpAdr { get { return ipAdr; } }
        public string User { get { return user; } }
        public int Port { get { return port; } }
        public string Pass { get { return pass; } }

        #endregion

        public ConnectInfo(string user, string ip, int port, string pass="")
        {
            this.uniqueID = Common.FingerPrint.GetUniqueID();
            this.user = user;
            this.ipAdr = ip;
            this.port = port;
            this.pass = pass;
        }
    }
}
