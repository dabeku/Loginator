# Loginator
A log viewer for all kinds of data sources (log4j, log4net, nlog, logcat, etc.)

This is basically a simple log viewer that shows you logs in realtime

## Loginator can do the following

* Activate/deactivate logging
* Scroll to bottom
* Limit the number of logs held in memory
* Clear the log
* Show details of a log like exception, namespace, etc.
* Show/Hide logs based on namespace

## TODO

* Make protocol type configurable (chainsaw, logcat, etc.)
* Make listening port configurable (currently: 7071)

## Chainsaw logging

NLog (.net):

In your logging app add a new target:
```
<target xsi:type="Chainsaw" name="chainsaw" address="udp://127.0.0.1:7071" />
```
and add the logger
```
<logger name="*" minlevel="Trace" writeTo="chainsaw" />
```

## Logcat logging

1. Connect your USB device to your PC or Mac and forward the logcat output to your machine with Loginator running.
2. Find you device ID: /...path-to.../Android/sdk/platform-tools/adb devices

Mac:
cat <(/[path-to]/Android/sdk/platform-tools/adb -s [your-device-id] logcat) | nc -u 192.168.1.5 7071

## Tested with

* Chainsaw: NLog (.net), same machine
* Logcat: Android logcat (default output format), remote machine
