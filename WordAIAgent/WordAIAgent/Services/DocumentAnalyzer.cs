using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Office.Interop.Word;
using Newtonsoft.Json;
using WordAIAgent.Models;
using Word = Microsoft.Office.Interop.Word;

namespace WordAIAgent.Services
{
    /// <summary>
    /// Builds rich document context for AI prompts.
    /// </summary>
    public class DocumentAnalyzer
    {
        private readonly WordService _ws;

        public DocumentAnalyzer(WordService wordService) => _ws = wordService;

        public string BuildExecutionContext(string userQuery, int maxChars = 7500)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(userQuery))
            {
                sb.AppendLine("USER_REQUEST:");
                sb.AppendLine(userQuery.Trim());
                sb.AppendLine();
            }

            if (!_ws.IsDocumentOpen())
            {
                sb.AppendLine("DOCUMENT_STATE: No active document.");
                return Trim(sb.ToString(), maxChars);
            }

            try
            {
                var app = Globals.ThisAddIn.Application;
                var doc = app.ActiveDocument;
                var sel = app.Selection;

                AppendOverview(sb, doc, sel, app);
                AppendSelection(sb, sel);
                AppendOutline(sb, doc);
                AppendTables(sb, doc);
                AppendDocumentEdges(sb, doc);
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[Context error: {ex.Message}]");
            }

