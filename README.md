# IPTVTuner

Emulates a network tuner to allow IPTV streams to play in Plex.

This is a very lightweight Windows service that is similar in function to [telly 1.1](https://github.com/tellytv/telly). It has been tested on Windows 10 and Windows Server Core 2016. 

## Install

1. Copy IPTVTuner.exe and related DLLs to the server.
2. Ensure that "NT AUTHORITY\LocalService" has read and execute access to IPTVTuner.exe, related DLLs, and the directory.

Example of changing ACLs in PowerShell.
```powershell
$acl = Get-Acl .\IPTVTuner.exe
$AccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("NT AUTHORITY\LocalService","ReadAndExecute","Allow")
$acl.SetAccessRule($AccessRule)
$acl | Set-Acl .\IPTVTuner.exe
$acl | Set-Acl .\Newtonsoft.Json.dll
$acl | Set-Acl .\uhttpsharp.dll
```

3. Using an admin console, register the IPTVTuner service.
```cmd
IPTVTuner.exe --install
```
You can remove the service with the `--uninstall` argument.

3. Configure the M3U/EPG URL and filters in the registry.

The following are required:
```cmd
REG ADD HKLM\Software\IPTVTuner /v M3UURL /t REG_SZ /d "http://provider.com/get.php..."
REG ADD HKLM\Software\IPTVTuner /v EPGURL /t REG_SZ /d "http://provider.com/xmltv.php..."
REG ADD HKLM\Software\IPTVTuner /v Filter /t REG_SZ /d "ENGLISH$"
```

The following are optional entries; default values are shown:
```cmd
REG ADD HKLM\Software\IPTVTuner /v IpAddress /t REG_SZ /d "127.0.0.1"
REG ADD HKLM\Software\IPTVTuner /v Port /t REG_DWORD /d 6079
REG ADD HKLM\Software\IPTVTuner /v StartChannel /t REG_DWORD /d 1
```

4. Start the service with `net start IPTVTuner`. If everything is configured correctly, the Application event log should report that IPTVTuner has begun updating the lineup and EPG.

## Update EPG

Create a scheduled task to periodically update the EPG. The scheduled task should invoke `IPTVTuner.exe --update-epg`. This will regenerate the local epg.xml so that Plex can update its guide.


## Debug/Develop

1. Compile the project and register the executable from the bin/Debug folder as per the install guide.
2. In an admin console, use `net start IPTVTuner` to start the service.
3. In Visual Studio, attach to IPTVTuner.exe using __Debug > Attach to Process...__

When compiled in DEBUG mode, the service should wait in Service.cs#Onstart for a debugger to attach before proceeding.
