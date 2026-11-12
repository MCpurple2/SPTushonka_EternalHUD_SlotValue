param(
    [Parameter(Mandatory=$true)][string]$GamePath,
    [string]$OutputPath = "$PSScriptRoot/EternalHUD.SlotValue.dll"
)
$ErrorActionPreference = 'Stop'
$managed = Join-Path $GamePath 'EscapeFromTarkov_Data/Managed'
$core = Join-Path $GamePath 'BepInEx/core'
$refs = @(
    "$managed/mscorlib.dll", "$managed/System.dll", "$managed/System.Core.dll",
    "$managed/Sirenix.Serialization.dll", "$managed/netstandard.dll", "$managed/Assembly-CSharp.dll",
    "$managed/UnityEngine.dll", "$managed/UnityEngine.CoreModule.dll",
    "$core/BepInEx.dll", "$core/0Harmony.dll", "$GamePath/BepInEx/plugins/EternalHUD.dll"
)
$sdk = (dotnet --list-sdks | Select-Object -Last 1)
if ($sdk -notmatch '^([^ ]+) \[(.+)\]$') { throw 'Install a .NET SDK first.' }
$csc = Join-Path $Matches[2] "$($Matches[1])/Roslyn/bincore/csc.dll"
dotnet $csc /nologo /noconfig /nostdlib+ /target:library /optimize+ /deterministic+ "/out:$OutputPath" @($refs | ForEach-Object { "/reference:$_" }) "$PSScriptRoot/Plugin.cs" "$PSScriptRoot/PriceDisplay.cs" "$PSScriptRoot/InventoryValue.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }

