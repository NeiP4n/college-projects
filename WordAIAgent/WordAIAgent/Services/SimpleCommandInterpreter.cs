using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using WordAIAgent.Models;

namespace WordAIAgent.Services
{
    public static class SimpleCommandInterpreter
    {
        public static List<ParsedCommand> TryParse(string input, string lastCommand)
        {
            var commands = new List<ParsedCommand>();
            var text = Normalize(input);
            if (string.IsNullOrWhiteSpace(text) || IsGreeting(text))
                return commands;

            if (ContainsAny(text, "отмени", "назад", "верни как было", "undo"))
            {
                commands.Add(new ParsedCommand("UNDO", ""));
                return commands;
            }

            if (ContainsAny(text, "повтори", "вперед", "redo"))
            {
                commands.Add(new ParsedCommand("REDO", ""));
                return commands;
            }

            if (ContainsAny(text, "удали весь текст", "удали всё", "удали все", "очисти документ", "очисти весь текст"))
            {
                commands.Add(new ParsedCommand("CLEAR_DOCUMENT", ""));
                return commands;
            }

            var replace = ParseReplace(text);
            if (replace != null)
            {
                commands.Add(replace);
                return commands;
            }

            if (IsDeleteTable(text, lastCommand))
            {
                commands.Add(new ParsedCommand("DELETE_TABLE", ""));
                return commands;
            }

            var deleteColumn = ParseDeleteColumn(text);
            if (deleteColumn != null)
            {
                commands.Add(deleteColumn);
                return commands;
            }

            var deleteRow = ParseDeleteRow(text);
            if (deleteRow != null)
            {
                commands.Add(deleteRow);
                return commands;
            }

            var insertTable = ParseInsertTable(text, out var moveToStart);
            if (insertTable != null)
            {
                if (moveToStart)
                    commands.Add(new ParsedCommand("GO_TO_START", ""));
                commands.Add(insertTable);
                return commands;
            }

            var gost19 = ParseGost19(text);
            if (gost19 != null)
            {
                commands.Add(gost19);
                return commands;
            }

            var pieChart = ParsePieChart(text);
            if (pieChart != null)
            {
                commands.Add(pieChart);
                return commands;
            }

            var smartArt = ParseSmartArt(text);
            if (smartArt != null)
            {
                commands.Add(smartArt);
                return commands;
            }

            var formatCommands = ParseFormatting(text);
            if (formatCommands.Count > 0)
                return formatCommands;

            return commands;
        }

        private static ParsedCommand ParseSmartArt(string text)
        {
            if (ContainsAny(text, "смарт арт", "smartart"))
                return new ParsedCommand("EXECUTE_MSO", "SmartArtInsert");
            return null;
        }

        private static ParsedCommand ParsePieChart(string text)
        {
            if (ContainsAny(text, "кругов") && ContainsAny(text, "диаграм"))
                return new ParsedCommand("INSERT_PIE_CHART", "");
            if (ContainsAny(text, "pie chart", "piechart"))
                return new ParsedCommand("INSERT_PIE_CHART", "");
            return null;
        }

        private static ParsedCommand ParseGost19(string text)
        {
            if (ContainsAny(text, "гост 19", "гост-19", "гост19", "gost 19", "gost-19"))
                return new ParsedCommand("APPLY_GOST19", "");
            return null;
        }

