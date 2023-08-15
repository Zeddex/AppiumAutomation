using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Support.UI;
using System.Net;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Spectre.Console;

namespace AppiumApp
{
    public static class App
    {
        private static AppiumOptions options;
        public static AndroidDriver<IWebElement> driver;
        public static WebDriverWait wait;

        public static void Init()
        {
            Init("", "" , "");
        }

        public static void Init(string appPackage, string appActivity, string udid, int port = 4723, bool noReset = true, string deviceName = "Samsung A50")
        {
            Console.WriteLine("Starting appium...\n");

            var p = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C appium --address 127.0.0.1 --port {port} --relaxed-security --log-level error",
                }
            };
            p.Start();

            // check appium server is started
            var request = WebRequest.Create($"http://127.0.0.1:{port}/sessions");
            request.Method = "HEAD";
            WebResponse? response = null;

            while (response == null)
            {
                try
                {
                    response = request.GetResponse();
                }
                catch { }

                Thread.Sleep(500);
            }

            options = new AppiumOptions();
            options.PlatformName = "Android";
            options.AddAdditionalCapability("appium:automationName", "UiAutomator2");
            options.AddAdditionalCapability("appium:noReset", noReset);
            options.AddAdditionalCapability("appium:udid", udid);
            options.AddAdditionalCapability("appium:deviceName", deviceName);
            options.AddAdditionalCapability("appium:appPackage", appPackage);
            options.AddAdditionalCapability("appium:appActivity", appActivity);
            options.AddAdditionalCapability("appium:newCommandTimeout", 3000);

            Uri url = new Uri($"http://127.0.0.1:{port}");

