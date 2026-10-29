Get-Process -Name "CodeLearn" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500
Set-Location "$PSScriptRoot\CodeLearn"
dotnet run
