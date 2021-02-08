# Loginator
The best way to display your logs

Loginator is a high performance log viewer that shows you logs in realtime:

* Chainsaw (nlog, log4net, log4j, etc.)
* Logcat (Android)

## Download

You can download the current release ![here](https://github.com/dabeku/Loginator/releases) or by clicking **Releases** above.

## Preview

Here is how it looks like:

![ScreenShot](https://cloud.githubusercontent.com/assets/14138808/9817745/0aaca424-58a5-11e5-82ca-e791690958d8.png)

![ScreenShot](https://cloud.githubusercontent.com/assets/14138808/25998297/a3489e76-3720-11e7-8c9f-60b67423d2c5.png)

## Some cool features include

* Limit the number of logs held in memory
* Show details of a log like exception, context, etc.
* Show/Hide logs based on application
* Show/Hide logs based on namespace
* Filter output based on log level and expressions
* Configure log type and port

## Issues
If the **timestamps are incorrect** (i.e. off by x hours): Remove existing configurations from C:\Users\your-username\AppData\Local\Loginator by simply deleting all folders and restarting the application.

## TODO (from most to least important)

* Add option to group by application (name only)
* Export function
* Documentation for installation
* Other data sources (database, file, etc.) via polling

## Chainsaw logging

* NLog (.net):

In your logging app add a new target

```xml
<target xsi:type="Chainsaw" name="chainsaw" address="udp://127.0.0.1:7071" />
```

or, if you want do include the context

```xml
<target xsi:type="Chainsaw" name="chainsaw" address="udp://127.0.0.1:7071" includeMdc="true" />
```

and add the logger

```xml
<logger name="*" minlevel="Trace" writeTo="chainsaw" />
```

### Nested diagnostic logical context support

To use nested diagnostic context in your application add the following attribute to your target

```xml
<target xsi:type="Chainsaw"
        includeNdlc="true"
        ...
    />
```

If Ndlc information is available, it is used as prefix to the message.

e.g.

```cs
NestedDiagnosticsLogicalContext.Push("Client 123");
this.logger.Debug("connected");
```

or in combination with [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging/) and [NLog.Extensions.Logging](https://www.nuget.org/packages/NLog.Extensions.Logging/)

```cs
using (this.logger.BeginScope("Client 123"))
{
    this.logger.LogDebug("connected");
}
```

is displayed as `Client 123 connected` in the message column.

## Logcat logging

1. Connect your Android device to your PC or Mac via USB

2. Find your device ID: /[path-to]/Android/sdk/platform-tools/adb devices

3. Forward the logcat output to your machine with Loginator running

* Windows:

[path-to]\Android\sdk\platform-tools\adb.exe -s [your-device-id] logcat | ncat.exe -u [ip-where-loginator-runs] 7081

Please note: As ncat.exe is not part of Windows, you have to download nmap first and use ncat.exe from there. To do that simply visit https://nmap.org/download.html locate and download the "Latest command-line zipfile". Unpack the .zip and take the files: libeay32.dll, ncat.exe and ssleay32.dll and copy them in a separate folder. You now have a valid ncat.exe application.

* Mac:

cat <(/[path-to]/Android/sdk/platform-tools/adb -s [your-device-id] logcat) | nc -u [ip-where-loginator-runs] 7081

## Tested with

* Chainsaw: NLog (.net), same machine (Windows 10)
* Logcat: Android logcat (default output format), remote machine (Android: Windows/Mac OSx, Loginator: Windows 10)
