@echo off
chcp 65001 > nul
title Ollama AI Plugin - Build and Install

net session > nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrator rights...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer\Build_And_Install.ps1"

if %errorlevel% neq 0 (
    echo.
    echo Failed. See error above.
    pause
)
