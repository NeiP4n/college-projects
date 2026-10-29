using System.Text.RegularExpressions;

namespace CodeLearn.Helpers
{
    /// <summary>
    /// Очищает пользовательский ввод от потенциально опасных символов.
    /// EF Core уже использует параметризованные запросы — это дополнительный слой.
    /// </summary>
    public static class InputSanitizer
    {
        // Разрешённые символы для никнейма: буквы, цифры, дефис, подчёркивание
        private static readonly Regex _nicknameRegex =
            new(@"^[\w\-а-яА-ЯёЁ]{3,20}$", RegexOptions.Compiled);

        // Удаляем всё кроме разрешённых символов никнейма
        private static readonly Regex _stripDangerous =
            new(@"[^\w\-а-яА-ЯёЁ]", RegexOptions.Compiled);

        public static string SanitizeNickname(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            return _stripDangerous.Replace(trimmed, "");
        }

        public static bool IsValidNickname(string input) =>
            !string.IsNullOrWhiteSpace(input) && _nicknameRegex.IsMatch(input);

        // Базовая очистка произвольной строки (удаляет HTML/script теги)
        public static string StripHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
