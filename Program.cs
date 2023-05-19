using System;
using AppiumApp;

string appPackage = "com.applisto.appcloner";
string appActivity = "com.applisto.appcloner.activity.MainActivity";
int appsAmount;

Console.ForegroundColor = ConsoleColor.DarkBlue;
Console.WriteLine($"Bumble AppCloner\n");

Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine($"Enter clones amount:");
while (!int.TryParse(Console.ReadLine(), out appsAmount)) { }

Console.WriteLine("\nStarting Appium...");

App.Init(appPackage, appActivity);

// scroll to Bumle app
while (!App.FindElement("//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and contains(@text, 'Bumble')]"))
{
    App.Scroll(200, 800, 200, 50);
}

// select Bumble app
App.Click("//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and contains(@text, 'Bumble')]");

// app settings
// scroll to privacy options
while (!App.FindElement("//android.widget.TextView[@resource-id='android:id/title' and contains(@text, 'Privacy options﻿')]"))
{
    App.Scroll(200, 800, 200, 50);
}

// click privacy options
App.Click("//android.widget.TextView[@resource-id='android:id/title' and contains(@text, 'Privacy options﻿')]");

// scroll to spoof location
while (!App.FindElement("//android.widget.TextView[@resource-id='android:id/title' and contains(@text, 'Spoof location')]"))
{
    App.Scroll(200, 800, 200, 50);
}

// enable spoof location
App.Click("//android.widget.TextView[@resource-id='android:id/title' and contains(@text, 'Spoof location')]");

// open coordinates settings
App.WaitElement("//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and contains(@text, 'Spoof location')]");
App.Click("//android.widget.CheckBox[@text='Enabled']");

//randomize coordinates
//App.Click("//android.widget.Button[@text='Randomize']");

// set coordinates manually
App.ClearField("//android.widget.EditText[@resource-id='com.applisto.appcloner:id/12' and @text='Latitude']");
App.SendText("//android.widget.EditText[@resource-id='com.applisto.appcloner:id/12' and @text='Latitude']", "0"); // 0 hardcoded
App.ClearField("//android.widget.EditText[@resource-id='com.applisto.appcloner:id/12' and @text='Longitude']");
App.SendText("//android.widget.EditText[@resource-id='com.applisto.appcloner:id/12' and @text='Longitude']", "1"); // 1 hardcoded
App.Click("//android.widget.Button[@resource-id='android:id/button1' and @text='OK']");

// close options
App.Click("//android.widget.ImageButton[@content-desc='Back']");

// back to main menu
App.Click("//android.widget.ImageButton[@content-desc='Navigate up']");

// scroll to Bumle app
while (!App.FindElement("//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and contains(@text, 'Bumble')]"))
{
    App.Scroll(200, 800, 200, 50);
}

// select Bumble app
App.Click("//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and contains(@text, 'Bumble')]");

// click Clone number
App.WaitElement("//android.widget.TextView[@resource-id='android:id/title' and @text='Clone number']");
App.Click("//android.widget.TextView[@resource-id='android:id/title' and @text='Clone number']");

// press batch cloning
App.WaitElement("//android.widget.LinearLayout[@content-desc='Batch cloning']");
App.Click("//android.widget.LinearLayout[@content-desc='Batch cloning']");

// enter clones amount
App.ClearField("//android.widget.LinearLayout/android.widget.LinearLayout[2]/android.widget.LinearLayout[@resource-id='com.applisto.appcloner:id/12']/android.widget.FrameLayout/android.widget.EditText[@resource-id='com.applisto.appcloner:id/12']");
App.SendText("//android.widget.LinearLayout/android.widget.LinearLayout[2]/android.widget.LinearLayout[@resource-id='com.applisto.appcloner:id/12']/android.widget.FrameLayout/android.widget.EditText[@resource-id='com.applisto.appcloner:id/12']", appsAmount.ToString());
App.Click("//android.widget.Button[@resource-id='android:id/button1' and @text='OK']");

