using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace AppiumApp
{
    public class Device
    {
        public string SelectDevice()
        {
            var devicesList = Adb.ListOfDevices();

            var device = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[underline blue]Select device:[/]")
                    .AddChoices(devicesList));

            return device;
        }

        public int GetFreePort()
        {
            int startingPort = 4723;
            int freePort;
            bool isPortAvailable;

            do
            {
                isPortAvailable = IsPortAvailable(startingPort);
                freePort = startingPort;
                startingPort++;

            } while (!isPortAvailable);

            return freePort;
        }

        public static bool IsPortAvailable(int port)
        {
            Thread.Sleep(7000);

            bool isPortAvailable = true;

            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isPortAvailable = false;
                    break;
                }
            }

            return isPortAvailable;
        }
    }
}
