using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;

namespace AppiumApp
{
    public static class Adb
    {
        public static List<string> ListOfDevices()
        {
            AdbClient client = new AdbClient();

            //var devices = client.GetDevices().Select(x => new {Serial = x.Serial, Model = x.Model}).ToList();
            var devices = client.GetDevices().Select(x => x.Serial).ToList();

            return devices;
        }


    }
}
