# Ollama AI Plugin for Microsoft Word 2021

## Quick Install (3 steps)

1. Install Ollama: https://ollama.ai/download

2. Open Command Prompt, run:
      ollama pull llama3.2

3. Double-click INSTALL.bat (as Administrator)

---

## How it works (Office Web Add-in)

This plugin uses the modern Office JS Add-in system.
It does NOT require VSTO or .NET compilation.
It runs a local HTTPS server (Node.js) that Word connects to.

---

## After install - add to Word

1. Open Word
2. Insert tab -> Get Add-ins
3. Click "Shared Folder" tab
4. Select "Ollama AI" -> click Add
5. Button appears in the Home tab ribbon

---

## Starting the server

The server starts automatically on Windows login.
Or double-click: "Ollama AI Server" on your Desktop.
Keep the server window open while using Word.

---

## Features

TEXT tab:
  - Improve style and grammar
  - Fix grammar only
  - Shorten / Expand
  - Translate (10 languages)
  - Change tone (8 options)

GENERATE tab:
  - Generate from prompt
  - Continue text from cursor
  - Generate table / list / headings

ANALYZE tab:
  - Full document analysis
  - Extract key points
  - Find logical errors
  - Word count + readability score
  - Ask any question about your document

CHAT tab:
  - Full AI chat with history
  - Add document to context

SETTINGS:
  - Custom Ollama URL (for remote servers)
  - Custom system prompt

---

## Requirements

- Windows 10 / 11
- Microsoft Word 2021 or Microsoft 365
- Node.js (installed automatically)
- Ollama running locally

---

## Troubleshooting

Plugin not loading in Word:
  - Make sure "Ollama AI Server" is running (check taskbar / start it from Desktop)
  - In Word: Insert -> Get Add-ins -> Shared Folder -> Add "Ollama AI"

AI not responding:
  - Check Ollama is running: open http://localhost:11434 in browser
  - In plugin Settings tab, click "Reconnect"

Change Ollama server:
  - Open plugin -> Settings tab -> change URL -> Save

