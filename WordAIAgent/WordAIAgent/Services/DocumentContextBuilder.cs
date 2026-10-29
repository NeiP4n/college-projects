using System;
using System.Text;
using Microsoft.Office.Interop.Word;

namespace WordAIAgent.Services
{
    public static class DocumentContextBuilder
    {
        public static string Build()
        {
            var ws = ThisAddIn.WordService;
            if (!ws.IsDocumentOpen())
                return "Document is not open.";

            var app = Globals.ThisAddIn.Application;
            var doc = app.ActiveDocument;
            var sel = app.Selection;

            var sb = new StringBuilder();
            sb.AppendLine($"Document: {doc.Name}");
            sb.AppendLine(ws.GetDocumentStats());

            var selected = ws.GetSelectedText();
            if (!string.IsNullOrWhiteSpace(selected))
                sb.AppendLine("SelectedText: " + Trim(selected, 600));
            else
                sb.AppendLine("SelectedText: (empty)");

            sb.AppendLine("Page: " + SafeGetPage(sel));
            sb.AppendLine("CursorParagraph: " + SafeGetParagraphIndex(doc, sel));

            return sb.ToString().Trim();
        }

        private static int SafeGetParagraphIndex(Document doc, Selection sel)
        {
            try
            {
                var range = doc.Range(0, sel.Start);
                return range.Paragraphs.Count;
            }
            catch
            {
                return 0;
            }
        }

        private static int SafeGetPage(Selection sel)
        {
            try
            {
                return sel.get_Information(WdInformation.wdActiveEndPageNumber);
            }
            catch
            {
                return 0;
            }
        }

        private static string Trim(string text, int maxLength)
        {
            var value = (text ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
            if (value.Length > maxLength)
                value = value.Substring(0, maxLength) + "...";
            return value;
        }
    }
}
