$EngineLocation = $args[0]
$LibCache = Get-ChildItem "$EngineLocation\\Editor\\Data\Resources\\PackageManager\\ProjectTemplates\\libcache" -filter "*.3d-*" -Directory
$LibCachePath = $LibCache.FullName

Write-Host "$LibCachePath\ScriptAssemblies"
