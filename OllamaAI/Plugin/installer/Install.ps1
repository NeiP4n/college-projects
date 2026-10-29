# Ollama AI Plugin - Installer
param([switch]$Uninstall)

$ErrorActionPreference = "Continue"
$PluginRoot = Split-Path -Parent $PSScriptRoot
$ServerDir  = Join-Path $PluginRoot "server"
$AddinDir   = Join-Path $PluginRoot "addin"
$CatalogId  = "{AAAA0000-0000-0000-0000-AAAAAAAAAAAA}"

function Log-Head { param($m) Write-Host ""; Write-Host "  --- $m ---" -ForegroundColor Cyan }
function Log-OK   { param($m) Write-Host "  [OK]  $m" -ForegroundColor Green }
function Log-Warn { param($m) Write-Host "  [!!]  $m" -ForegroundColor Yellow }
function Log-Err  { param($m) Write-Host "  [XX]  $m" -ForegroundColor Red }
function Log-Info { param($m) Write-Host "        $m" }

Clear-Host
Write-Host ""
Write-Host "  ================================================" -ForegroundColor Magenta
Write-Host "    Ollama AI Plugin for Word - Installer v2      " -ForegroundColor Magenta
Write-Host "  ================================================" -ForegroundColor Magenta
Write-Host ""

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Log-Err "Run as Administrator required!"
    Read-Host "Press Enter to exit"
    exit 1
}

if ($Uninstall) {
    Log-Head "Uninstalling"
    $link = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\OllamaAIServer.lnk"
    if (Test-Path $link) { Remove-Item $link -Force; Log-OK "Startup shortcut removed" }
    $desk = [Environment]::GetFolderPath("Desktop") + "\Ollama AI Server.lnk"
    if (Test-Path $desk) { Remove-Item $desk -Force; Log-OK "Desktop shortcut removed" }
    foreach ($v in @("16.0","15.0")) {
        $r = "HKCU:\Software\Microsoft\Office\$v\WEF\TrustedCatalogs\$CatalogId"
        if (Test-Path $r) { Remove-Item $r -Recurse -Force; Log-OK "Registry $v removed" }
    }
    Log-OK "Uninstalled. Close any running server window manually."
    Read-Host "Press Enter to exit"
    exit 0
}

