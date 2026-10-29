@echo off
chcp 437 > nul
title Ollama AI Plugin - Install

net session > nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrator rights...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer\Install.ps1"
if %errorlevel% neq 0 (
    echo.
    echo  Error during install. Try running installer\Install.ps1 manually.
    pause
)
