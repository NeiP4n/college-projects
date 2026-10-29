@echo off
chcp 437 > nul
title Ollama AI Plugin - Uninstall

net session > nul 2>&1
if %errorlevel% neq 0 (
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer\Install.ps1" -Uninstall
pause
