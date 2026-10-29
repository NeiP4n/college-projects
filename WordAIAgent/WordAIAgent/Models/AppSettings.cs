using System;

namespace WordAIAgent.Models
{
    public class AppSettings
    {
        public const int CurrentPromptVersion = 5;

        private const string LegacySystemPrompt = @"You are an AI editor for Microsoft Word.
Your job is to execute high-level user requests by returning Word actions.

Return STRICT JSON only. No markdown. No explanations.

Valid output shapes:
1) { ""action"": ""ActionName"", ""params"": { ... } }
2) { ""actions"": [ { ""action"": ""ActionName"", ""params"": { ... } } ] }

Prefer high-level actions when possible:
- FormatDocumentGOST
- SetHeadingLevel (params: level)
- SetJustifyAll
- SetFirstLineIndentCm (params: value)
- RemoveDoubleSpaces
- RemoveManualLineBreaks
- SetParagraphSpacing (params: before, after)
- SetPageMarginsCm (params: top, bottom, left, right)
- CreateTableOfContents
- UpdateTableOfContents
- InsertPageNumbers
- InsertSectionBreak
- SetPageOrientation (params: value=portrait|landscape)
- ExportPDF

Other allowed actions:
NewDocument, SaveDocument, InsertText, AppendParagraph, FindReplace,
Bold, Italic, Underline, AlignLeft, AlignCenter, AlignRight, AlignJustify,
ApplyHeading, ApplyStyle, CreateBulletList, CreateNumberedList, InsertTable,
InsertHeader, InsertFooter, InsertDateTime, CheckSpelling.

For complex tasks, return multiple actions in order.
If a parameter is missing, choose safe defaults.";

        public static string DefaultSystemPrompt => @"You are an AI assistant embedded in Microsoft Word.
When performing document actions, use ONLY the [CMD:...] tags from the allowed list.
First briefly explain what you will do, then output the required tag(s).
Never output JSON. Never use markdown code blocks.
If the user requests an action, you MUST output at least one [CMD:...] tag.
Do not invent new commands. If a command is not supported, say so.
Reply in Russian. Never insert the service context into the document.";

        public string OllamaHost { get; set; } = "http://0.0.0.0:11434";
        public string ModelName { get; set; } = "llama3";
        public int MaxTokens { get; set; } = 2048;
        public double Temperature { get; set; } = 0.2;
        public string ExecutionMode { get; set; } = "manual";
        public int PromptVersion { get; set; } = CurrentPromptVersion;
        public string SystemPrompt { get; set; } = DefaultSystemPrompt;

        public bool Normalize()
        {
            var changed = false;

            if (string.IsNullOrWhiteSpace(OllamaHost))
            {
                OllamaHost = "http://0.0.0.0:11434";
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(ModelName))
            {
                ModelName = "llama3";
                changed = true;
            }

            if (MaxTokens <= 0)
            {
                MaxTokens = 2048;
                changed = true;
            }

            if (Temperature < 0 || Temperature > 2)
            {
                Temperature = 0.2;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(ExecutionMode))
            {
                ExecutionMode = "manual";
                changed = true;
            }
            else
            {
                var mode = ExecutionMode.Trim().ToLowerInvariant();
                if (mode != "manual" && mode != "full")
                {
                    ExecutionMode = "manual";
                    changed = true;
                }
                else
                {
                    ExecutionMode = mode;
                }
            }

            if (string.IsNullOrWhiteSpace(SystemPrompt) ||
                PromptVersion < CurrentPromptVersion ||
                IsLegacyPrompt(SystemPrompt))
            {
                SystemPrompt = DefaultSystemPrompt;
                PromptVersion = CurrentPromptVersion;
                changed = true;
            }

            if (PromptVersion < CurrentPromptVersion)
            {
                PromptVersion = CurrentPromptVersion;
                changed = true;
            }

            return changed;
        }

        private static bool IsLegacyPrompt(string prompt)
        {
            var value = (prompt ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
                return true;

            if (string.Equals(value, LegacySystemPrompt.Trim(), StringComparison.Ordinal))
                return true;

            if (value.IndexOf("Return STRICT JSON only", StringComparison.OrdinalIgnoreCase) >= 0 &&
                value.IndexOf("Valid output shapes", StringComparison.OrdinalIgnoreCase) >= 0 &&
                value.IndexOf("Other allowed actions", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (value.IndexOf("\"action\"", StringComparison.OrdinalIgnoreCase) >= 0 &&
                value.IndexOf("\"params\"", StringComparison.OrdinalIgnoreCase) >= 0 &&
                value.IndexOf("JSON", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }
    }
}
