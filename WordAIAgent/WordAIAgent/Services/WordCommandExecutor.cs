using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using WordAIAgent.Models;
using Word = Microsoft.Office.Interop.Word;

namespace WordAIAgent.Services
{
    public class CommandExecutionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Cancelled { get; set; }
    }

    public class WordCommandExecutor
    {
        private readonly WordService _ws;

        public WordCommandExecutor(WordService wordService)
        {
            _ws = wordService;
        }

        public CommandExecutionResult Execute(ParsedCommand command)
        {
            var result = new CommandExecutionResult { Success = true };
            if (command == null)
            {
                result.Success = false;
                result.Message = "Command is null.";
                return result;
            }

            if (CommandCatalog.IsDestructive(command.Name))
            {
                if (!ConfirmDestructive(command.Name))
                {
                    result.Success = false;
                    result.Cancelled = true;
                    result.Message = "Operation cancelled by user.";
                    return result;
                }
            }

            try
            {
                _ws.EnsureDocumentReady();
                switch (command.Name)
                {
                    case "INSERT_TEXT":
                        _ws.InsertText(command.Payload);
                        return result;
                    case "GO_TO_START":
                        _ws.GoToStart();
                        return result;
                    case "SELECT_ALL":
                        _ws.SelectAll();
                        return result;
                    case "INSERT_PARAGRAPH":
                        _ws.AppendParagraph(command.Payload);
                        return result;
                    case "REPLACE":
                        Replace(command.Payload);
                        return result;
                    case "DELETE_SELECTION":
                        _ws.DeleteSelectedText();
                        return result;
                    case "CLEAR_DOCUMENT":
                        _ws.DeleteAllText();
                        return result;
                    case "BOLD_SELECTION":
                        EnsureSelectionOrAll();
                        _ws.SetBold(true);
                        return result;
                    case "BOLD_OFF":
                        EnsureSelectionOrAll();
                        _ws.SetBold(false);
                        return result;
                    case "ITALIC_SELECTION":
                        EnsureSelectionOrAll();
                        _ws.SetItalic(true);
                        return result;
                    case "ITALIC_OFF":
                        EnsureSelectionOrAll();
                        _ws.SetItalic(false);
                        return result;
                    case "UNDERLINE_SELECTION":
                        EnsureSelectionOrAll();
                        _ws.SetUnderline(true);
                        return result;
                    case "UNDERLINE_OFF":
                        EnsureSelectionOrAll();
                        _ws.SetUnderline(false);
                        return result;
                    case "UPPERCASE_SELECTION":
                        EnsureSelectionOrAll();
                        _ws.SetAllCaps(true);
                        return result;
                    case "SET_FONT_SIZE":
                        EnsureSelectionOrAll();
                        _ws.SetFontSize(ParseFloat(command.Payload, 12f));
                        return result;
                    case "SET_FONT":
                        EnsureSelectionOrAll();
                        _ws.SetFontName(command.Payload);
                        return result;
                    case "SET_FONT_COLOR":
                        EnsureSelectionOrAll();
                        _ws.SetFontColor(ParseColor(command.Payload));
                        return result;
                    case "HIGHLIGHT_YELLOW":
                        EnsureSelectionOrAll();
                        _ws.SetHighlight(WdColorIndex.wdYellow);
                        return result;
                    case "ALIGN_CENTER":
                        _ws.AlignCenter();
                        return result;
                    case "ALIGN_JUSTIFY_ALL":
                        _ws.SelectAll();
                        _ws.AlignJustify();
                        return result;
                    case "LINE_SPACING":
                        _ws.SetLineSpacingMultiple(ParseFloat(command.Payload, 1.5f));
                        return result;
                    case "INSERT_TABLE":
                        InsertTable(command.Payload);
                        return result;
                    case "INSERT_PIE_CHART":
                        _ws.InsertPieChart();
                        return result;
                    case "TABLE_ADD_ROW":
                        _ws.AddTableRow();
                        return result;
                    case "TABLE_ADD_COLUMN":
                        _ws.AddTableColumn();
                        return result;
                    case "TABLE_DELETE_ROW":
                        DeleteTableRow(command.Payload);
                        return result;
                    case "TABLE_DELETE_COLUMN":
                        DeleteTableColumn(command.Payload);
                        return result;
                    case "DELETE_TABLE":
                        _ws.DeleteCurrentOrLastTable();
                        return result;
                    case "INSERT_PAGE_BREAK":
                        _ws.InsertPageBreak();
                        return result;
                    case "INSERT_LINE_BREAK":
                        InsertLineBreak();
                        return result;
                    case "SAVE_DOCUMENT":
                        _ws.SaveDocument();
                        return result;
                    case "EXPORT_PDF":
                        ExportPdf(command.Payload);
                        return result;
                    case "ADD_PAGE_NUMBERS":
                        _ws.InsertPageNumbers();
                        return result;
                    case "SET_HEADER":
                        _ws.InsertHeader(command.Payload);
                        return result;
                    case "SET_FOOTER":
                        _ws.InsertFooter(command.Payload);
                        return result;
                    case "SET_MARGINS":
                        SetMargins(command.Payload);
                        return result;
                    case "PAGE_LANDSCAPE":
                        _ws.SetPageOrientation(Word.WdOrientation.wdOrientLandscape);
                        return result;
                    case "PAGE_PORTRAIT":
                        _ws.SetPageOrientation(Word.WdOrientation.wdOrientPortrait);
                        return result;
                    case "INSERT_DATE":
                        _ws.InsertDateTime();
                        return result;
                    case "INSERT_TOC":
                        _ws.InsertTableOfContents();
                        return result;
                    case "SPELL_CHECK":
                        _ws.CheckSpellingAndGrammar();
                        return result;
                    case "SHOW_STATS":
                        result.Message = _ws.GetDocumentStats();
                        return result;
                    case "COUNT_WORDS":
                        result.Message = _ws.GetWordCount();
                        return result;
                    case "INSERT_HEADING1":
                        InsertHeading(command.Payload, 1);
                        return result;
                    case "INSERT_HEADING2":
                        InsertHeading(command.Payload, 2);
                        return result;
                    case "INSERT_BULLET_LIST":
                        InsertBulletList(command.Payload);
                        return result;
                    case "NUMBERED_LIST_SELECTION":
                        _ws.CreateNumberedList();
                        return result;
                    case "UNDO":
                        _ws.Undo();
                        return result;
                    case "REDO":
                        _ws.Redo();
                        return result;
                    case "APPLY_GOST19":
                        var inserted = _ws.ApplyGost19Template();
                        result.Message = inserted
                            ? "Вставлен шаблон ГОСТ 19."
                            : "Применено форматирование ГОСТ 19.";
                        return result;
                    case "EXECUTE_MSO":
                        ExecuteMso(command.Payload);
                        return result;
                    case "RUN_VBA":
                        RunVba(command.Payload);
                        return result;
                    default:
                        result.Success = false;
                        result.Message = $"Unsupported command: {command.Name}";
                        return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
        }

        private void Replace(string payload)
        {
            var parts = (payload ?? string.Empty).Split(new[] { "|||" }, StringSplitOptions.None);
            if (parts.Length < 2)
                return;
            _ws.FindAndReplace(parts[0], parts[1]);
        }

        private void InsertTable(string payload)
        {
            var parts = (payload ?? string.Empty).Split(',');
            var rows = parts.Length > 0 ? ParseInt(parts[0], 2) : 2;
            var cols = parts.Length > 1 ? ParseInt(parts[1], 2) : 2;
            _ws.InsertTable(Math.Max(1, rows), Math.Max(1, cols));
        }

        private void DeleteTableRow(string payload)
        {
            var index = ParseOptionalInt(payload);
            _ws.DeleteTableRow(index);
        }

        private void DeleteTableColumn(string payload)
        {
            var index = ParseOptionalInt(payload);
            _ws.DeleteTableColumn(index);
        }

        private void InsertLineBreak()
        {
            var app = Globals.ThisAddIn.Application;
            app.Selection.InsertBreak(WdBreakType.wdLineBreak);
        }

        private void ExportPdf(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var path = System.IO.Path.Combine(desktop, $"WordAIAgent_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                _ws.ExportAsPDF(path);
                return;
            }

            _ws.ExportAsPDF(payload);
        }

        private void SetMargins(string payload)
        {
            var parts = (payload ?? string.Empty).Split(',');
            var top = parts.Length > 0 ? ParseFloat(parts[0], 2f) : 2f;
            var left = parts.Length > 1 ? ParseFloat(parts[1], 2f) : 2f;
            var bottom = parts.Length > 2 ? ParseFloat(parts[2], 2f) : 2f;
            var right = parts.Length > 3 ? ParseFloat(parts[3], 2f) : 2f;
            _ws.SetPageMarginsCm(top, bottom, left, right);
        }

        private void InsertHeading(string text, int level)
        {
            _ws.InsertText(text);
            _ws.ApplyHeading(level);
        }

        private void InsertBulletList(string payload)
        {
            var items = (payload ?? string.Empty).Split('|')
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();

            if (items.Count == 0)
                return;

            var app = Globals.ThisAddIn.Application;
            var start = app.Selection.Start;
            foreach (var item in items)
            {
                app.Selection.TypeText(item);
                app.Selection.TypeParagraph();
            }

            var end = app.Selection.End;
            var range = app.ActiveDocument.Range(start, end);
            range.Select();
            _ws.CreateBulletList();
        }

        private static bool ConfirmDestructive(string name)
        {
            var result = MessageBox.Show(
                $"Confirm destructive operation: {name}?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }

        private static int ParseInt(string value, int defaultValue)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            if (int.TryParse(value, out parsed))
                return parsed;
            return defaultValue;
        }

        private static int? ParseOptionalInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            if (int.TryParse(value, out parsed))
                return parsed;
            return null;
        }

        private static float ParseFloat(string value, float defaultValue)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            if (float.TryParse(value, out parsed))
                return parsed;
            return defaultValue;
        }

        private static System.Drawing.Color ParseColor(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return System.Drawing.Color.Black;

            value = value.Trim();
            if (value.StartsWith("#"))
            {
                try { return System.Drawing.ColorTranslator.FromHtml(value); } catch { }
            }

            var parts = value.Split(',');
            if (parts.Length == 3 &&
                int.TryParse(parts[0], out var r) &&
                int.TryParse(parts[1], out var g) &&
                int.TryParse(parts[2], out var b))
            {
                return System.Drawing.Color.FromArgb(r, g, b);
            }

            return System.Drawing.Color.Black;
        }

        private void EnsureSelectionOrAll()
        {
            if (_ws.IsSelectionEmpty())
                _ws.SelectAll();
        }

        private void ExecuteMso(string payload)
        {
            var controlId = (payload ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(controlId))
                return;
            _ws.ExecuteMso(controlId);
        }

        private void RunVba(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return;
            _ws.ExecuteTemporaryVba(payload);
        }
    }
}