# Step 1 - Node.js
Log-Head "Step 1/4 - Node.js"
$nodeCmd = Get-Command node -ErrorAction SilentlyContinue
if ($nodeCmd) {
    $ver = & node --version 2>&1
    Log-OK "Node.js found: $ver"
} else {
    Log-Warn "Node.js not found - downloading v20 LTS..."
    $msi = "$env:TEMP\node-installer.msi"
    try {
        Invoke-WebRequest -Uri "https://nodejs.org/dist/v20.11.0/node-v20.11.0-x64.msi" -OutFile $msi -UseBasicParsing
        Start-Process msiexec -ArgumentList "/i `"$msi`" /qn /norestart" -Wait
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
        Log-OK "Node.js installed"
    } catch {
        Log-Err "Node.js install failed. Get it from https://nodejs.org then rerun INSTALL.bat"
        Read-Host "Press Enter to exit"
        exit 1
    }
}

# Step 2 - SSL Certificate
Log-Head "Step 2/4 - SSL Certificate"
$certFile = Join-Path $ServerDir "cert.pem"
$keyFile  = Join-Path $ServerDir "key.pem"

if ((Test-Path $certFile) -and (Test-Path $keyFile)) {
    Log-OK "Certificate files already exist - skipping"
} else {
    $openssl = Get-Command openssl -ErrorAction SilentlyContinue
    if (-not $openssl) {
        $git = "C:\Program Files\Git\usr\bin\openssl.exe"
        if (Test-Path $git) { $openssl = @{ Source = $git } }
    }
    if ($openssl) {
        $exe = if ($openssl -is [hashtable]) { $openssl.Source } else { $openssl.Source }
        $cnf = "$env:TEMP\ssl.cnf"
        @"
[req]
distinguished_name=dn
x509_extensions=v3
prompt=no
[dn]
CN=localhost
[v3]
subjectAltName=DNS:localhost,IP:127.0.0.1
"@ | Set-Content $cnf -Encoding ASCII
        & $exe req -x509 -newkey rsa:2048 -keyout $keyFile -out $certFile -days 3650 -nodes -config $cnf 2>&1 | Out-Null
        Remove-Item $cnf -ErrorAction SilentlyContinue
        Log-OK "Certificate generated with openssl"
    } else {
        Log-Info "Generating certificate with PowerShell..."
        try {
            $c = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My" `
                -NotAfter (Get-Date).AddYears(10) -FriendlyName "OllamaAI" `
                -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256
            $pem = "-----BEGIN CERTIFICATE-----`r`n" +
                [Convert]::ToBase64String($c.RawData, "InsertLineBreaks") +
                "`r`n-----END CERTIFICATE-----"
            Set-Content $certFile $pem -Encoding ASCII
            $pfxBytes = $c.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, "ollama123")
            [IO.File]::WriteAllBytes((Join-Path $ServerDir "cert.pfx"), $pfxBytes)
            Set-Content $keyFile "PFX_MODE" -Encoding ASCII
            $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root","LocalMachine")
            $store.Open("ReadWrite"); $store.Add($c); $store.Close()
            Log-OK "Certificate created and trusted (PowerShell)"
        } catch {
            Log-Warn "Certificate creation failed: $_"
            "SKIP" | Set-Content $certFile -Encoding ASCII
            "SKIP" | Set-Content $keyFile  -Encoding ASCII
        }
    }
}

# Trust cert
try {
    $pem = Get-Content $certFile -Raw -ErrorAction SilentlyContinue
    if ($pem -like "*BEGIN CERTIFICATE*") {
        $x = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
        $x.Import([System.Text.Encoding]::ASCII.GetBytes($pem))
        $s = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root","LocalMachine")
        $s.Open("ReadWrite"); $s.Add($x); $s.Close()
        Log-OK "Certificate trusted in Windows Root store"
    }
} catch { Log-Warn "Could not auto-trust cert (may show browser warning): $_" }

# Step 3 - Register manifest
Log-Head "Step 3/4 - Register in Word"
$catDir = "$env:LOCALAPPDATA\OllamaWordPlugin\catalog"
New-Item -ItemType Directory -Force -Path $catDir | Out-Null
Copy-Item (Join-Path $AddinDir "manifest.xml") -Destination $catDir -Force
Log-OK "Manifest copied to: $catDir"

foreach ($v in @("16.0","15.0")) {
    $rp = "HKCU:\Software\Microsoft\Office\$v\WEF\TrustedCatalogs\$CatalogId"
    try {
        if (-not (Test-Path $rp)) { New-Item -Path $rp -Force | Out-Null }
        Set-ItemProperty $rp "Id"    $CatalogId
        Set-ItemProperty $rp "Url"   $catDir
        Set-ItemProperty $rp "Flags" 1 -Type DWord
        Log-OK "Registered for Office $v"
    } catch { Log-Warn "Office $v: $_" }
}

# Step 4 - Shortcuts
Log-Head "Step 4/4 - Shortcuts"
$bat = Join-Path $ServerDir "start-server.bat"
("@echo off`r`n" +
 "chcp 437 > nul`r`n" +
 "title Ollama AI Plugin Server (keep open)`r`n" +
 "cd /d `"$ServerDir`"`r`n" +
 "echo Keep this window open while using Word.`r`n" +
 "echo Close to stop the plugin.`r`n" +
 "echo.`r`n" +
 "node server.js`r`n" +
 "pause`r`n") | Set-Content $bat -Encoding ASCII
Log-OK "start-server.bat created"

$startup = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\OllamaAIServer.lnk"
try {
    $w = New-Object -ComObject WScript.Shell
    $s = $w.CreateShortcut($startup)
    $s.TargetPath = $bat; $s.WorkingDirectory = $ServerDir; $s.WindowStyle = 7; $s.Save()
    Log-OK "Auto-start shortcut created (minimized at login)"
} catch { Log-Warn "Startup shortcut failed: $_" }

try {
    $w2 = New-Object -ComObject WScript.Shell
    $s2 = $w2.CreateShortcut([Environment]::GetFolderPath("Desktop") + "\Ollama AI Server.lnk")
    $s2.TargetPath = $bat; $s2.WorkingDirectory = $ServerDir; $s2.WindowStyle = 1; $s2.Save()
    Log-OK "Desktop shortcut: 'Ollama AI Server'"
} catch {}

# Ollama check
Write-Host ""
try {
    Invoke-WebRequest -Uri "http://localhost:11434/api/version" -TimeoutSec 3 -UseBasicParsing -EA Stop | Out-Null
    Log-OK "Ollama is running!"
} catch {
    Log-Warn "Ollama not found on port 11434"
    Log-Info "Get it at: https://ollama.ai/download"
    Log-Info "Then run:  ollama pull llama3.2"
}

# Start server
$nc = Get-Command node -ErrorAction SilentlyContinue
if ($nc) {
    $sjs = Join-Path $ServerDir "server.js"
    Start-Process -FilePath $nc.Source -ArgumentList "`"$sjs`"" -WorkingDirectory $ServerDir -WindowStyle Normal
    Start-Sleep 2
    Log-OK "Server started"
}

Write-Host ""
Write-Host "  ================================================" -ForegroundColor Green
Write-Host "    Installation complete!                        " -ForegroundColor Green
Write-Host "  ================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  NEXT STEPS IN WORD:"
Write-Host "    1. Open Word"
Write-Host "    2. Insert -> Get Add-ins -> Shared Folder tab"
Write-Host "    3. Select 'Ollama AI' -> Add"
Write-Host "    4. Button appears in the Home tab ribbon"
Write-Host ""

$ans = Read-Host "Open Word now? (Y/n)"
if ($ans -ne "n") { Start-Process "winword.exe" -EA SilentlyContinue }
