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

4. Configure the M3U URL, EPG URL and Filter in the registry.

```cmd
REG ADD HKLM\Software\IPTVTuner /v M3UURL /t REG_SZ /d "http://provider.com/get.php..."
REG ADD HKLM\Software\IPTVTuner /v EPGURL /t REG_SZ /d "http://provider.com/xmltv.php..."
REG ADD HKLM\Software\IPTVTuner /v Filter /t REG_SZ /d "ENGLISH$"
```

You may also configure additional settings like bind IP address, port, starting channel number, etc. See the Registry Settings section below.

5. Start the service with `net start IPTVTuner`. If everything is configured correctly, the Application event log should report that IPTVTuner has begun updating the lineup and EPG. If you get "access denied," ensure you have completed step 2.

### Update EPG

Create a scheduled task using Windows' Task Scheduler to periodically update the EPG. The scheduled task should invoke `IPTVTuner.exe --update-epg`. This will regenerate the local epg.xml so that Plex can update its guide. I recommend scheduling this to run 15 minutes before the "Scheduled Tasks" window (2am - 5am by default) in Plex.

## Settings

Settings for IPTVTuner are stored in the Windows Registry under HKLM\Software\IPTVTuner. You must create this key if it does not exist. (See step 3 of the Install section.)

| Key | Type | Description | Default Value |
| --- | --- | --- | --- |
| M3UURL | REG_SZ | URL of the M3U from your provider. | **Required** |
| EPGURL | REG_SZ | URL of the EPG from your provider. | **Required** |
| Filter | REG_SZ | Regular expression to include channels. | **Required** |
| IpAddress | REG_SZ | IP address on which to listen. | 127.0.0.1 |
| Port | REG_SZ | Port number on which to listen. | 6079 |
| StartChannel | REG_DWORD | Starting channel number for IPTV lineup. | 1 |

### Additional Registry Settings

#### Missing Channel Logos

IPTVTuner uses the logo URL in your provider's XML for each channel. If no URL is provided, IPTVTuner will generate a basic logo image using the channel name. Some Plex clients (e.g.: Roku) do not do this automatically. This feature is intended to make it easer to locate a channel when seeking in the grid guide.

The following registry entries configure the color and font for generated channel logo images:

| Key | Type | Description | Default Value |
| --- | --- | --- | --- |
| LogoFontFamily | REG_SZ | Font family used in logo. | Segoe UI |
| LogoColor | REG_DWORD | ARGB color value for text in logo. | 0xFFDCDCDC |
| LogoBackground | REG_DWORD | ARGB color value for background in logo. | 0x1 |

Note: The magic value 0x1 in LogoBackground means "select a dynamic background color using the channel name."

Note: IPTVTuner does not check if the logo URLs resolve. If you are missing a logo for a channel, the URL in your provider's EPG data may be broken.

#### Program Guide Gap Fill

Some Plex clients (e.g.: Plex, Android) fail to display channels in the grid view if the channel has no guide data. The web interface displays these channels using an "Unknown Airing" placeholder. As a workaround for those clients, IPTVTuner can insert dummy episodes on half-hour intervals to ensure all channels appear on buggy clients. To enable this feature, set GapFillAmount to a value greater than zero.

The following registry entries configure the gap filling behaviour:

| Key | Type | Description | Default Value |
| --- | --- | --- | --- |
| GapFillAmount | REG_DWORD | Number of hours to fill with dummy episodes starting from midnight today. | 0 |
| GapFillTitle | REG_SZ | Title for dummy episodes that appear in the guide | Unknown Airing |

## Debug/Develop

1. Compile the project and register the executable from the bin/Debug folder as per the install guide.
2. In an admin console, use `net start IPTVTuner` to start the service.
3. In Visual Studio, attach to IPTVTuner.exe using __Debug > Attach to Process...__

When compiled in DEBUG mode, the service should wait in Service.cs#Onstart for a debugger to attach before proceeding.
