# Ollama Word Plugin - Build and Install
param([switch]$Uninstall)

$ErrorActionPreference = "Stop"
$PluginName = "OllamaWordPlugin"
$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$PluginRoot = Split-Path -Parent $ScriptDir
$SrcDir     = Join-Path $PluginRoot "src"
$BinDir     = Join-Path $PluginRoot "bin"
$InstDir    = "$env:LOCALAPPDATA\$PluginName"
$RegPath    = "HKCU:\Software\Microsoft\Office\Word\Addins\OllamaWordPlugin.Connect"

function Log-Head { param($m) Write-Host ""; Write-Host "  --- $m ---" -ForegroundColor Cyan }
function Log-OK   { param($m) Write-Host "  [OK]  $m" -ForegroundColor Green }
function Log-Warn { param($m) Write-Host "  [!!]  $m" -ForegroundColor Yellow }
function Log-Err  { param($m) Write-Host "  [XX]  $m" -ForegroundColor Red }
function Log-Info { param($m) Write-Host "        $m" }

function Get-NuGetDll {
    param([string]$PackageId, [string]$Version, [string]$DllName, [string]$OutFile)
    if (Test-Path $OutFile) { Log-OK "$DllName already present"; return $true }
    Log-Info "Downloading $PackageId $Version..."
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = Join-Path $env:TEMP "$PackageId-$Version.zip"
        Invoke-WebRequest "https://www.nuget.org/api/v2/package/$PackageId/$Version" -OutFile $zip -UseBasicParsing
        $dir = Join-Path $env:TEMP "$PackageId-$Version"
        if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
        [System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $dir)
        $dll = Get-ChildItem $dir -Recurse -Filter $DllName | Select-Object -First 1
        if ($dll) { Copy-Item $dll.FullName $OutFile -Force; Log-OK "$DllName ready"; return $true }
    } catch { Log-Warn "Could not get $PackageId : $_" }
    return $false
}

function Find-InGac {
    param([string]$FolderName, [string]$DllName)
    foreach ($base in @("C:\Windows\assembly\GAC_MSIL","C:\Windows\Microsoft.NET\assembly\GAC_MSIL")) {
        $folder = Join-Path $base $FolderName
        if (Test-Path $folder) {
            $f = Get-ChildItem $folder -Recurse -Filter $DllName -EA SilentlyContinue | Select-Object -First 1
            if ($f) { return $f.FullName }
        }
    }
    return $null
}

Clear-Host
Write-Host ""
Write-Host "  =================================================" -ForegroundColor Magenta
Write-Host "    Ollama AI Plugin - Build and Install" -ForegroundColor Magenta
Write-Host "  =================================================" -ForegroundColor Magenta

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { Log-Err "Run as Administrator!"; Read-Host; exit 1 }

if ($Uninstall) {
    Get-Process -Name "WINWORD" -EA SilentlyContinue | Stop-Process -Force
    Start-Sleep 2
    if (Test-Path $RegPath) { Remove-Item $RegPath -Recurse -Force; Log-OK "Registry removed" }
    if (Test-Path $InstDir)  { Remove-Item $InstDir -Recurse -Force; Log-OK "Files removed" }
    Log-OK "Done"; Read-Host "Press Enter"; exit 0
}

New-Item -ItemType Directory -Force -Path $BinDir | Out-Null
$dllFile = Join-Path $BinDir "OllamaWordPlugin.dll"
$buildOk = $false

Log-Head "Step 1/4 - Build"

$hasDotnet = $false
try {
    $null = & dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) { $hasDotnet = $true }
} catch {}

if (-not $hasDotnet) {
    Log-Err "dotnet SDK not found. Install .NET 6 SDK"
    Read-Host
    exit 1
}

Log-Info "Using dotnet build"

# Find office.dll
$officeDll = $null
foreach ($p in @(
    "C:\Windows\assembly\GAC_MSIL\office",
    "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\office"
)) {
    if (Test-Path $p) {
        $f = Get-ChildItem $p -Recurse -Filter office.dll | Select-Object -First 1
        if ($f) { $officeDll = $f.FullName; break }
    }
}

# Find Extensibility.dll
$extDll = $null
foreach ($p in @(
    "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\Extensibility.dll",
    "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\Extensibility.dll"
)) {
    if (Test-Path $p) { $extDll = $p; break }
}

if (-not $officeDll) {
    Log-Err "office.dll not found. Microsoft Office must be installed."
    Read-Host
    exit 1
}

if (-not $extDll) {
    Log-Err "Extensibility.dll not found."
    Read-Host
    exit 1
}

