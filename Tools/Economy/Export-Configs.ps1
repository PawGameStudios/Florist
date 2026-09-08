param([string]$OutputDirectory = (Join-Path $env:TEMP 'florist-economy'))
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$yamlAssembly = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Library/PackageCache') -Filter Unity.VisualScripting.YamlDotNet.dll -Recurse | Select-Object -First 1
if (!$yamlAssembly) { throw 'Cached Unity Visual Scripting YAML parser is required. This tool does not launch Unity.' }
[Reflection.Assembly]::LoadFrom($yamlAssembly.FullName) | Out-Null
$deserializer = [Unity.VisualScripting.YamlDotNet.Serialization.DeserializerBuilder]::new().Build()
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
foreach ($config in 'ShopConfig','WorkshopConfig','LevelConfig','CustomerConfig','EconomyConfig','ProfileConfig') {
    $source = Join-Path $projectRoot "Assets/_Florist/_Configs/$config.asset"
    $yaml = [IO.File]::ReadAllText($source) -replace '(?m)^%.*\r?\n','' -replace '(?m)^--- !u!\d+ &-?\d+( stripped)?','---'
    $deserializer.Deserialize[object]($yaml) | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $OutputDirectory "$config.json") -Encoding utf8
}
Write-Output "Exported current config data to $OutputDirectory. Unity was not launched."