// press clone
App.Click("//android.widget.ImageButton[@resource-id='com.applisto.appcloner:id/12' and @content-desc='Clone app']");
App.Click("//android.widget.Button[@resource-id='android:id/button1' and @text='OK']");

// cloning app process...

// waiting for "app cloned" text
App.WaitElement("//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and @text='App cloned']", 120 * appsAmount);

// press install apps
App.Click("//android.widget.Button[@resource-id='android:id/button1' and @text='INSTALL APPS']");

// waiting next app intall confirmation or end of all apps installation
for (int i = 0; i < appsAmount; i++)
{
    // install app confirmation
    App.WaitElement("//android.widget.Button[@resource-id='com.android.packageinstaller:id/ok_button']", 100);
    App.Click("//android.widget.Button[@resource-id='com.android.packageinstaller:id/ok_button']");
}

// waiting end of installation
App.WaitElement("//android.widget.ImageView[@resource-id='com.applisto.appcloner:id/icon']", 60);
    
// back to main menu
App.Click("//android.widget.ImageButton[@content-desc='Navigate up']");

Clones:
// select clones tab
App.Click("//android.widget.ImageButton[@content-desc='Open drawer']");
App.Click("//androidx.appcompat.widget.LinearLayoutCompat[2]/android.widget.CheckedTextView[@resource-id='com.applisto.appcloner:id/12']");

// change permissions to all clones
for (int i = 1; i <= appsAmount; i++)
{
    // find current app
    // scroll to find
    while (!App.FindElement($"//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and @text='Bumble • {i}']"))
    {
        App.Scroll(200, 800, 200, 50);
    }

    App.Click($"//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and @text='Bumble • {i}']");

    // edit app settings
    App.Click("//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and @text='Edit clone settings']");
    App.WaitElement("//android.widget.ImageView[@content-desc='App icon']");

    // scroll to privacy options
    while (!App.FindElement("//android.widget.TextView[@resource-id='android:id/title' and contains(@text, 'Privacy options﻿')]"))
    {
        App.Scroll(200, 800, 200, 50);
    }

    // click privacy options
    App.Click("//android.widget.TextView[@resource-id='android:id/title' and contains(@text, 'Privacy options﻿')]");

    // enable spoof location
    App.Click("//android.widget.TextView[@resource-id='android:id/title' and contains(@text, 'Spoof location')]");

    // open coordinates settings
    App.WaitElement("//android.widget.TextView[@resource-id='com.applisto.appcloner:id/12' and contains(@text, 'Spoof location')]");

    // set coordinates manually
    App.ClearField("//android.widget.EditText[@resource-id='com.applisto.appcloner:id/12' and @text='0']");
    App.SendText("//android.widget.EditText[@resource-id='com.applisto.appcloner:id/12' and @text='Latitude']", new Random().Next(0, 80).ToString());
    App.ClearField("//android.widget.EditText[@resource-id='com.applisto.appcloner:id/12' and @text='1']");
    App.SendText("//android.widget.EditText[@resource-id='com.applisto.appcloner:id/12' and @text='Longitude']", new Random().Next(0, 160).ToString());
    App.Click("//android.widget.Button[@resource-id='android:id/button1' and @text='OK']");

    // confirm settings
    App.Click("//android.widget.Button[@resource-id='com.applisto.appcloner:id/12' and @content-desc='Send settings to clone']");
    App.WaitElement("//android.widget.Button[@resource-id='android:id/button1' and @text='OK']");
    App.Click("//android.widget.Button[@resource-id='android:id/button1' and @text='OK']");

    // discard launch app
    App.WaitElement("//android.widget.Button[@resource-id='android:id/button2' and @text='CANCEL']");
    App.Click("//android.widget.Button[@resource-id='android:id/button2' and @text='CANCEL']");

    // close privacy options
    App.Click("//android.widget.ImageButton[@content-desc='Back']");

    // back to main menu
    App.Click("//android.widget.ImageButton[@content-desc='Navigate up']");
}

App.CloseApp();