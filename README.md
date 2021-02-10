# CapsLockShortcuts
Application to disable the CapsLock toggle and instead use the key for shortcuts. At the moment, shortcuts only map to a single key press. However, you can either map to a key which isn't present on your keyboard  (e.g. F13-F24) or map an existing key to a more ergonomic location (default config). Works with any keyboard layout.

Feel free to make suggestions or report bugs.


## Prerequisites

- Windows 10
- [.NET 5.0](https://dotnet.microsoft.com/download)


## Installation

Download the latest release file from [here](https://github.com/Thomi7/CapsLockShortcuts/releases) and extract the contents to a folder of your choice (we'd suggest "C:\ProgramData\CapsLockShortcuts"). You can run the exe and use the application now.

## Notes

By default, keyboard inputs can't be captured, when an application running with elevated rights is in foreground or uses raw keyboard data. To bypass these limitations, you can run CapsLockShortcuts as administrator.


## Autostart

We present two methods here, one for starting CapsLockShortcuts in standard user mode and one for starting with elevated rights.

### Autostart (User)

Just put a shortcut to CapsLockShortcuts.exe into the Autostart-folder. The Autostart-folder can be found by pressing Win + R, entering "shell:startup" in the textbox and finally hitting return.

### Autostart (Administrator)

Type 'cmd' into the Start Menu and open the Command Prompt through 'Run as Administrator'. 

Replace '<path to CapsLockShortcuts.exe>' in the following command and run it:

```
SCHTASKS /CREATE /TR "<path to CapsLockShortcuts.exe>" /SC ONLOGON /TN "CapsLockShortcuts" /RL HIGHEST /IT
```

You can open 'Task Scheduler' through the search function of the start menu in order to further customize the task. Especially take a look at the tab 'Conditions' if you have a laptop, since by default the task is configured to not run and also stop when on battery.

## Tray Icon

The application places an icon in your taskbar, which on right click allows you to configure or temporarily disable its functionality.

## Default Configuration

<img src="README Resources/default config shortcuts.png" width=100% title="default config shortcuts"/>

## Configuration

You can find the config file by right clicking the tray icon and then hitting 'Config'.

To configure the shortcuts, you use every key's code, which you can find out by hitting 'Keycodes' in the tray icon menu. Alternatively, if you wish to use keys that aren't present on your keyboard, have a look [here](https://docs.microsoft.com/de-de/dotnet/api/system.windows.forms.keys). Under 'BaseKeys', you can opt to use another key combination instead of CapsLock to initiate the hotkeys you will define next. Under 'Shortcuts' you define a key to key mapping, meaning you choose a single 'InputKey', which triggers the shortcut and then a single 'OutputKey', the key which should be pressed.

When opting for different 'BaseKeys' than CapsLock, make sure that Windows doesn't already use the shortcut you are defining since that shortcut won't work then.
