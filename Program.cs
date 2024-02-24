using System;
using AppiumApp;

string appPackage = "";
string appActivity = "";

Device device = new Device();

string currentDevice = device.SelectDevice();
int port = device.GetFreePort();

App app = new App(appPackage, appActivity, currentDevice, port);
TargetApp targetApp = new TargetApp();
Logger logger = new Logger();
app.Start();

Console.WriteLine($"\nSession has been started. Device {currentDevice} on port {port}");
Console.ReadLine();

app.CloseApp();