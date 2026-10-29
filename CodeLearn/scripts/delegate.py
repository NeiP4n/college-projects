#!/usr/bin/env python3
"""
delegate.py — отправляет задачу нужной модели через доступные API.

Использование:
  python scripts/delegate.py code   "задача"
  python scripts/delegate.py doc    "задача"
  python scripts/delegate.py lint   "задача"
  python scripts/delegate.py <provider>/<model>  "задача"

Приоритет по скорости/цене:
  code → Groq llama-3.1-70b (быстро + бесплатно)
  doc  → OpenRouter gemini-2.0-flash (качественно)
  lint → Cerebras llama3.1-8b (мгновенно)
"""

import sys, json, os
from pathlib import Path

KEYS_FILE = Path(__file__).parent / "api_keys.json"
if KEYS_FILE.exists():
    KEYS = json.loads(KEYS_FILE.read_text())
else:
    KEYS = {}

# Роутер: тип задачи → (provider, model, max_tokens)
ROUTES = {
    "code": ("groq",       "llama-3.3-70b-versatile",         4096),
    "doc":  ("openrouter", "google/gemini-2.0-flash-001",     4096),
    "lint": ("cerebras",   "llama3.1-8b",                      512),
    "fast": ("cerebras",   "llama3.1-8b",                     2048),
    "big":  ("sambanova",  "Meta-Llama-3.3-70B-Instruct",     4096),
}

PROVIDERS = {
    "groq":       "https://api.groq.com/openai/v1",
    "cerebras":   "https://api.cerebras.ai/v1",
    "sambanova":  "https://api.sambanova.ai/v1",
    "mistral":    "https://api.mistral.ai/v1",
    "openrouter": "https://openrouter.ai/api/v1",
}

OPENROUTER_HEADERS = {
    "HTTP-Referer": "https://codelearn.local",
    "X-Title": "CodeLearn"
}

SYSTEM = {
    "code": (
        "Ты помощник по Python. Генерируй чистый, рабочий код. "
        "Отвечай только кодом или JSON без объяснений, если не просят иное."
    ),
    "doc": (
        "Ты документатор. Пиши на русском языке, кратко и по делу. "
        "Объясняй ЗАЧЕМ (WHY), а не только ЧТО. Без вступлений."
    ),
    "lint": "Валидатор. Отвечай кратко: OK или список проблем.",
    "fast": "Ты помощник. Отвечай точно и кратко.",
    "big": (
        "Ты эксперт по Python. Генерируй подробные, качественные ответы."
    ),
}


def call_openai_compat(base_url, model, api_key, system, user, max_tokens, extra_headers=None):
    import urllib.request, urllib.error
    payload = json.dumps({
        "model": model,
        "messages": [
            {"role": "system", "content": system},
            {"role": "user",   "content": user}
        ],
        "max_tokens": max_tokens,
        "temperature": 0.3,
    }, ensure_ascii=False).encode("utf-8")

    headers = {
        "Authorization": f"Bearer {api_key}",
        "Content-Type":  "application/json",
        "User-Agent":    "CodeLearn/1.0",
    }
    if extra_headers:
        headers.update(extra_headers)

    req = urllib.request.Request(f"{base_url}/chat/completions", data=payload, headers=headers)
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            data = json.loads(r.read().decode("utf-8"))
            return data["choices"][0]["message"]["content"]
    except urllib.error.HTTPError as e:
        body = e.read().decode()[:300]
        raise RuntimeError(f"HTTP {e.code}: {body}")


def delegate(task_type: str, task: str, verbose: bool = True) -> str:
    # Поддержка прямого указания: "groq/llama-3.1-8b-instant"
    if "/" in task_type:
        provider, model = task_type.split("/", 1)
        max_tokens = 4096
        system_key = "fast"
    else:
        if task_type not in ROUTES:
            task_type = "code"
        provider, model, max_tokens = ROUTES[task_type]
        system_key = task_type

    base_url = PROVIDERS.get(provider)
    if not base_url:
        return f"❌ Неизвестный провайдер: {provider}"

    api_key = KEYS.get(provider, os.environ.get(f"{provider.upper()}_API_KEY", ""))
    if not api_key:
        return f"❌ Нет ключа для {provider}. Добавь в scripts/api_keys.json"

    system = SYSTEM.get(system_key, SYSTEM["fast"])
    extra = OPENROUTER_HEADERS if provider == "openrouter" else None

    if verbose:
        print(f"[{task_type.upper()} → {provider}/{model}]", flush=True)

    try:
        return call_openai_compat(base_url, model, api_key, system, task, max_tokens, extra)
    except Exception as e:
        return f"❌ {e}"


if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Использование: python scripts/delegate.py <code|doc|lint|big|provider/model> \"задача\"")
        sys.exit(1)

    task_type = sys.argv[1].lower()
    task      = " ".join(sys.argv[2:])
    result    = delegate(task_type, task)
    print(result)
