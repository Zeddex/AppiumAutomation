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
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;

namespace AppiumApp
{
    public class App
    {
        public string AppPackage { get; set; }
        public string AppActivity { get; set; }
        public string Udid { get; set; }
        public int Port { get; set; } = 4723;
        public AppiumOptions options { get; set; }
        public AndroidDriver<IWebElement> driver { get; set; }
        public WebDriverWait wait { get; set; }

        public App() : this("", "", "", 4723)
        { }

        public App(string appPackage, string appActivity, string udid, int port)
        {
            AppPackage = appPackage;
            AppActivity = appActivity;
            Udid = udid;
            Port = port;
        }

        public void Start()
        {
            Console.WriteLine("Starting appium...\n");

            var p = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C appium --address 127.0.0.1 --port {Port} --relaxed-security --log-level error",
                }
            };
            p.Start();

            // check appium server is started
            var client = new HttpClient();
            string sessionUrl = $@"http://127.0.0.1:{Port}/sessions";
            HttpResponseMessage result = null;

            while (result == null)
            {
                try
                {
                    result = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, sessionUrl)).Result;
                }
                catch { }

                Thread.Sleep(500);
            }

            Console.WriteLine("\nAppium has been started\n");

            options = new AppiumOptions();
            options.PlatformName = "Android";
            options.AddAdditionalCapability("appium:automationName", "UiAutomator2");
            options.AddAdditionalCapability("appium:noReset", "true");
            options.AddAdditionalCapability("appium:deviceName", "Samsung A50");
            options.AddAdditionalCapability("appium:udid", Udid);
            options.AddAdditionalCapability("appium:appPackage", AppPackage);
            options.AddAdditionalCapability("appium:appActivity", AppActivity);
            options.AddAdditionalCapability("appium:newCommandTimeout", 3000);

            Uri url = new Uri($@"http://127.0.0.1:{Port}");
            driver = new AndroidDriver<IWebElement>(url, options);
        }

        public void CloseApp()
        {
            driver.CloseApp();

            foreach (var process in Process.GetProcessesByName("node"))
            {
                process.Kill();
            }
        }

        public void RunCurrentApp()
        {
            driver.LaunchApp();
        }

        public void RunApp(string appPackage, string appActivity)
        {
            driver.StartActivity(appPackage, appActivity);
        }
		
		public void SetContext(string context = "NATIVE_APP")
		{
			driver.Context = context.ToString();
		}

		public List<string> GetContexts()
		{
			var contexts = driver.Contexts.ToList();

			return contexts;
		}
		
		public void InputText(string text)
        {
            Actions action = new Actions(driver);
            action.SendKeys(text).Perform();
        }

        /// <summary>
        /// Input text by symbol
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pause">Pause between each symbol (in milleseconds)</param>
        public void InputTextBySymbol(string text, int pause = 0)
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
        public void UploadImagesToDevice(string pcDir, bool shuffle, bool deleteAfterUpload, int amount = 0)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string inputDir = currentDir + pcDir;
            string androidDir = "/storage/emulated/0/Download/";

            var dir = new DirectoryInfo(inputDir);

            int filesCount = 0;

            if (shuffle)
            {
                int filesAmount = dir.GetFiles().Length;
                var rndFilesList = Extension.GetRndNumbersFromRange(0, filesAmount, amount);

                for (int i = 0; i < rndFilesList.Count; i++)
                {
                    ++filesCount;

                    int rndFileIndex = rndFilesList[i];
                    var currentFile = dir.GetFiles()[rndFileIndex];
                    driver.PushFile($"{androidDir}{filesCount}.jpeg", currentFile);
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
        public void DeleteImagesFromDevice()
        {
            ExecuteAdbShellCommand($"adb shell cd /storage/emulated/0/Download && rm -rf *.jpeg && rm -rf *.jpg");
        }

        public string TakeElementScreenshot(string xpath)
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

        public void TakeScreenshot()
        {
            var scr = driver.TakeScreenshot();

            string currentDir = Directory.GetCurrentDirectory();
            string filePath = currentDir + @"\screenshot.jpeg";
            scr.SaveAsFile(filePath);
        }
		
		public void TakeScreenshotSystem()
		{
			ExecuteKeyCode(KeyCode.KEYCODE_SYSRQ);
		}

        public string TakeScreenshotByCoord(int topX, int topY, int bottomX, int bottomY)
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

        public int FindElementsAmount(string xPath)
        {
            var elementsList = driver.FindElementsByXPath(xPath);

            int amount = elementsList.Count;

            return amount;
        }

        #region App Navigation

        public string GetElementText(string xpath)
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

        public string TryGetElementText(string xpath)
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

        public void Click(string xpath)
        {
            driver.FindElementByXPath(xpath).Click();
        }

        public void TryClick(string xpath)
        {
            try
            {
                driver.FindElementByXPath(xpath).Click();
            }
            catch { }
        }

        public void ClickCoordinates(int x, int y)
        {
            var action = new TouchAction(driver);

            action.Tap(x, y).Perform();
        }

        public void WaitAndClick(string xpath, int seconds = 10)
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

        public bool FindElement(string xpath)
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

        public bool FindElements(string xpath)
        {
            bool found = false;

            var elements = driver.FindElementsByXPath(xpath);

            if (elements.Count > 0)
            {
                found = true;
            }

            return found;
        }

        public ReadOnlyCollection<IWebElement> GetElementsCollection(string xpath)
        {
            var elements = driver.FindElementsByXPath(xpath);

            return elements;
        }

        public void WaitElement(string xpath, int seconds = 10)
        {
            if (wait == null)
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            }

            wait.Timeout = TimeSpan.FromSeconds(seconds);
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(xpath)));
        }

        public bool TryWaitElement(string xpath, int seconds = 10)
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

        public void SwipeRandom(int fromX, int fromY, int maxOffsetFromX, int maxOffsetFromY, int maxOffsetToX, int maxOffsetToY)
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

        public void Swipe(int fromX, int fromY, int toX, int toY)
        {
            var action = new TouchAction(driver);

            action.Press(fromX, fromY)
                .Wait(500)
                .MoveTo(toX, toY)
                .Release()
                .Perform();
        }

        public void SwipeElement(string elementXpath, int toX, int toY)
        {
            var element = driver.FindElementByXPath(elementXpath);
            var action = new TouchAction(driver);

            action.Press(element)
                .Wait(500)
                .MoveTo(toX, toY)
                .Release()
                .Perform();
        }

        public void SwipeElementRandomOffset(string elementXpath, int maxOffsetToX, int maxOffsetToY)
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

        public void SwipeCenterLeft(int x = 540, int y = 900)
        {
            var action = new TouchAction(driver);

            action.Press(x, y)
                .Wait(500)
                .MoveTo(x - 200, y)
                .Release()
                .Perform();
        }

        public void SwipeCenterRight(int x = 540, int y = 900)
        {
            var action = new TouchAction(driver);

            action.Press(x, y)
                .Wait(500)
                .MoveTo(x + 200, y)
                .Release()
                .Perform();
        }
		
		public void MoveSlider(string elementXpath, int xAxisEndPoint)
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

		public (int xLeft, int yUpper, int xRight, int yLower) GetElementCoord(string elementXpath)
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

        public void SendText(string xpath, string text)
        {
            driver.FindElementByXPath(xpath).SendKeys(text);
        }

        public void ClearField(string xpath)
        {
            driver.FindElementByXPath(xpath).Clear();
        }

        public void PressDigitKey(int digit)
        {
            var digitCode = Extension.DigitToKeyCodeConvert(digit);
            driver.PressKeyCode((int)digitCode);
        }

        public void EnterNumberOnDigitPad(int number)
        {
            int[] digits = number.ToString().Select(digit => int.Parse(digit.ToString())).ToArray();

            foreach (var digit in digits)
            {
                var digitCode = Extension.DigitToKeyCodeConvert(digit);
                driver.PressKeyCode((int)digitCode);
            }
        }

        public void ExecuteKeyCode(KeyCode keyCode)
        {
            driver.PressKeyCode((int)keyCode);
        }

        #endregion


        #region AdbCommands

        /// <summary>
        /// Input text using virtual keyboard (ADBKeyboard)
        /// </summary>
        /// <param name="text"></param>
        public void AdbKeyboardInputText(string text)
		{
			ExecuteAdbShellCommand($"adb shell am broadcast -a ADB_INPUT_TEXT --es msg '{text}'");
		}

		public void AdbInputText(string text)
		{
			ExecuteAdbShellCommand($"adb shell input text '{text}'");
		}

        public void ExecuteAdbShellCommand(string adbShellCommand)
        {
            var args = AdbConvertToAppium(adbShellCommand);

            var output = driver.ExecuteScript("mobile: shell", args);
        }

        public string AdbCurrentActivity()
        {
            var argv = new Dictionary<string, object>();

            argv.Add("command", "dumpsys");
            argv.Add("args", new List<string> { "window", "windows", "|", "grep", "-E", "'mCurrentFocus'" });
            string result = driver.ExecuteScript("mobile: shell", argv).ToString();

            return result;
        }

        public void ToggleAirplaneMode()
        {
            driver.ToggleAirplaneMode();

            Thread.Sleep(2000);
        }

        public void ToggleMobileData()
        {
            driver.ToggleData();

            Thread.Sleep(2000);
        }

        public void AdbToggleWifi()
        {
            driver.ToggleWifi();

            Thread.Sleep(3000);
        }

        private Dictionary<string, object> AdbConvertToAppium(string adbShellCommand)
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
