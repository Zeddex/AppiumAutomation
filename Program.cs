using System;
using AppiumApp;

string appPackage = "";
string appActivity = "";

Device device = new Device();
Logger logger = new Logger();

string currentDevice = device.SelectDevice();
int port = device.GetFreePort();

App.Init(appPackage, appActivity, currentDevice, port);

Console.WriteLine($"\nSession has been started. Device {currentDevice} on port {port}");
Console.ReadLine();

App.CloseApp();