INSTALL SERVICE

**Windows**

1. Build the project    
    1. cd .\kvcdr-watcher\ 
    1. dotnet clean
    1. dotnet build
1. Deploy the service
    1. rm -Recurse -Path 'C:\Program Files\kvcdr\kvcdrFileWatcher\' -Force
    1. mkdir -Path 'C:\Program Files\kvcdr\kvcdrFileWatcher\' -Force
    1. cd .\kvcdr-watcher-svc\
    1. dotnet publish .\kvcdr-watcher.svc.csproj -o 'C:\Program Files\kvcdr\kvcdrFileWatcher'
    1. New-Service -Name kvcdrFileWatcher -BinaryPathName "C:\Program Files\kvcdr\kvcdrFileWatcher\kvcdr-watcher.svc.exe" -StartupType Automatic
1. Publish the service
    1. Start-Service  -Name kvcdrFileWatcher
1. Uninstall the service    
    1. Stop-Service  -Name kvcdrFileWatcher
    1. Remove-Service -Name kvcdrFileWatcher
    1. rm -Recurse -Path 'C:\Program Files\kvcdr\kvcdrFileWatcher\' -Force


Dependencies

1. This implementation expects [Thunderbird](https://www.thunderbird.net/en-US/) and was tested with version 128.2.3esr (64-bit)
1. This was tested on Win11 Pro 23H2 Build 22631.4169
