using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordAIAgent.Services
{
    public class CommandDefinition
    {
        public string Name { get; }
        public string Syntax { get; }
        public string Description { get; }
        public bool IsDestructive { get; }

        public CommandDefinition(string name, string syntax, string description, bool isDestructive = false)
        {
            Name = name;
            Syntax = syntax;
            Description = description;
            IsDestructive = isDestructive;
        }
    }

    public static class CommandCatalog
    {
        private static readonly List<CommandDefinition> Commands = new List<CommandDefinition>
        {
            new CommandDefinition("GO_TO_START", "", "Move cursor to the start of document."),
            new CommandDefinition("SELECT_ALL", "", "Select entire document."),
            new CommandDefinition("INSERT_TEXT", "text", "Insert text at the cursor."),
            new CommandDefinition("INSERT_PARAGRAPH", "text", "Insert a new paragraph with text."),
            new CommandDefinition("REPLACE", "old|||new", "Replace all occurrences in the document."),
            new CommandDefinition("DELETE_SELECTION", "", "Delete current selection.", true),
            new CommandDefinition("CLEAR_DOCUMENT", "", "Delete all document content.", true),
            new CommandDefinition("BOLD_SELECTION", "", "Make selection bold."),
            new CommandDefinition("BOLD_OFF", "", "Remove bold from selection."),
            new CommandDefinition("ITALIC_SELECTION", "", "Make selection italic."),
            new CommandDefinition("ITALIC_OFF", "", "Remove italic from selection."),
            new CommandDefinition("UNDERLINE_SELECTION", "", "Underline selection."),
            new CommandDefinition("UNDERLINE_OFF", "", "Remove underline from selection."),
            new CommandDefinition("UPPERCASE_SELECTION", "", "Uppercase selection."),
            new CommandDefinition("SET_FONT_SIZE", "size", "Set font size in points."),
            new CommandDefinition("SET_FONT", "font name", "Set font name for selection."),
            new CommandDefinition("SET_FONT_COLOR", "r,g,b or #RRGGBB", "Set font color."),
            new CommandDefinition("HIGHLIGHT_YELLOW", "", "Highlight selection with yellow."),
            new CommandDefinition("ALIGN_CENTER", "", "Center align current paragraph."),
            new CommandDefinition("ALIGN_JUSTIFY_ALL", "", "Justify all paragraphs in document."),
            new CommandDefinition("LINE_SPACING", "value", "Set line spacing (e.g., 1.5, 2.0)."),
            new CommandDefinition("INSERT_TABLE", "rows,cols", "Insert a table at cursor."),
            new CommandDefinition("INSERT_PIE_CHART", "", "Insert a pie chart at the cursor."),
            new CommandDefinition("TABLE_ADD_ROW", "", "Add row to active table."),
            new CommandDefinition("TABLE_ADD_COLUMN", "", "Add column to active table."),
            new CommandDefinition("TABLE_DELETE_ROW", "index?", "Delete row by index or current row.", true),
            new CommandDefinition("TABLE_DELETE_COLUMN", "index?", "Delete column by index or current column.", true),
            new CommandDefinition("DELETE_TABLE", "", "Delete current or last table.", true),
            new CommandDefinition("INSERT_PAGE_BREAK", "", "Insert a page break."),
            new CommandDefinition("INSERT_LINE_BREAK", "", "Insert a line break."),
            new CommandDefinition("SAVE_DOCUMENT", "", "Save the active document."),
            new CommandDefinition("EXPORT_PDF", "path", "Export document as PDF."),
            new CommandDefinition("ADD_PAGE_NUMBERS", "bottom,center", "Insert page numbers."),
            new CommandDefinition("SET_HEADER", "text", "Set header text."),
            new CommandDefinition("SET_FOOTER", "text", "Set footer text."),
            new CommandDefinition("SET_MARGINS", "top,left,bottom,right (cm)", "Set page margins."),
            new CommandDefinition("PAGE_LANDSCAPE", "", "Switch to landscape orientation."),
            new CommandDefinition("PAGE_PORTRAIT", "", "Switch to portrait orientation."),
            new CommandDefinition("INSERT_DATE", "", "Insert current date/time."),
            new CommandDefinition("INSERT_TOC", "", "Insert table of contents."),
            new CommandDefinition("SPELL_CHECK", "", "Run spelling and grammar check."),
            new CommandDefinition("SHOW_STATS", "", "Show document statistics."),
            new CommandDefinition("COUNT_WORDS", "", "Show word count."),
            new CommandDefinition("APPLY_GOST19", "", "Apply GOST 19 template and formatting."),
            new CommandDefinition("INSERT_HEADING1", "text", "Insert Heading 1."),
            new CommandDefinition("INSERT_HEADING2", "text", "Insert Heading 2."),
            new CommandDefinition("INSERT_BULLET_LIST", "item1|item2|item3", "Insert bullet list."),
            new CommandDefinition("NUMBERED_LIST_SELECTION", "", "Apply numbered list to selection."),
            new CommandDefinition("UNDO", "", "Undo last action."),
            new CommandDefinition("REDO", "", "Redo last action."),
            new CommandDefinition("EXECUTE_MSO", "controlId", "Execute built-in Word command by controlId."),
            new CommandDefinition("RUN_VBA", "code", "Run provided VBA code.", true)
        };

        public static IReadOnlyList<CommandDefinition> All => Commands;

        public static bool IsSupported(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            return Commands.Any(c => c.Name == name);
        }

        public static bool IsDestructive(string name)
        {
            return Commands.Any(c => c.Name == name && c.IsDestructive);
        }

        public static string BuildPrompt()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Доступные команды (используй ТОЛЬКО эти теги):");
            foreach (var cmd in Commands)
            {
                var syntax = string.IsNullOrWhiteSpace(cmd.Syntax) ? "" : cmd.Syntax;
                sb.AppendLine($"[CMD:{cmd.Name}]{syntax}[/CMD] - {cmd.Description}");
            }

            sb.AppendLine();
            sb.AppendLine("Если пользователь просит действие, обязательно выдай хотя бы один тег команды.");
            sb.AppendLine();
            sb.AppendLine("Примеры:");
            sb.AppendLine("Пользователь: Вставь заголовок Введение");
            sb.AppendLine("Ассистент: Вставлю заголовок H1. [CMD:INSERT_HEADING1]Введение[/CMD]");
            sb.AppendLine("Пользователь: Замени все 'компания' на 'организация'");
            sb.AppendLine("Ассистент: Выполняю замену. [CMD:REPLACE]компания|||организация[/CMD]");
            sb.AppendLine("Пользователь: Сохрани документ");
            sb.AppendLine("Ассистент: Сохраняю. [CMD:SAVE_DOCUMENT][/CMD]");
            sb.AppendLine("Пользователь: Создай таблицу 3 на 4");
            sb.AppendLine("Ассистент: Создаю таблицу 3x4. [CMD:INSERT_TABLE]3,4[/CMD]");
            sb.AppendLine("Пользователь: Открой вставку SmartArt");
            sb.AppendLine("Ассистент: Открываю SmartArt. [CMD:EXECUTE_MSO]SmartArtInsert[/CMD]");

            return sb.ToString();
        }
    }
}
