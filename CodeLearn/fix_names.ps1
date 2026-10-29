add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustFix : ICertificatePolicy {
    public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem) { return true; }
}
"@ -ErrorAction SilentlyContinue
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustFix -ErrorAction SilentlyContinue
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
$BASE = "https://localhost:7187"
function Invoke-Api($method, $url, $body) {
    if ($body) {
        $json  = $body | ConvertTo-Json -Depth 10 -Compress
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
        return Invoke-RestMethod -Uri "$BASE$url" -Method $method -Body $bytes -ContentType "application/json; charset=utf-8"
    }
    return Invoke-RestMethod -Uri "$BASE$url" -Method $method
}

# Фикс названий уроков
$lessonNames = @{
    1  = "Что такое C# и .NET"
    2  = "Установка окружения"
    4  = "Команды dotnet CLI"
    5  = "NuGet — менеджер пакетов"
    6  = "Компиляция и IL"
    7  = "CLR — Common Language Runtime"
    8  = "JIT и AOT компиляция"
    9  = "Сборки и пространства имён"
    10 = "Переменные и константы"
    11 = "Типы данных"
    12 = "Nullable типы"
    13 = "Операторы"
    14 = "Строки"
    15 = "Приведение типов и Boxing"
}
foreach ($id in $lessonNames.Keys) {
    Invoke-Api "PUT" "/api/builder/lessons/$id" @{ title=$lessonNames[$id]; isLocked=$false } | Out-Null
    Write-Host "OK: урок $id = $($lessonNames[$id])"
}

# Перезаписываем урок 2 — без инструкций по установке, только концепции
Invoke-Api "GET" "/api/builder/lessons/2/blocks" $null | ForEach-Object {
    Invoke-Api "DELETE" "/api/builder/blocks/$($_.id)" $null | Out-Null
}

Invoke-Api "POST" "/api/builder/lessons/2/blocks" @{
    type="text"; order=0
    content=('{"markdown":"## Окружение для работы с C#\n\n### .NET SDK\n.NET SDK — это всё необходимое для разработки: компилятор, runtime, CLI-инструменты. Бесплатно скачивается с **dotnet.microsoft.com**.\n\n### IDE (среда разработки)\n\nIDE превращает написание кода в удобный процесс: подсветка, автодополнение, отладка.\n\n| IDE | Особенности |\n|-----|-------------|\n| **Visual Studio 2022** | Самая полная для .NET, мощный отладчик, бесплатная Community-версия |\n| **JetBrains Rider** | Быстрая, умная, кроссплатформенная |\n| **VS Code** | Лёгкая, расширяемая, для любых языков |\n\n### Первый проект\n\nПосле установки SDK создать проект — одна команда. dotnet сам создаёт структуру файлов, настраивает зависимости и сразу запускает.\n\n```\nMyFirstApp/\n├── MyFirstApp.csproj    <- описание проекта\n├── Program.cs           <- точка входа (здесь твой код)\n└── obj/                 <- временные файлы сборки\n```\n\n> **Главная идея:** .NET SDK + любая IDE = полное окружение для разработки на C#. Остальное — детали конкретного инструмента."}')
} | Out-Null

Invoke-Api "POST" "/api/builder/lessons/2/blocks" @{
    type="code"; order=1
    content=('{"language":"bash","code":"# Проверить что SDK установлен\ndotnet --version\n\n# Создать новый консольный проект\ndotnet new console -n MyFirstApp\n\n# Зайти в папку проекта\ncd MyFirstApp\n\n# Запустить\ndotnet run\n# Output: Hello, World!"}')
} | Out-Null

Write-Host "OK: урок 2 обновлён"
Write-Host "Готово!"