param (
    [string]$PathToServiceProjectFolder = '',
    [string]$ServiceInstallFolder = 'C:\Program Files\kvcdr\kvcdrFileWatcher'
)

"publishing '$PathToServiceProjectFolder' to '$ServiceInstallFolder' and installing service 'kvcdrFileWatcher'"

if($PathToServiceProjectFolder -ne "" -and (Test-Path -Path "$PathToServiceProjectFolder"))
{
    $svc = Get-Service -Name kvcdrFileWatcher -ErrorAction Ignore

    if ($svc -ne $null){
        Stop-Service -Name kvcdrFileWatcher -ErrorAction Stop
        Remove-Service -Name kvcdrFileWatcher -ErrorAction Stop
    }

    if(Test-Path -Path "$ServiceInstallFolder"){
        Remove-Item -Recurse -Path "$ServiceInstallFolder" -Force -ErrorAction Stop
    }

    New-Item -Path "$ServiceInstallFolder" -Force -ItemType Directory
    dotnet publish .\kvcdr-watcher.svc.csproj -o "$ServiceInstallFolder"
    New-Service -Name kvcdrFileWatcher -BinaryPathName "$ServiceInstallFolder\kvcdr-watcher.svc.exe" -StartupType Automatic


    Get-Service kvcdrFileWatcher | select Status, Name, UserName, StartType, BinaryPathName
    
}
else
{
    "'$PathToServiceProjectFolder' is an invalid path"
}



