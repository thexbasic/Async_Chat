using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace AsyncChatLib.Common
{
    class ObjectSerializer
    {

        public static byte[] Serialize(object c)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream s = new MemoryStream();
            formatter.Serialize(s, c);
            using (MemoryStream ms = new MemoryStream())
            {
                s.CopyTo(ms);
                return ms.ToArray();
            }
        }

        // Different Wrappers -> used for instantly trying to cast the object after Deserializaion in order to avoid later Exceptions
        public enum WrapperType
        {
            ConnectInfo,
            ChatClient,
            ChatClientMessage,
            ChatClientList
        }

        public static object Deserialize(WrapperType wt, byte[] b)
        {
            Stream stream = new MemoryStream(b);
            IFormatter formatter = new BinaryFormatter();
            switch (wt)
            {
                case WrapperType.ConnectInfo: return (ConnectInfo)formatter.Deserialize(stream);
                case WrapperType.ChatClient: return (ChatClientWrapper)formatter.Deserialize(stream);
                case WrapperType.ChatClientList: return (ChatClientListWrapper)formatter.Deserialize(stream);
                case WrapperType.ChatClientMessage: return (ChatClientMessageWrapper)formatter.Deserialize(stream);
            }
            return null;
        }

    }
}
