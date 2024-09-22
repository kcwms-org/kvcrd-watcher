INSTALL SERVICE

    1. New-Service -Name kvcdrFileWatcher -BinaryPathName "C:\Program Files\kvcdr\kvcdrFileWatcher\kvcdr-watcher.svc.exe" -StartupType Automatic
    1. Start-Service  -Name kvcdrFileWatcher

UNINSTALL SERVICE 
    
    1. Stop-Service  -Name kvcdrFileWatcher
    1. Remove-Service -Name kvcdrFileWatcher