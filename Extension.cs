using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AppiumApp
{
    public class Extension
    {
        public static string ExtractDataValue(string json, string key)
        {
            string value = null;

            try
            {
                dynamic data = JObject.Parse(json);
                value = data[key];
            }

            catch { }

            return value;
        }

        public static KeyCode DigitToKeyCodeConvert(int digit)
        {
            var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), $"KeyCode_{digit}");

            return keyCode;
        }

        public static List<int> GetRndNumbersFromRange(int startRange, int endRange, int numbersAmount)
        {
            var rnd = new Random();

            var numbers = new List<int>();

            while (numbers.Count != numbersAmount)
            {
                int currentNumber = rnd.Next(startRange, endRange + 1);

                if (!numbers.Contains(currentNumber))
                {
                    numbers.Add(currentNumber);
                }
            }

            return numbers;
        }

        public static string CropBase64Image(string base64, int x, int y, int height, int width)
        {
            byte[] bytes = Convert.FromBase64String(base64);

            using (var ms = new MemoryStream(bytes))
            {
                Bitmap bmp = new Bitmap(ms);
                Rectangle rect = new Rectangle(x, y, width, height);

                Bitmap croppedBitmap = new Bitmap(rect.Width, rect.Height, bmp.PixelFormat);

                using (Graphics gfx = Graphics.FromImage(croppedBitmap))
                {
                    gfx.DrawImage(bmp, 0, 0, rect, GraphicsUnit.Pixel);
                }

                using (MemoryStream ms2 = new MemoryStream())
                {
                    croppedBitmap.Save(ms2, ImageFormat.Jpeg);
                    byte[] byteImage = ms2.ToArray();
                    var croppedBase64 = Convert.ToBase64String(byteImage);
                    return croppedBase64;
                }
            }
        }

        public static string CreateMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }
    }
}
