import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

print('=== 987: Отладка в VS / VS Code ===')

post(987, 'text', {'markdown': """## Отладка в VS / VS Code

Отладка — ключевой навык. Научись читать код «глазами компьютера».

### Основные инструменты

| Инструмент | Клавиша (VS) | Описание |
|-----------|-------------|----------|
| Точка останова | F9 | Программа паузируется на этой строке |
| Запустить с отладкой | F5 | Старт в режиме Debug |
| Step Over | F10 | Выполнить строку, не заходя в метод |
| Step Into | F11 | Зайти внутрь вызываемого метода |
| Продолжить | F5 | Продолжить до следующей точки |

### Окна отладчика
- **Locals** — текущие значения переменных
- **Watch** — отслеживать конкретные выражения
- **Call Stack** — цепочка вызовов методов

### Условные точки останова
Правая кнопка по точке → **Conditions** → например `i == 5`.
Программа остановится только когда условие истинно — очень удобно в циклах!"""})

post(987, 'code', {'language': 'csharp', 'title': 'Код для отладки — проследи sum в Locals', 'code': """int sum = 0;
for (int i = 1; i <= 5; i++)
{
    sum += i;   // поставь точку останова (F9) сюда
                // на каждом шаге смотри sum в окне Locals
}
Console.WriteLine($"Сумма: {sum}"); // ожидаем 15"""})

post(987, 'quiz', {'question': 'Что делает F10 (Step Over) в отладчике?', 'options': [
    'Перезапускает программу заново',
    'Выполняет строку, не заходя внутрь вызываемого метода',
    'Заходит внутрь метода в текущей строке',
    'Продолжает до следующей точки останова'
], 'correct': 1, 'explanation': 'F10 выполняет текущую строку целиком. Если там вызов метода — он выполнится, но отладчик не зайдёт внутрь. F11 (Step Into) — зашёл бы внутрь.'})

post(987, 'control_question', {'question': 'Какую клавишу нажать, чтобы поставить/убрать точку останова в Visual Studio?', 'answer': 'F9'})
print('Done!')