            return Trim(sb.ToString(), maxChars);
        }

        public string BuildExecutionContextJson(int maxChars = 2000)
        {
            var ctx = BuildContext();
            var json = JsonConvert.SerializeObject(ctx, Formatting.None);
            return Trim(json, maxChars);
        }

        public DocumentContext BuildContext()
        {
            var ctx = new DocumentContext();

            if (!_ws.IsDocumentOpen())
                return ctx;

            try
            {
                var app = Globals.ThisAddIn.Application;
                var doc = app.ActiveDocument;
                var sel = app.Selection;

                ctx.Pages = SafeStat(() => doc.ComputeStatistics(WdStatistic.wdStatisticPages));
                ctx.Tables = SafeStat(() => doc.Tables.Count);
                ctx.Images = SafeStat(() => doc.InlineShapes.Count + doc.Shapes.Count);
                ctx.Headers = HasHeader(doc);
                ctx.Footers = HasFooter(doc);
                ctx.Styles = SafeStyles(doc, 30);
                ctx.Outline = SafeOutline(doc, 20);
                ctx.TableInfo = SafeTableInfo(doc, 10);

                if (sel != null)
                {
                    ctx.Selection.Empty = sel.Start == sel.End;
                    ctx.Selection.InTable = sel.Tables.Count > 0;
                    ctx.Selection.InHeader = IsHeaderStory(sel.StoryType);
                    ctx.Selection.InFooter = IsFooterStory(sel.StoryType);
                    ctx.Selection.InTextBox = sel.StoryType == WdStoryType.wdTextFrameStory;
                    try { ctx.Selection.InShape = sel.ShapeRange != null && sel.ShapeRange.Count > 0; }
                    catch { ctx.Selection.InShape = false; }
                    ctx.Selection.Page = SafePage(sel);
                    ctx.Selection.ParagraphIndex = SafeParagraphIndex(doc, sel);
                }
            }
            catch
            {
            }

            return ctx;
        }

        private static void AppendOverview(StringBuilder sb, Word.Document doc, Selection sel, Word.Application app)
        {
            sb.AppendLine("DOCUMENT_STATE:");
            sb.AppendLine($"Name: {doc.Name}");
            sb.AppendLine($"Saved: {doc.Saved}");
            TryStat(sb, "Words", () => doc.ComputeStatistics(WdStatistic.wdStatisticWords).ToString());
            TryStat(sb, "Pages", () => doc.ComputeStatistics(WdStatistic.wdStatisticPages).ToString());
            TryStat(sb, "Tables", () => doc.Tables.Count.ToString());
            TryStat(sb, "TrackChanges", () => doc.TrackRevisions.ToString());
            sb.AppendLine();
        }

        private static void AppendSelection(StringBuilder sb, Selection sel)
        {
            sb.AppendLine("SELECTION:");
            sb.AppendLine($"Empty: {sel.Start == sel.End}");
            var selText = sel.Text ?? "";
            if (selText.Length > 0)
                sb.AppendLine($"SelectedText: {NormalizeText(selText, 600)}");

            // Current paragraph
            try
            {
                if (sel.Paragraphs.Count > 0)
                {
                    var pText = NormalizeText(sel.Paragraphs[1].Range.Text, 300);
                    if (!string.IsNullOrWhiteSpace(pText))
                        sb.AppendLine($"CurrentParagraph: {pText}");
                }
            }
            catch { }
            sb.AppendLine();
        }

        private static void AppendOutline(StringBuilder sb, Word.Document doc)
        {
            sb.AppendLine("OUTLINE:");
            int count = 0;
            int limit = Math.Min(doc.Paragraphs.Count, 300);
            for (int i = 1; i <= limit && count < 20; i++)
            {
                Word.Paragraph p = null;
                try
                {
                    p = doc.Paragraphs[i];
                    var lvl = p.OutlineLevel;
                    if (lvl >= WdOutlineLevel.wdOutlineLevel1 && lvl <= WdOutlineLevel.wdOutlineLevel9)
                    {
                        var text = NormalizeText(p.Range.Text, 120);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            sb.AppendLine($"  H{(int)lvl}: {text}");
                            count++;
                        }
                    }
                }
                catch { }
                finally { if (p != null) try { System.Runtime.InteropServices.Marshal.ReleaseComObject(p); } catch { } }
            }
            if (count == 0) sb.AppendLine("  (Заголовков не найдено)");
            sb.AppendLine();
        }

        private static void AppendTables(StringBuilder sb, Word.Document doc)
        {
            if (doc.Tables.Count == 0) return;
            sb.AppendLine($"TABLES: ({doc.Tables.Count} таблиц)");
            int max = Math.Min(doc.Tables.Count, 5);
            for (int i = 1; i <= max; i++)
            {
                try
                {
                    var t = doc.Tables[i];
                    sb.AppendLine($"  Таблица {i}: {t.Rows.Count}x{t.Columns.Count}");
                }
                catch { }
            }
            sb.AppendLine();
        }

        private static void AppendDocumentEdges(StringBuilder sb, Word.Document doc)
        {
            try
            {
                var full = doc.Content.Text ?? "";
                sb.AppendLine("DOCUMENT_BEGINNING:");
                sb.AppendLine(NormalizeText(full, 1500));
                if (full.Length > 1500)
                {
                    sb.AppendLine("DOCUMENT_END:");
                    sb.AppendLine(NormalizeText(full.Substring(Math.Max(0, full.Length - 800)), 800));
                }
            }
            catch { }
        }

        private static void TryStat(StringBuilder sb, string label, Func<string> getter)
        {
            try { sb.AppendLine($"{label}: {getter()}"); }
            catch { }
        }

        private static int SafeStat(Func<int> getter)
        {
            try { return getter(); }
            catch { return 0; }
        }

        private static bool HasHeader(Word.Document doc)
        {
            try
            {
                foreach (Word.Section sec in doc.Sections)
                {
                    foreach (WdHeaderFooterIndex idx in new[] {
                        WdHeaderFooterIndex.wdHeaderFooterPrimary,
                        WdHeaderFooterIndex.wdHeaderFooterFirstPage,
                        WdHeaderFooterIndex.wdHeaderFooterEvenPages })
                    {
                        if (!string.IsNullOrWhiteSpace(sec.Headers[idx].Range.Text)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool HasFooter(Word.Document doc)
        {
            try
            {
                foreach (Word.Section sec in doc.Sections)
                {
                    foreach (WdHeaderFooterIndex idx in new[] {
                        WdHeaderFooterIndex.wdHeaderFooterPrimary,
                        WdHeaderFooterIndex.wdHeaderFooterFirstPage,
                        WdHeaderFooterIndex.wdHeaderFooterEvenPages })
                    {
                        if (!string.IsNullOrWhiteSpace(sec.Footers[idx].Range.Text)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool IsHeaderStory(WdStoryType t) =>
            t == WdStoryType.wdPrimaryHeaderStory ||
            t == WdStoryType.wdFirstPageHeaderStory ||
            t == WdStoryType.wdEvenPagesHeaderStory;

        private static bool IsFooterStory(WdStoryType t) =>
            t == WdStoryType.wdPrimaryFooterStory ||
            t == WdStoryType.wdFirstPageFooterStory ||
            t == WdStoryType.wdEvenPagesFooterStory;

        private static int SafePage(Selection sel)
        {
            try { return sel.get_Information(WdInformation.wdActiveEndPageNumber); }
            catch { return 0; }
        }

        private static int SafeParagraphIndex(Word.Document doc, Selection sel)
        {
            try
            {
                var range = doc.Range(0, sel.Start);
                return range.Paragraphs.Count;
            }
            catch { return 0; }
        }

        private static List<string> SafeStyles(Word.Document doc, int max)
        {
            var list = new List<string>();
            try
            {
                foreach (Word.Style s in doc.Styles)
                {
                    try
                    {
                        var name = s.NameLocal;
                        if (!string.IsNullOrWhiteSpace(name)) list.Add(name);
                        if (list.Count >= max) break;
                    }
                    catch { }
                }
            }
            catch { }
            return list;
        }

        private static List<string> SafeOutline(Word.Document doc, int max)
        {
            var list = new List<string>();
            try
            {
                int limit = Math.Min(doc.Paragraphs.Count, 300);
                for (int i = 1; i <= limit && list.Count < max; i++)
                {
                    try
                    {
                        var p = doc.Paragraphs[i];
                        var lvl = p.OutlineLevel;
                        if (lvl >= WdOutlineLevel.wdOutlineLevel1 && lvl <= WdOutlineLevel.wdOutlineLevel9)
                        {
                            var text = NormalizeText(p.Range.Text, 120);
                            if (!string.IsNullOrWhiteSpace(text))
                                list.Add($"H{(int)lvl}: {text}");
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return list;
        }

        private static List<Models.TableInfo> SafeTableInfo(Word.Document doc, int max)
        {
            var list = new List<Models.TableInfo>();
            try
            {
                int limit = Math.Min(doc.Tables.Count, max);
                for (int i = 1; i <= limit; i++)
                {
                    try
                    {
                        var t = doc.Tables[i];
                        list.Add(new Models.TableInfo { Rows = t.Rows.Count, Cols = t.Columns.Count });
                    }
                    catch { }
                }
            }
            catch { }
            return list;
        }

        private static string NormalizeText(string text, int max)
        {
            var v = (text ?? "")
                .Replace("\a", " ").Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Trim();
            while (v.Contains("  ")) v = v.Replace("  ", " ");
            return v.Length > max ? v.Substring(0, max) + "..." : v;
        }

        private static string Trim(string s, int max) =>
            s.Length > max ? s.Substring(0, max) + "\n...[truncated]" : s;
    }
}
