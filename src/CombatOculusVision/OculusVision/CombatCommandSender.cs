using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Converters;

namespace OculusVision
{
    public static class CombatCommandSender
    {
        public enum ECombatCommand
        {
            Stop,
            Fore,
            Back,
            Left,
            Right,
        }

        private const string CommandUrlFmt = "http://192.168.3.223/{0}";
        private static readonly char[] CommandChar = {'s', 'f', 'b', 'l', 'r'};

        public static bool SendCommandStop()
        {
            return SendCommand(ECombatCommand.Stop);
        }

        public static bool SendCommandFore()
        {
            return SendCommand(ECombatCommand.Fore);
        }

        public static bool SendCommandBack()
        {
            return SendCommand(ECombatCommand.Back);
        }

        public static bool SendCommandLeft()
        {
            return SendCommand(ECombatCommand.Left);
        }

        public static bool SendCommandRight()
        {
            return SendCommand(ECombatCommand.Right);
        }

        private static bool SendCommand(ECombatCommand command)
        {
            var commandNo = (int) command;

            if (commandNo < 0 || CommandChar.Length <= commandNo)
            {
                throw new ArgumentException();
            }

            var url = string.Format(CommandUrlFmt, CommandChar[commandNo]);

            var request = WebRequest.Create(url) as HttpWebRequest;

            HttpWebResponse response = null;

            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException e)
            {
                response = e.Response as HttpWebResponse;
            }
            catch (Exception e)
            {
                return false;
            }

            if (response == null) return false;

            var reader = new StreamReader(response.GetResponseStream());
            var result = reader.ReadToEnd();

            var ret = Convert.ToBoolean(result);

            return ret;
        }
    }
}
