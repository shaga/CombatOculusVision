using System;
using System.Collections;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;

namespace CombatController
{
    public delegate void Action();

    public class WebServerCommandReceiver : IDisposable
    {
        private const int DefaultPort = 80;

        private const string ResHeaderInfo = "HTTP/1.1 200 OK\r\nContent-Type: text; charset=UTF-8\r\n";
        private const string ResHeaderLen = "Content-Length: ";
        private const string ResHeaderConnection = "\r\nConnection: close\r\n\r\n";

        private Socket Socket { get; set; }

        private class CommandAction
        {
            public char Command { get; private set; }
            public Action Action { get; private set; }

            public CommandAction(char command, Action action)
            {
                Command = command;
                Action = action;
            }
        }

        private ArrayList CommandArray { get; set; }

        public WebServerCommandReceiver(int port = DefaultPort)
        {
            CommandArray = new ArrayList();

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(new IPEndPoint(IPAddress.Any, port));
            Socket.Listen(10);
        }

        ~WebServerCommandReceiver()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Socket == null)
            {
                return;
            }

            Socket.Close();
        }

        public void AddAction(char command, Action action)
        {
            for (var i = 0; i < CommandArray.Count; i++)
            {
                if (command != ((CommandAction) CommandArray[i]).Command) continue;
                CommandArray.RemoveAt(i);
                break;
            }

            CommandArray.Add(new CommandAction(command, action));
        }

        public bool WaitForRequest()
        {
            while (true)
            {
                using (var clientSocket = Socket.Accept())
                {
                    var result = ReceiveCommand(clientSocket);

                    var response = ResHeaderInfo + ResHeaderLen + result.ToString().Length + ResHeaderConnection +
                                   result;

                    clientSocket.Send(Encoding.UTF8.GetBytes(response), SocketFlags.None);
                }
            }
        }

        public bool ReceiveCommand(Socket socket)
        {
            if (socket == null)
            {
                Debug.Print("Client Socket is null");
                return false;
            }

            var length = socket.Available;

            if (length == 0)
            {
                Debug.Print("Socket Request Length:0");
                return false;
            }

            var buffer = new byte[length];
            var count = socket.Receive(buffer, length, SocketFlags.None);

            if (count == 0)
            {
                Debug.Print("Socket Receive Length:0");
                return false;
            }

            var requestString = new string(Encoding.UTF8.GetChars(buffer));

            var requestArray = requestString.Split('\n');

            if (requestArray.Length == 0)
            {
                Debug.Print("Bad Request received");
                return false;
            }

            var lastIdx = requestArray[0].IndexOf("HTTP");

            if (lastIdx < 0)
            {
                Debug.Print("Http not found");
                return false;
            }

            var data = requestArray[0].Substring(5, lastIdx - 1).Trim();

            if (data.Length == 0)
            {
                Debug.Print("Command not found");
                return false;
            }

            return RunAction(data[0]);
        }

        private bool RunAction(char command)
        {
            foreach (CommandAction commandInfo in CommandArray)
            {
                if (commandInfo.Command == command)
                {
                    commandInfo.Action();
                    return true;
                }
            }

            Debug.Print("Command:" + command + " is not registered.");
            return false;
        }
    }
}
