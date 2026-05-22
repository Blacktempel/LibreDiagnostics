<p align="center">
    <img src="LibreDiagnostics.UI/Assets/Icon.ico" width="128" height="128"/>
</p>
<h1 align="center">LibreDiagnostics</h1>

[![GitHub license](https://img.shields.io/github/license/blacktempel/LibreDiagnostics?label=License)](https://github.com/blacktempel/librediagnostics/blob/master/LICENSE)
[![Build master](https://github.com/Blacktempel/LibreDiagnostics/actions/workflows/master.yml/badge.svg)](https://github.com/Blacktempel/LibreDiagnostics/actions/workflows/master.yml)
![GitHub all releases](https://img.shields.io/github/downloads/Blacktempel/LibreDiagnostics/total?label=Downloads)
[![GitHub release](https://img.shields.io/github/v/release/Blacktempel/LibreDiagnostics?label=Version)](https://github.com/Blacktempel/LibreDiagnostics/releases/latest)

A desktop utility for real-time hardware monitoring and diagnostics.

## Preview of app

Please click on an image to show its full size.<br/>
Note that the screenshots may not always represent the latest version of the app.

<a href="Screenshots/Preview01.jpg">
    <img src="Screenshots/Preview01.jpg" width="125"/>
</a>
<a href="Screenshots/Preview02.jpg">
    <img src="Screenshots/Preview02.jpg" width="125"/>
</a>

## Features

### Monitoring for your hardware components
- CPU
- GPU
- RAM
- Drives
- Network
- Fans

### Various customization options, such as
- display the app as an app bar (Windows)
- autostart after you initially logged in (Windows)
- docking location[s] on different screens
- adjust application font size & width
- tailor update interval to your needs
- various color options for you to modify
- enable or disable specific monitors, hardware or sensors
- and more !

### S.M.A.R.T. data of your drives
You can show various [S.M.A.R.T.](https://en.wikipedia.org/wiki/Self-Monitoring,_Analysis_and_Reporting_Technology) attributes of your drives,
such as health, temperature, power-on hours and more, by simply clicking on the drives name in the drives monitor.

### Missing something ?
Feel free to create a feature request if you think something would improve your handling of the app.<br/>
Your proposal will be considered and analysed to verify if an implementation is possible.<br/>
Please note that an exact timeframe for a feature cannot be provided.

## Requirements

#### All platforms
- .NET 8 Desktop Runtime<br/>

#### Windows 10+
- [PawnIO](https://pawnio.eu/) (Kernel Driver) - source available on [github](https://github.com/namazso/PawnIO)

## Where can I download it ?
You can download the latest release [from here.](https://github.com/Blacktempel/LibreDiagnostics/releases)

## Updating the app

#### Auto update
If enabled, the application will check for an update upon starting, download and apply it.<br/>
Using the update feature WILL erase all files inside of the directory where this application resides.<br/>
Please be aware of that and don't save anything else beside the application itself in its directory.<br/>
Your config file is saved elsewhere and will not be affected.<br/>
The update will be downloaded directly from the latest release on Github.<br/>
If Github has an issue or is down, the update cannot be downloaded and applied.

#### Manual update
You can also update manually by selecting the update button when opening the menu (right click) on the tray icon in your taskbar.<br/>
The update process will be the same as explained in auto update.<br/>
Downloading the latest release from Github and manually replacing it is also perfectly fine.

## What platforms are supported ?
All of our code is cross-platform and also builds on Linux.<br/>
So far only Windows has been tested, so Linux support is experimental.<br/>
Not all settings and features may be available on other operating systems.

## Supported languages
| Language | Native name |
|---|---|
| Czech | Čeština |
| Danish | Dansk |
| English | English |
| Finnish | Suomi |
| French | Français |
| German | Deutsch |
| Italian | Italiano |
| Japanese | 日本語 |
| Korean | 한국어 |
| Norwegian | Norsk bokmål |
| Polish | Polski |
| Romanian | Română |
| Russian | Русский |
| Simplified Chinese (China) | 简体中文（中国） |
| Spanish | Español |
| Swedish | Svenska |
| Traditional Chinese (Taiwan) | 繁體中文（台灣） |

Feel free to contribute translations for other languages !

## Project overview
| Project | .NET Version[s] |
| --- | --- |
| **[LibreDiagnostics](https://github.com/Blacktempel/LibreDiagnostics/tree/master/LibreDiagnostics)** <br/> Main entry assembly that starts the application. | .NET 8 |
| **[LibreDiagnostics.Language](https://github.com/Blacktempel/LibreDiagnostics/tree/master/LibreDiagnostics.Language)** <br/> Localization resources and translations.<br/>Provides resources used in UI text, dialogs and messages. | .NET 8 |
| **[LibreDiagnostics.Models](https://github.com/Blacktempel/LibreDiagnostics/tree/master/LibreDiagnostics.Models)** <br/> Data models and global helpers used across the app (e.g. configuration).<br/>Central place for strongly-typed settings and shared model structures. | .NET 8 |
| **[LibreDiagnostics.MVVM](https://github.com/Blacktempel/LibreDiagnostics/tree/master/LibreDiagnostics.MVVM)** <br/> ViewModels and MVVM utilities.<br/>Binds models to the UI, implements app logic / state, and supports data flow between monitors and views. | .NET 8 |
| **[LibreDiagnostics.Tasks](https://github.com/Blacktempel/LibreDiagnostics/tree/master/LibreDiagnostics.Tasks)** <br/> Background tasks and services (e.g. GitHub updater logic). | .NET 8 |
| **[LibreDiagnostics.UI](https://github.com/Blacktempel/LibreDiagnostics/tree/master/LibreDiagnostics.UI)** <br/> UI entry and application wiring.<br/>Hosts [Avalonia UI](https://avaloniaui.net/) startup, icon setup, and user-facing update prompts.<br/>Contains client logic for launching the desktop app. | .NET 8 |
| **[LibreDiagnostics.Updater](https://github.com/Blacktempel/LibreDiagnostics/tree/master/LibreDiagnostics.Updater)** <br/> Standalone updater executable. | .NET 8 |

## Reporting issues
LibreDiagnostics is using existing hardware monitoring libraries and drivers.<br/>
Any issues regarding hardware access (like invalid sensor values) should be reported to the respective project.<br/>
If you find bugs or issues in LibreDiagnostics itself, please open an issue on the repository.

### Used libraries and projects
- https://github.com/LibreHardwareMonitor/LibreHardwareMonitor

#### Verifying sensor issues with LibreHardwareMonitor
You can verify if a sensor issue is related to LibreDiagnostics or LibreHardwareMonitor by using the user interface of LibreHardwareMonitor.<br/>
A good verification step is to check if the same sensor shows the same issue in both applications.<br/>
Please remember that the latest release of LibreHardwareMonitor may be different than the version used in LibreDiagnostics.<br/>
To have a better comparison, you can download Artifacts of a recent master build from ["Actions" on the LibreHardwareMonitor repository](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/actions) and test with that.

## FAQ
**Q:** Where can I find my config file ?<br/>
**A:** The config file is located in your local application data folder.<br/>
For Windows this would be at: ``%LocalAppData%/LibreDiagnostics/Settings.json``.<br/>

## How can I help improve the project ?
Feel free to give feedback and contribute to our project !<br/>
Pull requests are welcome. Please include as much information as possible.

## Administrator rights

#### Windows
Some functionality requires administrator privileges to access the data.<br/>
This includes, but is not limited to, Kernel drivers and therefore specific calls to `DeviceIoControl`.<br/>
If forcing the software to start without appropiate rights, functionality may be limited or not work at all.

## License
LibreDiagnostics is free and open source software licensed under MPL 2.0.<br/>
You can use it in private and commercial projects.<br/>
Keep in mind that you must include a copy of the license in your project.