Log-OK "office.dll: $officeDll"
Log-OK "Extensibility.dll: $extDll"

# Create csproj
$csprojXml = @"
<Project Sdk="Microsoft.NET.Sdk">

<PropertyGroup>
  <OutputType>Library</OutputType>
  <TargetFramework>net6.0-windows</TargetFramework>
  <AssemblyName>OllamaWordPlugin</AssemblyName>
  <RootNamespace>OllamaWordPlugin</RootNamespace>
  <UseWindowsForms>true</UseWindowsForms>
  <PlatformTarget>AnyCPU</PlatformTarget>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.Office.Interop.Word" Version="15.0.4797.1004" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>

<ItemGroup>
  <Reference Include="office">
    <HintPath>$officeDll</HintPath>
  </Reference>

  <Reference Include="Extensibility">
    <HintPath>$extDll</HintPath>
  </Reference>
</ItemGroup>

<ItemGroup>
  <EmbeddedResource Include="src\RibbonUI.xml"
                    LogicalName="OllamaWordPlugin.RibbonUI.xml"/>
</ItemGroup>

</Project>
"@

$csprojPath = Join-Path $PluginRoot "OllamaWordPlugin.csproj"
Set-Content $csprojPath $csprojXml -Encoding UTF8

Log-Info "Building plugin..."

& dotnet build $csprojPath -c Release -o $BinDir --nologo

if ($LASTEXITCODE -ne 0) {
    Log-Err "Build failed"
    Read-Host
    exit 1
}

if (-not (Test-Path $dllFile)) {
    Log-Err "DLL not produced"
    Read-Host
    exit 1
}

Log-OK "Build successful"
$buildOk = $true

# Fallback: csc.exe
if (-not $buildOk) {
    $cscExe = $null
    foreach ($p in @("$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
                     "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe")) {
        if (Test-Path $p) { $cscExe = $p; break }
    }
    if (-not $cscExe) { Log-Err "csc.exe not found"; Read-Host; exit 1 }
    Log-Info "csc.exe: $cscExe"

    $pkgDir = Join-Path $BinDir "pkg"
    New-Item -ItemType Directory -Force -Path $pkgDir | Out-Null

    # Newtonsoft.Json
    $jsonDll = Join-Path $pkgDir "Newtonsoft.Json.dll"
    Get-NuGetDll "Newtonsoft.Json" "13.0.3" "Newtonsoft.Json.dll" $jsonDll | Out-Null

    # Microsoft.Office.Interop.Word - from GAC first
# Try locate required COM assemblies
$officeDll = Find-InGac "office" "office.dll"
if (-not $officeDll) {
    foreach ($op in @(
        "${env:ProgramFiles}\Microsoft Office\root\vfs\ProgramFilesCommonX86\Microsoft Shared\OFFICE16",
        "${env:ProgramFiles(x86)}\Microsoft Office\root\vfs\ProgramFilesCommonX86\Microsoft Shared\OFFICE16",
        "${env:CommonProgramFiles}\Microsoft Shared\OFFICE16",
        "${env:CommonProgramFiles(x86)}\Microsoft Shared\OFFICE16"
    )) {
        $c = Join-Path $op "office.dll"
        if (Test-Path $c) { $officeDll = $c; break }
    }
}

$extDll = Find-InGac "Extensibility" "Extensibility.dll"
if (-not $extDll) {
    foreach ($p in @(
        "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\Extensibility.dll",
        "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\Extensibility.dll"
    )) {
        if (Test-Path $p) { $extDll = $p; break }
    }
}

if (-not $officeDll) { Log-Warn "office.dll not found, build may fail" }
if (-not $extDll)    { Log-Warn "Extensibility.dll not found, build may fail" }

    # Extensibility.dll (IDTExtensibility2) - from GAC first
    $extDll = Find-InGac "Extensibility" "Extensibility.dll"
    if (-not $extDll) {
        $extDll = Join-Path $pkgDir "Extensibility.dll"
        Get-NuGetDll "Extensibility" "1.0.0" "Extensibility.dll" $extDll | Out-Null
        if (-not (Test-Path $extDll)) { $extDll = $null }
    } else { Log-OK "Extensibility.dll (GAC): $extDll" }

    # Build refs array - use & operator for direct call, no Invoke-Expression
    $refs = [System.Collections.ArrayList]@(
        "System.dll","System.Net.Http.dll","System.Windows.Forms.dll","System.Drawing.dll"
    )
    if (Test-Path $jsonDll)                    { [void]$refs.Add($jsonDll) }
    if ($wordDll -and (Test-Path $wordDll))    { [void]$refs.Add($wordDll) }
    if ($officeDll -and (Test-Path $officeDll)){ [void]$refs.Add($officeDll) }
    if ($extDll -and (Test-Path $extDll))      { [void]$refs.Add($extDll) }

    $refArgs  = @($refs | ForEach-Object { "/r:$_" })
    $srcFiles = @(Get-ChildItem $SrcDir -Filter "*.cs" | ForEach-Object { $_.FullName })
    $outArg   = "/out:$dllFile"

    Log-Info "Compiling $($srcFiles.Count) files..."
    # Direct array call - this is the ONLY correct way in PowerShell
    & $cscExe /target:library $outArg $refArgs $srcFiles

    if ($LASTEXITCODE -eq 0 -and (Test-Path $dllFile)) {
        $buildOk = $true; Log-OK "csc.exe OK"
    } else {
        Log-Err "csc.exe failed (exit $LASTEXITCODE)"
    }
}