            driver = new AndroidDriver<IWebElement>(url, options);
        }

        public static bool isPortAvailable(int port)
        {
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

        public static void CloseApp()
        {
            driver.CloseApp();

            foreach (var process in Process.GetProcessesByName("node"))
            {
                process.Kill();
            }
        }

        public static void RunCurrentApp()
        {
            driver.LaunchApp();
        }

        public static void RunApp(string appPackage, string appActivity)
        {
            driver.StartActivity(appPackage, appActivity);
        }
		
		public static void SetContext(string context = "NATIVE_APP")
		{
			driver.Context = context.ToString();
		}

		public static List<string> GetContexts()
		{
			var contexts = driver.Contexts.ToList();

			return contexts;
		}
		
		public static void InputText(string text)
        {
            Actions action = new Actions(driver);
            action.SendKeys(text).Perform();
        }

        /// <summary>
        /// Input text by symbol
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pause">Pause between each symbol (in milleseconds)</param>
        public static void InputTextBySymbol(string text, int pause = 0)
        {
            Actions action = new Actions(driver);

            foreach (var symbol in text)
            {
                InputText(symbol.ToString());
                Thread.Sleep(pause);
            }
        }

        /// <summary>
        /// Upload images to device from PC
        /// </summary>
        /// <param name="pcDir">path to img directory on computer within working directory</param>
        /// <param name="shuffle">mix images before uploading</param>
        /// <param name="deleteAfterUpload">delete photos from pc after uploading</param>
        /// <param name="amount">0 for all photos uploading</param>
        public static void UploadImagesToDevice(string pcDir, bool shuffle, bool deleteAfterUpload, int amount = 0)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string inputDir = currentDir + pcDir;
            string androidDir = "/storage/emulated/0/Download/";

            var dir = new DirectoryInfo(inputDir);

            int filesCount = 0;

            if (shuffle)
            {
                int filesAmount = dir.GetFiles().Length;
                var rndFilesList = Extension.GetRndNumbersFromRange(0, filesAmount, filesAmount);

                foreach (var file in dir.GetFiles())
                {
                    ++filesCount;
                    driver.PushFile($"{androidDir}{rndFilesList[filesCount]}.jpeg", file);

                    if (filesCount == amount)
                    {
                        break;
                    }
                }
            }

            else
            {
                foreach (var file in dir.GetFiles())
                {
                    ++filesCount;
                    driver.PushFile($"{androidDir}{filesCount}.jpeg", file);

                    if (filesCount == amount)
                    {
                        break;
                    }
                }
            }

            filesCount = 0;

            if (deleteAfterUpload)
            {
                foreach (var file in dir.GetFiles())
                {
                    ++filesCount;
                    file.Delete();

                    if (filesCount == amount)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Delete images from Download dir
        /// </summary>
        public static void DeleteImagesFromDevice()
        {
            ExecuteAdbShellCommand($"adb shell cd /storage/emulated/0/Download && rm -rf *.jpeg && rm -rf *.jpg");
        }

        public static string TakeElementScreenshot(string xpath)
        {
            var el = driver.FindElementByXPath(xpath);

            int x = el.Location.X;
            int y = el.Location.Y;
            int h = el.Size.Height;
            int w = el.Size.Width;

            var screenshot = driver.TakeScreenshot();

            string screenshotBase64 = screenshot.AsBase64EncodedString;
            string croppedImg64 = Extension.CropBase64Image(screenshotBase64, x, y, h, w);

            return croppedImg64;
        }

        public static void TakeScreenshot()
        {
            var scr = driver.TakeScreenshot();

            string currentDir = Directory.GetCurrentDirectory();
            string filePath = currentDir + @"\screenshot.jpeg";
            scr.SaveAsFile(filePath);
        }
		
		public static void TakeScreenshotSystem()
		{
			ExecuteKeyCode(KeyCode.KEYCODE_SYSRQ);
		}

        public static string TakeScreenshotByCoord(int topX, int topY, int bottomX, int bottomY)
        {
            int x = topX;
            int y = topY;
            int h = bottomY - topY;
            int w = bottomX - topX;

            var screenshot = driver.TakeScreenshot();

            string screenshotBase64 = screenshot.AsBase64EncodedString;
            string croppedImg64 = Extension.CropBase64Image(screenshotBase64, x, y, h, w);

            return croppedImg64;
        }

        public static int FindElementsAmount(string xPath)
        {
            var elementsList = driver.FindElementsByXPath(xPath);

            int amount = elementsList.Count;

            return amount;
        }

        #region App Navigation

        public static string GetElementText(string xpath)
        {
            try
            {
                var el = driver.FindElementByXPath(xpath);
                string text = el.Text;
                return text;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static string TryGetElementText(string xpath)
        {
            string result = string.Empty;

            try
            {
                var el = driver.FindElementByXPath(xpath);
                result = el.Text;

                return result;
            }
            catch
            {
                return result;
            }
        }

        public static void Click(string xpath)
        {
            driver.FindElementByXPath(xpath).Click();
        }

        public static void TryClick(string xpath)
        {
            try
            {
                driver.FindElementByXPath(xpath).Click();
            }
            catch { }
        }

        public static void ClickCoordinates(int x, int y)
        {
            var action = new TouchAction(driver);

            action.Tap(x, y).Perform();
        }

        public static void WaitAndClick(string xpath, int seconds = 10)
        {
            WaitElement(xpath, seconds);

            try
            {
                driver.FindElementByXPath(xpath).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static bool FindElement(string xpath)
        {
            IWebElement element = null;

            bool found = false;

            try
            {
                element = driver.FindElementByXPath(xpath);
            }
            catch { }


            if (element != null)
            {
                found = true;
            }

            return found;
        }

        public static bool FindElements(string xpath)
        {
            bool found = false;

            var elements = driver.FindElementsByXPath(xpath);

            if (elements.Count > 0)
            {
                found = true;
            }

            return found;
        }

        public static ReadOnlyCollection<IWebElement> GetElementsCollection(string xpath)
        {
            var elements = driver.FindElementsByXPath(xpath);

            return elements;
        }

        public static void WaitElement(string xpath, int seconds = 10)
        {
            if (wait == null)
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            }

            wait.Timeout = TimeSpan.FromSeconds(seconds);
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(xpath)));
        }

        public static bool TryWaitElement(string xpath, int seconds = 10)
        {
            if (wait == null)
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            }

            wait.Timeout = TimeSpan.FromSeconds(seconds);

            try
            {
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(xpath)));
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static void SwipeRandom(int fromX, int fromY, int maxOffsetFromX, int maxOffsetFromY, int maxOffsetToX, int maxOffsetToY)
        {
            var rnd = new Random();

            var action = new TouchAction(driver);

            fromX += rnd.Next(-maxOffsetFromX, maxOffsetFromX);
            fromY += rnd.Next(-maxOffsetFromY, maxOffsetFromY);

            action.Press(fromX, fromY)
                .Wait(500)
                .MoveTo(fromX + rnd.Next(-maxOffsetToX, maxOffsetToX), fromY + rnd.Next(-maxOffsetToY, maxOffsetToY))
                .Release()
                .Perform();
        }

        public static void Swipe(int fromX, int fromY, int toX, int toY)
        {
            var action = new TouchAction(driver);

            action.Press(fromX, fromY)
                .Wait(500)
                .MoveTo(toX, toY)
                .Release()
                .Perform();
        }

        public static void SwipeElement(string elementXpath, int toX, int toY)
        {
            var element = driver.FindElementByXPath(elementXpath);
            var action = new TouchAction(driver);

            action.Press(element)
                .Wait(500)
                .MoveTo(toX, toY)
                .Release()
                .Perform();
        }

        public static void SwipeElementRandomOffset(string elementXpath, int maxOffsetToX, int maxOffsetToY)
        {
            var element = driver.FindElementByXPath(elementXpath);
            int xLeft = element.Location.X;
            int xRight = xLeft + element.Size.Width;
            int xMiddle = (xLeft + xRight) / 2;
            int yUpper = element.Location.Y;
            int yLower = yUpper + element.Size.Height;
            int yMiddle = (yUpper + yLower) / 2;

            var action = new TouchAction(driver);

            var rnd = new Random();

            action.Press(element)
                .Wait(500)
                .MoveTo(xMiddle + rnd.Next(-maxOffsetToX, maxOffsetToX), yMiddle + rnd.Next(-maxOffsetToY, maxOffsetToY))
                .Release()
                .Perform();
        }

        public static void SwipeCenterLeft(int x = 540, int y = 900)
        {
            var action = new TouchAction(driver);

            action.Press(x, y)
                .Wait(500)
                .MoveTo(x - 200, y)
                .Release()
                .Perform();
        }

        public static void SwipeCenterRight(int x = 540, int y = 900)
        {
            var action = new TouchAction(driver);

            action.Press(x, y)
                .Wait(500)
                .MoveTo(x + 200, y)
                .Release()
                .Perform();
        }
		
		public static void MoveSlider(string elementXpath, int xAxisEndPoint)
        {
            var slider = driver.FindElementByXPath(elementXpath);
            int xAxisStartPoint = slider.Location.X;
            int yAxis = slider.Location.Y;

            var action = new TouchAction(driver);

            action.Press(xAxisStartPoint, yAxis)
                .Wait(500)
                .MoveTo(xAxisEndPoint, yAxis)
                .Release()
                .Perform();
        }

		public static (int xLeft, int yUpper, int xRight, int yLower) GetElementCoord(string elementXpath)
        {
            var element = driver.FindElementByXPath(elementXpath);

            int xLeft = element.Location.X;
            int xRight = xLeft + element.Size.Width;
            int xMiddle = (xLeft + xRight) / 2;
            int yUpper = element.Location.Y;
            int yLower = yUpper + element.Size.Height;
            int yMiddle = (yUpper + yLower) / 2;

            return (xLeft, yUpper, xRight, yLower);
        }

        public static void SendText(string xpath, string text)
        {
            driver.FindElementByXPath(xpath).SendKeys(text);
        }

        public static void ClearField(string xpath)
        {
            driver.FindElementByXPath(xpath).Clear();
        }

        public static void PressDigitKey(int digit)
        {
            var digitCode = Extension.DigitToKeyCodeConvert(digit);
            driver.PressKeyCode((int)digitCode);
        }

        public static void ExecuteKeyCode(KeyCode keyCode)
        {
            driver.PressKeyCode((int)keyCode);
        }

        #endregion


        #region AdbCommands

        /// <summary>
        /// Input text using virtual keyboard (ADBKeyboard)
        /// </summary>
        /// <param name="text"></param>
        public static void AdbKeyboardInputText(string text)
		{
			ExecuteAdbShellCommand($"adb shell am broadcast -a ADB_INPUT_TEXT --es msg '{text}'");
		}

		public static void AdbInputText(string text)
		{
			ExecuteAdbShellCommand($"adb shell input text '{text}'");
		}

        public static void ExecuteAdbShellCommand(string adbShellCommand)
        {
            var args = AdbConvertToAppium(adbShellCommand);

            var output = driver.ExecuteScript("mobile: shell", args);
        }

        public static string AdbCurrentActivity()
        {
            var argv = new Dictionary<string, object>();

            argv.Add("command", "dumpsys");
            argv.Add("args", new List<string> { "window", "windows", "|", "grep", "-E", "'mCurrentFocus'" });
            string result = driver.ExecuteScript("mobile: shell", argv).ToString();

            return result;
        }

        public static void ToggleAirplaneMode()
        {
            driver.ToggleAirplaneMode();

            Thread.Sleep(2000);
        }

        public static void ToggleMobileData()
        {
            driver.ToggleData();

            Thread.Sleep(2000);
        }

        public static void AdbToggleWifi()
        {
            driver.ToggleWifi();

            Thread.Sleep(3000);
        }

        private static Dictionary<string, object> AdbConvertToAppium(string adbShellCommand)
        {
            var map = new Dictionary<string, object>();
            var parameters = new List<string>();

            var args = adbShellCommand.Split(' ');

            map.Add("command", args[2]);

            if (args.Length > 3)
            {
                for (int i = 3; i < args.Length; i++)
                {
                    parameters.Add(args[i]);
                }
            }

            map.Add("args", parameters);

            return map;
        }

        #endregion

    }
}
