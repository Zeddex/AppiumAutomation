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

namespace AppiumApp
{
    public static class App
    {
        private static AppiumOptions options;
        public static AndroidDriver<IWebElement> driver;
        public static WebDriverWait wait;

        public static void Init()
        {
            Init("", "");
        }

        public static void Init(string appPackage, string appActivity, bool noReset = true, string deviceName = "Samsung A50")
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = "/C appium --address 127.0.0.1 --port 4723 --relaxed-security",
                }
            };
            p.Start();

            // check appium server is started
            var request = WebRequest.Create("http://127.0.0.1:4723/sessions");
            request.Method = "HEAD";
            WebResponse response = null;

            while (response == null)
            {
                try
                {
                    response = request.GetResponse();
                }
                catch { }

                Thread.Sleep(1000);
            }

            options = new AppiumOptions();
            options.PlatformName = "Android";
            options.AddAdditionalCapability("appium:automationName", "UiAutomator2");
            options.AddAdditionalCapability("appium:noReset", noReset);
            options.AddAdditionalCapability("appium:deviceName", deviceName);
            options.AddAdditionalCapability("appium:appPackage", appPackage);
            options.AddAdditionalCapability("appium:appActivity", appActivity);
            options.AddAdditionalCapability("appium:newCommandTimeout", 3000);

            driver = new AndroidDriver<IWebElement>(new Uri("http://127.0.0.1:4723"), options);
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

        public static void UploadImagesToDevice(string pcDir, bool shuffle)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string inputDir = currentDir + pcDir;
            string androidDir = "/storage/emulated/0/Download/";

            var dir = new DirectoryInfo(inputDir);

            int filesCount = 0;

            if (shuffle)
            {
                int filesAmount = dir.GetFiles().Length;
                var rndFilesList = Extension.GetRndNumbersFromRange(1, filesAmount, filesAmount);

                foreach (var file in dir.GetFiles())
                {
                    driver.PushFile($"{androidDir}{rndFilesList[filesCount]}.jpeg", file);

                    filesCount++;
                }
            }

            else
            {
                foreach (var file in dir.GetFiles())
                {
                    ++filesCount;

                    driver.PushFile($"{androidDir}{filesCount}.jpeg", file);
                }
            }
        }

        public static void DelImagesFromDevice(int imagesAmount)
        {
            for (int i = 1; i <= imagesAmount; i++)
            {
                try
                {
                    ExecuteAdbShellCommand($"adb shell rm /storage/emulated/0/Download/{i}.jpeg");
                }
                catch { }
            }
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

        public static void Scroll(int fromX, int fromY, int toX, int toY)
        {
            var action = new TouchAction(driver);

            action.LongPress(fromX, fromY)
                .MoveTo(toX, toY)
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

            Thread.Sleep(3000);
        }

        public static void ToggleMobileData()
        {
            driver.ToggleData();

            Thread.Sleep(3000);

            //TryClick("//android.widget.CheckBox[@resource-id='androidhwext:id/warning_to_pdp_never_notify' and @text='Do not remind again']");
            //TryClick("//android.widget.Button[@resource-id='android:id/button1' and @text='YES']");
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
