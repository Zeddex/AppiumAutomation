using System;
using AppiumApp;
using Spectre.Console;
using AdvancedSharpAdbClient.DeviceCommands;

string appPackage = "com.applisto.appcloner";
string appActivity = "com.applisto.appcloner.activity.MainActivity";

Device device = new Device();

string currentDevice = device.SelectDevice();
int port = device.GetFreePort();

App.Init(appPackage, appActivity, currentDevice, port);

Console.WriteLine();

App.CloseApp();