        private static ParsedCommand ParseReplace(string text)
        {
            var match = Regex.Match(text, @"замени\s+(.+?)\s+на\s+(.+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var from = TrimQuotes(match.Groups[1].Value);
            var to = TrimQuotes(match.Groups[2].Value);
            if (string.IsNullOrWhiteSpace(from))
                return null;

            return new ParsedCommand("REPLACE", $"{from}|||{to}");
        }

        private static bool IsDeleteTable(string text, string lastCommand)
        {
            if (ContainsAny(text, "удали", "удалить", "убери", "удалите") && text.Contains("таблиц"))
                return true;

            if (ContainsAny(text, "удали", "убери") && ContainsAny(text, "ее", "её", "эту") &&
                string.Equals(lastCommand, "INSERT_TABLE", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static ParsedCommand ParseDeleteColumn(string text)
        {
            if (!ContainsAny(text, "удали", "удалить", "убери", "удалите"))
                return null;

            if (!ContainsAny(text, "столб", "колонк"))
                return null;

            var index = ExtractNumber(text);
            return new ParsedCommand("TABLE_DELETE_COLUMN", index?.ToString(CultureInfo.InvariantCulture) ?? "");
        }

        private static ParsedCommand ParseDeleteRow(string text)
        {
            if (!ContainsAny(text, "удали", "удалить", "убери", "удалите"))
                return null;

            if (!ContainsAny(text, "строк", "строку", "строка"))
                return null;

            var index = ExtractNumber(text);
            return new ParsedCommand("TABLE_DELETE_ROW", index?.ToString(CultureInfo.InvariantCulture) ?? "");
        }

        private static ParsedCommand ParseInsertTable(string text, out bool moveToStart)
        {
            moveToStart = ContainsAny(text, "вначал", "в начале", "сначала");

            if (!ContainsAny(text, "таблиц"))
                return null;

            if (!ContainsAny(text, "добав", "встав", "созда", "сделай"))
                return null;

            var rows = 0;
            var cols = 0;

            var rowMatch = Regex.Match(text, @"(\d+)\s*(строк|строка|строки)", RegexOptions.IgnoreCase);
            if (rowMatch.Success)
                rows = SafeInt(rowMatch.Groups[1].Value);

            var colMatch = Regex.Match(text, @"(\d+)\s*(столбц|столбца|столбцов|колонк)", RegexOptions.IgnoreCase);
            if (colMatch.Success)
                cols = SafeInt(colMatch.Groups[1].Value);

            if (rows == 0 && cols == 0)
            {
                var pair = Regex.Match(text, @"(\d+)\s*(?:x|х|×|на)\s*(\d+)", RegexOptions.IgnoreCase);
                if (pair.Success)
                {
                    rows = SafeInt(pair.Groups[1].Value);
                    cols = SafeInt(pair.Groups[2].Value);
                }
            }

            if (rows == 0 && cols == 0)
            {
                rows = 2;
                cols = 2;
            }
            else if (rows == 0)
            {
                rows = 2;
            }
            else if (cols == 0)
            {
                cols = 2;
            }

            return new ParsedCommand("INSERT_TABLE", $"{rows},{cols}");
        }

        private static List<ParsedCommand> ParseFormatting(string text)
        {
            var commands = new List<ParsedCommand>();
            var allText = ContainsAny(text, "весь текст", "все текст", "весь документ", "всё");
            if (allText)
                commands.Add(new ParsedCommand("SELECT_ALL", ""));

            if (ContainsAny(text, "жирн") && ContainsAny(text, "убери", "сними", "не "))
            {
                commands.Add(new ParsedCommand("BOLD_OFF", ""));
                return commands;
            }

            if (ContainsAny(text, "курсив") && ContainsAny(text, "убери", "сними", "не "))
            {
                commands.Add(new ParsedCommand("ITALIC_OFF", ""));
                return commands;
            }

            if (ContainsAny(text, "подчерк") && ContainsAny(text, "убери", "сними", "не "))
            {
                commands.Add(new ParsedCommand("UNDERLINE_OFF", ""));
                return commands;
            }

            if (ContainsAny(text, "жирн"))
                commands.Add(new ParsedCommand("BOLD_SELECTION", ""));
            else if (ContainsAny(text, "курсив"))
                commands.Add(new ParsedCommand("ITALIC_SELECTION", ""));
            else if (ContainsAny(text, "подчерк"))
                commands.Add(new ParsedCommand("UNDERLINE_SELECTION", ""));

            return commands;
        }

        private static bool IsGreeting(string text)
        {
            return ContainsAny(text, "привет", "здравств", "добрый день", "добрый вечер", "доброе утро");
        }

        private static string Normalize(string text)
        {
            return (text ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Replace('ё', 'е');
        }

        private static bool ContainsAny(string text, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (var token in tokens)
            {
                if (text.Contains(token))
                    return true;
            }

            return false;
        }

        private static int SafeInt(string value)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            return 0;
        }

        private static int? ExtractNumber(string text)
        {
            var match = Regex.Match(text, @"\b(\d+)\b");
            if (!match.Success)
                return null;
            return SafeInt(match.Groups[1].Value);
        }

        private static string TrimQuotes(string value)
        {
            var text = (value ?? string.Empty).Trim();
            return text.Trim('\"', '\'', '«', '»');
        }
    }
}
