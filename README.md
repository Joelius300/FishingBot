## Introduction
I made this for myself to be able to afk-fish in minecraft because I was too lazy to find something to hold down a key which worked 100% reliably. Doing it with software turned out to be a lot more reliable in my case.  

This bot simply starts holding down the right mouse button and won't release it until you tell it to.  

It will check in an interval if the button was released and press it again if necessary. This is especially useful if you have to quickly do something on your computer but then want to go back to fishing. With this feature you don't have to restart the bot (that part is easy but you might just forget).
This interval defaults to 2 minutes, for customization see [Customization](#Customization).  

When starting the bot, it will give you a countdown intended to give you time to switch back in the minecraft tab and position your mouse. This value defaults to 10 seconds, for customization see [Refreshrate](#Refreshrate).  

This application only works on windows because it uses a library which wraps windows-input functionality to more easily program keyboard and mouse simulations.  
The library is fairly old but still works like a charm. The original is [here](https://archive.codeplex.com/?p=inputsimulator) and the version I have in my project is just a port to .net core because the original uses .net framework. The nuget for that is [here](https://www.nuget.org/packages/InputSimulatorCore/1.0.5)

## Usage

After compiling you get a bunch of files but you only need the following to use it:
 - FishingBot.dll
 - FishingBot.runtimeconfig.json
 - WindowsInput.dll

You can then run `dotnet FishingBot.dll` in the folder with the three files and the countdown should start.  

### Customization

### Countdown

You also have the possibility to define the countdown at the start.
The default value is 10 seconds.  
You can customize it by using the following parameter options:

- `/countdown`
- `--countdown`
- `-c`

Syntax: `dotnet FishingBot.dll -c 00:00:30`
Syntax: `dotnet FishingBot.dll /countdown 00:01:05`

### Refreshrate

The same goes for the refresh interval. After starting to hold down the right mouse button, it will continuously check with this interval if it's necessary to start holding down the button again and do so respectively.   
The default value is 2 minutes.  
You can customize it by using the following parameter options:

- `/refreshrate`
- `--refreshrate`
- `-r`

Syntax: `dotnet FishingBot.dll --refreshrate 00:00:50`
Syntax: `dotnet FishingBot.dll -r 00:30:00`

### Combined
  
These two options can of course be combined. For example: `dotnet FishingBot.dll --refreshrate 00:00:50 -c 00:00:04`