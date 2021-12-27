$UnityVersions = Get-ChildItem 'HKCU:\SOFTWARE\Unity Technologies\Installer'

foreach ($VersionInfo in $UnityVersions) {
    $RegistryKey = Get-ItemProperty Registry::$VersionInfo
    Write-Host $RegistryKey.$("Location x64")
    Exit
}
