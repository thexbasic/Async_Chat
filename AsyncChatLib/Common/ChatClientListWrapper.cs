using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncChatLib.Common
{
    public class ChatClientListWrapper
    {
        #region Variables

        private List<ChatClientWrapper> list = new List<ChatClientWrapper>();


        #endregion

        #region Propertys

        public List<ChatClientWrapper> List
        {
            get { return list; }
        }

        #endregion
    }
}
