# IPTVTuner

Emulates a tuner to allow IPTV streams to play in Plex.

## Install

1. Copy IPTVTuner.exe and related DLLs to the server.
2. Ensure that "NT AUTHOITY\NetworkService" has read/write access to IPTVTuner.exe and related DLLs.

Example of changing ACLs in PowerShell.
```
$acl = Get-Acl .\IPTVTuner.exe
$AccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("NT AUTHORITY\NetworkService","FullControl","Allow")
$acl.SetAccessRule($AccessRule)
$acl | Set-Acl .\IPTVTuner.exe
```

3. Configure the M3U/EPG URL and filters in the registry.

The following are required:
```
REG ADD HKLM\Software\IPTVTuner \v M3UURL \t REG_SZ \d <M3U URL>
REG ADD HKLM\Software\IPTVTuner \v EPGURL \t REG_SZ \d <EPG URL>
```

The following are optional:
```
REG ADD HKLM\Software\IPTVTuner \v IpAddress \t REG_SZ \d <Start channel>
REG ADD HKLM\Software\IPTVTuner \v Port \t REG_DWORD \d <Start channel>
REG ADD HKLM\Software\IPTVTuner \v StartChannel \t REG_DWORD \d <Start channel>
REG ADD HKLM\Software\IPTVTuner \v Filter \t REG_SZ \d <Filter>
```

4. Start the service with `net start IPTVTuner`. Check the application event log to ensure the IPTVTuner has started.

## Update

Create a scheduled task to periodically update the EPG.

The scheduled task should invoke `IPTVTuner.exe --update-epg`.