if (-not $buildOk) {
    Log-Err "Build failed. Try: https://dotnet.microsoft.com/download/dotnet/6.0"
    Read-Host "Press Enter to exit"; exit 1
}

# Step 3: Copy
Log-Head "Step 3/4 - Copy files"
Get-Process -Name "WINWORD" -EA SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1
New-Item -ItemType Directory -Force -Path $InstDir | Out-Null

Get-ChildItem $BinDir -Filter "*.dll" | ForEach-Object {
    Copy-Item $_.FullName $InstDir -Force; Log-OK "Copied: $($_.Name)"
}
$rx = Join-Path $SrcDir "RibbonUI.xml"
if (Test-Path $rx) { Copy-Item $rx $InstDir -Force; Log-OK "Copied: RibbonUI.xml" }

# Step 4: Registry
Log-Head "Step 4/4 - Register"

# Register as COM Add-in (ProgId based, not VSTO manifest)
if (-not (Test-Path $RegPath)) { New-Item -Path $RegPath -Force | Out-Null }
Set-ItemProperty $RegPath "Description"  "Ollama AI Integration for Word"
Set-ItemProperty $RegPath "FriendlyName" "Ollama AI Plugin"
Set-ItemProperty $RegPath "LoadBehavior" 3 -Type DWord
# Remove old manifest key if leftover from VSTO installs
Remove-ItemProperty $RegPath "Manifest" -ErrorAction SilentlyContinue
Log-OK "Registry: $RegPath"

# Trusted locations
foreach ($v in @("14.0","15.0","16.0")) {
    $tp = "HKCU:\Software\Microsoft\Office\$v\Word\Security\Trusted Locations\OllamaPlugin"
    try {
        if (-not (Test-Path $tp)) { New-Item $tp -Force | Out-Null }
        Set-ItemProperty $tp "Path" $InstDir
        Set-ItemProperty $tp "AllowSubFolders" 1 -Type DWord
    } catch {}
}
Log-OK "Trusted locations set"

# Clear Word disabled-addin lists (fixes checkbox unchecking)
foreach ($v in @("14.0","15.0","16.0")) {
    $dp = "HKCU:\Software\Microsoft\Office\$v\Word\Resiliency\DisabledItems"
    if (Test-Path $dp) {
        (Get-Item $dp).GetValueNames() | ForEach-Object {
            Remove-ItemProperty $dp -Name $_ -EA SilentlyContinue
        }
        Log-OK "Cleared disabled items (Office $v)"
    }
    $sp = "HKCU:\Software\Microsoft\Office\$v\Word\Resiliency\StartupItems"
    if (Test-Path $sp) { Remove-Item $sp -Recurse -Force -EA SilentlyContinue }
}

# Check Ollama
Write-Host ""
try {
    Invoke-WebRequest "http://localhost:11434/api/version" -TimeoutSec 3 -UseBasicParsing -EA Stop | Out-Null
    Log-OK "Ollama is running!"
} catch { Log-Warn "Ollama not running. Start it and run: ollama pull llama3.2" }

Write-Host ""
Write-Host "  =================================================" -ForegroundColor Green
Write-Host "      Done! Open Word -> tab Ollama AI" -ForegroundColor Green
Write-Host "      $InstDir" -ForegroundColor Yellow
Write-Host "  =================================================" -ForegroundColor Green
Write-Host ""
$a = Read-Host "Launch Word now? (Y/n)"
if ($a -ne "n") { Start-Process "winword.exe" -EA SilentlyContinue }
