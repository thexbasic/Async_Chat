using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncChatLib.Common
{
    public class ChatClientMessageWrapper : ChatClientWrapper
    {
        #region Variables

        string message = string.Empty;

        #endregion

        #region Propertys

        public string Message { get { return message; } }

        #endregion

        public ChatClientMessageWrapper(string uniqueID, string name, int permLevel, bool canSeeIPs, string message)
            : base(uniqueID, name, permLevel, canSeeIPs)
        {
            this.message = message;
        }

        public ChatClientMessageWrapper(ChatClientWrapper ccw, string message)
            :base(ccw.UniqueID, ccw.Name, ccw.PermLevel, ccw.CanSeeIPs)
        {
            this.message = message;
        }
    }
}
