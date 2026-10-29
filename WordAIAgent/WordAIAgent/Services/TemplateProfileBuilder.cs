using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Office.Interop.Word;
using WordAIAgent.Models;
using Word = Microsoft.Office.Interop.Word;

namespace WordAIAgent.Services
{
    public class TemplateProfileBuilder
    {
        public TemplateProfileBuilder(WordService ws)
        {
            _ = ws;
        }

        public TemplateProfile Build(string path, int maxExcerptChars = 1200, int maxOutline = 30, int maxTables = 20, int maxStyles = 40)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь к шаблону пустой.");
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл шаблона не найден: " + path);

            Word.Document doc = null;
            Word.Document prevDoc = null;
            try
            {
                var app = Globals.ThisAddIn.Application;
                if (app.Documents.Count > 0)
                    prevDoc = app.ActiveDocument;

                doc = app.Documents.Open(path,
                    ReadOnly: true,
                    Visible: false,
                    AddToRecentFiles: false);

                var profile = new TemplateProfile
                {
                    SourcePath = path,
                    Pages = SafeStat(() => doc.ComputeStatistics(WdStatistic.wdStatisticPages)),
                    Outline = SafeOutline(doc, maxOutline),
                    Tables = SafeTables(doc, maxTables),
                    Styles = SafeStyles(doc, maxStyles),
                    Excerpt = SafeExcerpt(doc, maxExcerptChars)
                };

                return profile;
            }
            finally
            {
                try
                {
                    if (doc != null)
                        doc.Close(SaveChanges: false);
                }
                catch { }

                try { prevDoc?.Activate(); } catch { }
            }
        }

        private static int SafeStat(Func<int> getter)
        {
            try { return getter(); }
            catch { return 0; }
        }

        private static List<string> SafeOutline(Word.Document doc, int max)
        {
            var result = new List<string>();
            int count = 0;
            int limit = Math.Min(doc.Paragraphs.Count, 500);
            for (int i = 1; i <= limit && count < max; i++)
            {
                Word.Paragraph p = null;
                try
                {
                    p = doc.Paragraphs[i];
                    var lvl = p.OutlineLevel;
                    if (lvl >= WdOutlineLevel.wdOutlineLevel1 && lvl <= WdOutlineLevel.wdOutlineLevel9)
                    {
                        var text = NormalizeText(p.Range.Text, 160);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            result.Add($"H{(int)lvl}: {text}");
                            count++;
                        }
                    }
                }
                catch { }
                finally { if (p != null) try { System.Runtime.InteropServices.Marshal.ReleaseComObject(p); } catch { } }
            }
            return result;
        }

        private static List<TableInfo> SafeTables(Word.Document doc, int max)
        {
            var result = new List<TableInfo>();
            int count = Math.Min(doc.Tables.Count, max);
            for (int i = 1; i <= count; i++)
            {
                try
                {
                    var t = doc.Tables[i];
                    result.Add(new TableInfo { Rows = t.Rows.Count, Cols = t.Columns.Count });
                }
                catch { }
            }
            return result;
        }

        private static List<string> SafeStyles(Word.Document doc, int max)
        {
            var result = new List<string>();
            foreach (Word.Style s in doc.Styles)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(s.NameLocal))
                        result.Add(s.NameLocal);
                    if (result.Count >= max) break;
                }
                catch { }
            }
            return result;
        }

        private static string SafeExcerpt(Word.Document doc, int maxChars)
        {
            try
            {
                var text = NormalizeText(doc.Content.Text ?? "", maxChars);
                return text;
            }
            catch { return ""; }
        }

        private static string NormalizeText(string text, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var s = text.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            s = s.Trim();
            if (maxLen > 0 && s.Length > maxLen) s = s.Substring(0, maxLen);
            return s;
        }
    }
}
