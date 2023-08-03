using System;
using AppiumApp;
using Spectre.Console;
using AdvancedSharpAdbClient.DeviceCommands;

string appPackage = "com.applisto.appcloner";
string appActivity = "com.applisto.appcloner.activity.MainActivity";

//AnsiConsole.MarkupLine("[underline blue]Select device:[/]");

string currentDevice = SelectDevice();

string SelectDevice()
{
    var devicesList = Adb.ListOfDevices();

    var device = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[underline blue]Select device:[/]")
            .PageSize(10)
            .AddChoices(devicesList));

    return device;
}




Thread.Sleep(1000);

//App.Init(appPackage, appActivity);
App.Init();

Thread.Sleep(1000);
Console.WriteLine();

App.CloseApp();