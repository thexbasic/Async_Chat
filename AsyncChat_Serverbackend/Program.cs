using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncChat_Serverbackend
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncChatLib.Server.AsyncChatServer server = new AsyncChatLib.Server.AsyncChatServer();

            server.AuthenticatedEvent += server_AuthenticatedEvent;
            server.BanEvent += server_BanEvent;
            server.ConnectedEvent += server_ConnectedEvent;
            server.DisconnectedEvent += server_DisconnectedEvent;
            server.GlobalMessageEvent += server_GlobalMessageEvent;
            server.KickEvevnt += server_KickEvevnt;
            server.PrivateMessageEvent += server_PrivateMessageEvent;
            server.UnbanEvent += server_UnbanEvent;

            server.StartServer( new int[]{60000}, 100, "" );

            Console.WriteLine("Server running on port 60000 now.\nWaiting for clients...");
        }

        static void server_UnbanEvent(AsyncChatLib.Server.ChatClient cc, AsyncChatLib.Common.ChatClientWrapper cw)
        {
            Console.WriteLine(cc.Name + " unbanned " + cw.Name);
        }

        static void server_PrivateMessageEvent(AsyncChatLib.Server.ChatClient cc, AsyncChatLib.Server.ChatClient ce)
        {
            Console.WriteLine(cc.Name + " sent PM to " + ce.Name);
        }

        static void server_KickEvevnt(AsyncChatLib.Server.ChatClient cc, AsyncChatLib.Server.ChatClient ce)
        {
            Console.WriteLine(cc.Name + " kicked " + ce.Name);
        }

        static void server_GlobalMessageEvent(AsyncChatLib.Server.ChatClient cc, string s)
        {
            Console.WriteLine(cc.Name + " wrote >> " + s);
        }

        static void server_DisconnectedEvent(AsyncChatLib.Server.ChatClient cc, string s)
        {
            Console.WriteLine(cc.Name + " disconnected. (" + s + ")");
        }

        static void server_ConnectedEvent(AsyncChatLib.Server.ChatClient cc)
        {
            Console.WriteLine(cc.IPAddress + " connected and is waiting for auth.");
        }

        static void server_BanEvent(AsyncChatLib.Server.ChatClient cc, AsyncChatLib.Server.ChatClient ce)
        {
            Console.WriteLine(cc.Name + " banned " + ce.Name);
        }

        static void server_AuthenticatedEvent(AsyncChatLib.Server.ChatClient cc)
        {
            Console.WriteLine(cc.Name + " authenticated.");
        }
    }
}
