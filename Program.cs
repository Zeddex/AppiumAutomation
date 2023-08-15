using System;
using AppiumApp;
using Spectre.Console;
using AdvancedSharpAdbClient.DeviceCommands;

string appPackage = "com.sec.android.app.popupcalculator";
string appActivity = "com.sec.android.app.popupcalculator.Caculator";

Device device = new Device();

string currentDevice = device.SelectDevice();
int port = device.GetFreePort();

App.Init(appPackage, appActivity, currentDevice, port);

Console.ReadLine();

App.CloseApp();