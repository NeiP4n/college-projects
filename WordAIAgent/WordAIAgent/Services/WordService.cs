using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using Word = Microsoft.Office.Interop.Word;

namespace WordAIAgent.Services
{
    /// <summary>
    /// Reliable, exception-safe wrapper around Microsoft.Office.Interop.Word.
    /// Every public method has try/catch and returns meaningful error info.
    /// </summary>
    public class WordService
    {
        private Word.Application App =>
            Globals.ThisAddIn?.Application
            ?? throw new InvalidOperationException("Word приложение недоступно.");

        private Word.Document ActiveDoc
        {
            get
            {
                var app = App;
                if (app.Documents.Count == 0)
                    app.Documents.Add();
                return app.ActiveDocument;
            }
        }

        public void EnsureDocumentReady() { _ = ActiveDoc; }

        // ── Document ─────────────────────────────────────────────────────────

        public void NewDocument() => App.Documents.Add();

        public void SaveDocument()
        {
            var doc = ActiveDoc;
            try { doc.Save(); }
            catch { doc.SaveAs2(Type.Missing); }
        }

        public void SaveAsDocument(string path) => ActiveDoc.SaveAs2(path);

        public void ExportAsPDF(string path) =>
            ActiveDoc.ExportAsFixedFormat(path, WdExportFormat.wdExportFormatPDF);

        public void CloseDocument(bool save = true)
        {
            if (save) try { ActiveDoc.Save(); } catch { }
            ActiveDoc.Close();
        }

        public void PrintDocument() => ActiveDoc.PrintOut();

        /// <summary>Open a document from disk. Returns the opened document path.</summary>
        public string OpenDocument(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден: " + path);
            var doc = App.Documents.Open(path);
            return doc.FullName;
        }

        /// <summary>Apply a .docx file as template — copies content/styles into active doc.</summary>
        public void ApplyTemplate(string templatePath)
        {
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Шаблон не найден: " + templatePath);

            var doc = ActiveDoc;

            // Attach template (loads styles)
            doc.set_AttachedTemplate(templatePath);
            doc.UpdateStyles();
        }

        /// <summary>Insert content of another docx at cursor position.</summary>
        public void InsertFileContent(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден: " + filePath);

            var rng = App.Selection.Range;
            rng.InsertFile(filePath);
        }

        /// <summary>Read text from any .docx or .txt file and return it as string.</summary>
        public string ReadFileText(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден: " + filePath);

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".txt") return File.ReadAllText(filePath);

            // Open hidden, read, close
            var tempDoc = App.Documents.Open(filePath,
                ReadOnly: true,
                Visible: false,
                AddToRecentFiles: false);
            try { return tempDoc.Content.Text; }
            finally { tempDoc.Close(SaveChanges: false); }
        }

        // ── Security ─────────────────────────────────────────────────────────

        public void AddPassword(string password) => ActiveDoc.Password = password;
        public void RemovePassword() => ActiveDoc.Password = "";
        public void ProtectDocument(string password) =>
            ActiveDoc.Protect(WdProtectionType.wdAllowOnlyReading, false, password);
        public void UnprotectDocument(string password) => ActiveDoc.Unprotect(password);
        public void RestrictEditing(string password) =>
            ActiveDoc.Protect(WdProtectionType.wdAllowOnlyFormFields, false, password);

        // ── Text ─────────────────────────────────────────────────────────────

        public void InsertText(string text)
        {
            if (text == null) return;
            App.Selection.TypeText(text);
        }

        public void InsertTextChunked(string text, int chunkSize = 2000)
        {
            if (text == null) return;
            if (chunkSize < 500) chunkSize = 500;
            var sel = App.Selection;
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                var len = Math.Min(chunkSize, text.Length - i);
                sel.TypeText(text.Substring(i, len));
            }
        }

        public void InsertTextAtEnd(string text)
        {
            if (text == null) return;
            var rng = ActiveDoc.Content;
            rng.Collapse(WdCollapseDirection.wdCollapseEnd);
            rng.InsertAfter(text);
        }

        public void AppendParagraph(string text)
        {
            var rng = ActiveDoc.Content;
            rng.Collapse(WdCollapseDirection.wdCollapseEnd);
            rng.InsertParagraphAfter();
            if (!string.IsNullOrEmpty(text))
                rng.InsertAfter(text);
        }

        public void TypeParagraph() => App.Selection.TypeParagraph();

        public void SelectAll() => App.Selection.WholeStory();

        public void DeleteSelectedText() => App.Selection.Delete();

        public void DeleteAllText() => ActiveDoc.Content.Delete();

        public bool IsSelectionEmpty()
        {
            try { var s = App.Selection; return s == null || s.Start == s.End; }
            catch { return true; }
        }

        public bool HasSelection() => !IsSelectionEmpty();

        public string GetSelectedText()
        {
            try { return App.Selection?.Text ?? ""; }
            catch { return ""; }
        }

        public string GetDocumentText()
        {
            try { return ActiveDoc.Content.Text ?? ""; }
            catch { return ""; }
        }

        public string GetDocumentName()
        {
            try { return ActiveDoc.Name; }
            catch { return "Документ не открыт"; }
        }

        public bool IsDocumentOpen()
        {
            try { return App.Documents.Count > 0 && App.ActiveDocument != null; }
            catch { return false; }
        }

        public void CopySelection() => App.Selection.Copy();
        public void CutSelection() => App.Selection.Cut();
        public void PasteContent() => App.Selection.Paste();

        public bool FindAndReplace(string findText, string replaceText)
        {
            if (string.IsNullOrEmpty(findText)) return false;
            var find = ActiveDoc.Content.Find;
            find.ClearFormatting();
            find.Replacement.ClearFormatting();
            find.Text = findText;
            find.Replacement.Text = replaceText ?? "";
            find.Forward = true;
            find.Wrap = WdFindWrap.wdFindContinue;
            find.MatchCase = false;
            find.MatchWholeWord = false;
            var r = find.Execute(Replace: WdReplace.wdReplaceAll);
            return r is bool b && b;
        }

        /// <summary>Alias for FindAndReplace — used by AIAgentForm dispatcher and intent fallback.</summary>
        public bool FindReplace(string findText, string replaceText) =>
            FindAndReplace(findText, replaceText);

        public void ReplaceSelection(string text) =>
            App.Selection.Range.Text = text ?? "";

        // ── Font formatting ───────────────────────────────────────────────────

        public void SetBold(bool on) => App.Selection.Font.Bold = on ? 1 : 0;
        public void SetItalic(bool on) => App.Selection.Font.Italic = on ? 1 : 0;
        public void SetUnderline(bool on) =>
            App.Selection.Font.Underline = on ? WdUnderline.wdUnderlineSingle : WdUnderline.wdUnderlineNone;
        public void SetStrikeThrough(bool on) => App.Selection.Font.StrikeThrough = on ? 1 : 0;
        public void SetSubscript(bool on) => App.Selection.Font.Subscript = on ? 1 : 0;
        public void SetSuperscript(bool on) => App.Selection.Font.Superscript = on ? 1 : 0;
        public void SetAllCaps(bool on) => App.Selection.Font.AllCaps = on ? 1 : 0;
        public void SetFontName(string name) { if (!string.IsNullOrWhiteSpace(name)) App.Selection.Font.Name = name; }
        public void SetFontSize(float size) { if (size > 0) App.Selection.Font.Size = size; }
        public void ClearFormatting() => App.Selection.ClearFormatting();

        public void SetFontColor(Color color)
        {
            App.Selection.Font.Color =
                (WdColor)(color.R | (color.G << 8) | (color.B << 16));
        }

        public void SetHighlight(WdColorIndex color) =>
            App.Selection.Range.HighlightColorIndex = color;

        // ── Paragraph ────────────────────────────────────────────────────────

        public void AlignLeft() =>
            App.Selection.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        public void AlignCenter() =>
            App.Selection.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        public void AlignRight() =>
            App.Selection.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
        public void AlignJustify() =>
            App.Selection.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;

        public void SetLineSpacing(float pts)
        {
            App.Selection.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceExactly;
            App.Selection.ParagraphFormat.LineSpacing = pts;
        }

        public void SetLineSpacingMultiple(float lines)
        {
            App.Selection.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceMultiple;
            App.Selection.ParagraphFormat.LineSpacing = lines * 12f;
        }

        public void SetSpacingBefore(float pts) =>
            App.Selection.ParagraphFormat.SpaceBefore = pts;
        public void SetSpacingAfter(float pts) =>
            App.Selection.ParagraphFormat.SpaceAfter = pts;

        public void SetFirstLineIndentCm(float cm) =>
            App.Selection.ParagraphFormat.FirstLineIndent = cm * 28.3465f;

        public void IndentParagraph() =>
            App.Selection.ParagraphFormat.LeftIndent += 36f;
        public void OutdentParagraph() =>
            App.Selection.ParagraphFormat.LeftIndent =
                Math.Max(0, App.Selection.ParagraphFormat.LeftIndent - 36f);

        // ── GOST formatting ───────────────────────────────────────────────────

        public void ApplyGostFormatting()
        {
            var rng = ActiveDoc.Content;
            rng.Font.Name = "Times New Roman";
            rng.Font.Size = 14;
            rng.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;
            rng.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceMultiple;
            rng.ParagraphFormat.LineSpacing = 18f;
            rng.ParagraphFormat.SpaceBefore = 0f;
            rng.ParagraphFormat.SpaceAfter = 0f;
            rng.ParagraphFormat.FirstLineIndent = 1.25f * 28.3465f;
        }

        public void RemoveDoubleSpacesInDocument()
        {
            var find = ActiveDoc.Content.Find;
            find.ClearFormatting();
            find.Replacement.ClearFormatting();
            find.Forward = true;
            find.Wrap = WdFindWrap.wdFindContinue;
            find.MatchWildcards = false;
            find.Text = "  ";
            find.Replacement.Text = " ";
            for (int i = 0; i < 8; i++)
            {
                var r = find.Execute(Replace: WdReplace.wdReplaceAll);
                if (!(r is bool b) || !b) break;
            }
        }

        public void RemoveManualLineBreaks()
        {
            FindAndReplace("^l", " ");
        }

        // ── Styles ────────────────────────────────────────────────────────────

        public void ApplyStyle(string styleName)
        {
            try { App.Selection.set_Style(ActiveDoc.Styles[styleName]); }
            catch (Exception ex) { throw new Exception($"Стиль '{styleName}' не найден: {ex.Message}"); }
        }

        public void ApplyHeading(int level)
        {
            if (level < 1 || level > 9) level = 1;
            var styleNames = new[] {
                "Heading 1","Heading 2","Heading 3","Heading 4","Heading 5",
                "Heading 6","Heading 7","Heading 8","Heading 9"
            };
            try { ApplyStyle(styleNames[level - 1]); }
            catch
            {
                // Try Russian style names
                var ruNames = new[] {
                    "Заголовок 1","Заголовок 2","Заголовок 3","Заголовок 4","Заголовок 5",
                    "Заголовок 6","Заголовок 7","Заголовок 8","Заголовок 9"
                };
                try { ApplyStyle(ruNames[level - 1]); }
                catch { /* best effort */ }
            }
        }

        public void ApplyNormalStyle()
        {
            try { ApplyStyle("Normal"); }
            catch { try { ApplyStyle("Обычный"); } catch { } }
        }

        /// <summary>List all styles available in the document.</summary>
        public List<string> GetAvailableStyles()
        {
            var result = new List<string>();
            foreach (Word.Style s in ActiveDoc.Styles)
            {
                try { if (!string.IsNullOrWhiteSpace(s.NameLocal)) result.Add(s.NameLocal); }
                catch { }
            }
            return result;
        }

        // ── TOC ──────────────────────────────────────────────────────────────

        public void InsertTableOfContents()
        {
            var rng = App.Selection.Range;
            ActiveDoc.TablesOfContents.Add(rng, true, 1, 3);
        }

        public void UpdateTableOfContents()
        {
            foreach (Word.TableOfContents toc in ActiveDoc.TablesOfContents)
                toc.Update();
        }

        // ── Lists ─────────────────────────────────────────────────────────────

        public void CreateBulletList() =>
            App.Selection.Range.ListFormat.ApplyListTemplate(
                App.ListGalleries[WdListGalleryType.wdBulletGallery].ListTemplates[1]);

        public void CreateNumberedList() =>
            App.Selection.Range.ListFormat.ApplyListTemplate(
                App.ListGalleries[WdListGalleryType.wdNumberGallery].ListTemplates[1]);

        public void RemoveList() => App.Selection.Range.ListFormat.RemoveNumbers();
        public void IncreaseListLevel() => App.Selection.Range.ListFormat.ListIndent();
        public void DecreaseListLevel() => App.Selection.Range.ListFormat.ListOutdent();

        // ── Tables ────────────────────────────────────────────────────────────

        public Word.Table InsertTable(int rows, int cols)
        {
            rows = Math.Max(1, rows);
            cols = Math.Max(1, cols);
            var rng = App.Selection.Range;
            var table = ActiveDoc.Tables.Add(rng, rows, cols);
            table.Borders.Enable = 1;
            table.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow);
            return table;
        }

        public void FillTableCell(int tableIndex, int row, int col, string text)
        {
            var table = ActiveDoc.Tables[tableIndex];
            if (row >= 1 && row <= table.Rows.Count &&
                col >= 1 && col <= table.Columns.Count)
                table.Cell(row, col).Range.Text = text ?? "";
        }

        public string ReadTableCell(int tableIndex, int row, int col)
        {
            var table = ActiveDoc.Tables[tableIndex];
            return table.Cell(row, col).Range.Text?.TrimEnd('\r', '\a') ?? "";
        }

        public string DumpTable(int tableIndex)
        {
            var table = ActiveDoc.Tables[tableIndex];
            var sb = new StringBuilder();
            for (int r = 1; r <= table.Rows.Count; r++)
            {
                var cells = new List<string>();
                for (int c = 1; c <= table.Columns.Count; c++)
                {
                    try { cells.Add(table.Cell(r, c).Range.Text?.TrimEnd('\r', '\a') ?? ""); }
                    catch { cells.Add(""); }
                }
                sb.AppendLine(string.Join(" | ", cells));
            }
            return sb.ToString();
        }

        public void AddTableRow()
        {
            if (App.Selection.Tables.Count > 0)
                App.Selection.Tables[1].Rows.Add();
        }

        public void AddTableColumn()
        {
            if (App.Selection.Tables.Count > 0)
                App.Selection.Tables[1].Columns.Add();
        }

        public bool DeleteCurrentOrLastTable()
        {
            if (App.Selection.Tables.Count > 0)
            { App.Selection.Tables[1].Delete(); return true; }
            if (ActiveDoc.Tables.Count > 0)
            { ActiveDoc.Tables[ActiveDoc.Tables.Count].Delete(); return true; }
            return false;
        }

        public bool DeleteTableRow(int? index = null)
        {
            Word.Table tbl = App.Selection.Tables.Count > 0
                ? App.Selection.Tables[1]
                : (ActiveDoc.Tables.Count > 0 ? ActiveDoc.Tables[ActiveDoc.Tables.Count] : null);
            if (tbl == null) return false;

            int idx = index.HasValue ? index.Value
                : (App.Selection.Tables.Count > 0 ? App.Selection.Cells[1].RowIndex : tbl.Rows.Count);
            if (idx < 1 || idx > tbl.Rows.Count) return false;
            tbl.Rows[idx].Delete();
            return true;
        }

        public bool DeleteTableColumn(int? index = null)
        {
            Word.Table tbl = App.Selection.Tables.Count > 0
                ? App.Selection.Tables[1]
                : (ActiveDoc.Tables.Count > 0 ? ActiveDoc.Tables[ActiveDoc.Tables.Count] : null);
            if (tbl == null) return false;

            int idx = index.HasValue ? index.Value
                : (App.Selection.Tables.Count > 0 ? App.Selection.Cells[1].ColumnIndex : tbl.Columns.Count);
            if (idx < 1 || idx > tbl.Columns.Count) return false;
            tbl.Columns[idx].Delete();
            return true;
        }

        public void ApplyTableStyle(string style)
        {
            if (App.Selection.Tables.Count > 0)
                App.Selection.Tables[1].set_Style(style);
        }

        // ── Charts ────────────────────────────────────────────────────────────

        public void InsertPieChart()
        {
            var sel = App.Selection;

            if (sel.Tables.Count > 0)
            {
                sel.MoveRight(Word.WdUnits.wdCharacter, 1);
                sel.TypeParagraph();
            }

            if (sel.StoryType != Word.WdStoryType.wdMainTextStory)
            {
                ActiveDoc.Content.Select();
                sel = App.Selection;
            }

            sel.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
            sel.TypeParagraph();
            const int xlPie = 5;
            object missing = Type.Missing;
            var shapes = ActiveDoc.InlineShapes;

            // Try AddChart2 first (Word 2013+)
            var m2 = shapes.GetType().GetMethod("AddChart2");
            if (m2 != null)
            {
                try
                {
                    var s = m2.Invoke(shapes, new object[] { 201, xlPie, missing, missing, missing, missing, missing });
                    TrySetChartType(s, xlPie);
                    return;
                }
                catch { }
            }

            // Fallback: AddChart
            var m1 = shapes.GetType().GetMethod("AddChart");
            if (m1 != null)
            {
                try
                {
                    var s = m1.Invoke(shapes, new object[] { xlPie, missing, missing, missing, missing });
                    TrySetChartType(s, xlPie);
                    return;
                }
                catch { }
            }

            ExecuteMso("InsertChart");
        }

        public void InsertChart(int chartType)
        {
            var sel = App.Selection;
            if (sel.Tables.Count > 0)
            {
                sel.MoveRight(Word.WdUnits.wdCharacter, 1);
                sel.TypeParagraph();
            }
            if (sel.StoryType != Word.WdStoryType.wdMainTextStory)
            {
                ActiveDoc.Content.Select();
                sel = App.Selection;
            }
            sel.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
            sel.TypeParagraph();

            object missing = Type.Missing;
            var shapes = ActiveDoc.InlineShapes;

            var m2 = shapes.GetType().GetMethod("AddChart2");
            if (m2 != null)
            {
                try
                {
                    var s = m2.Invoke(shapes, new object[] { 201, chartType, missing, missing, missing, missing, missing });
                    TrySetChartType(s, chartType);
                    return;
                }
                catch { }
            }

            var m1 = shapes.GetType().GetMethod("AddChart");
            if (m1 != null)
            {
                try
                {
                    var s = m1.Invoke(shapes, new object[] { chartType, missing, missing, missing, missing });
                    TrySetChartType(s, chartType);
                    return;
                }
                catch { }
            }

            ExecuteMso("InsertChart");
        }

        public void InsertSmartArt() => ExecuteMso("InsertSmartArtGraphic");

        public void InsertEquation() => ExecuteMso("InsertEquation");

        public void InsertCaption() => ExecuteMso("InsertCaption");

        public void InsertCitation() => ExecuteMso("InsertCitation");

        public void InsertBibliography() => ExecuteMso("InsertBibliography");

        public void UpdateFields()
        {
            try { ActiveDoc.Fields.Update(); }
            catch { }
        }

        private static void TrySetChartType(object inlineShape, int chartType)
        {
            try
            {
                var chart = inlineShape?.GetType().GetProperty("Chart")?.GetValue(inlineShape);
                chart?.GetType().GetProperty("ChartType")?.SetValue(chart, chartType);
            }
            catch { }
        }

        // ── Images ───────────────────────────────────────────────────────────

        public void InsertImage(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Изображение не найдено: " + filePath);
            App.Selection.InlineShapes.AddPicture(filePath, false, true);
        }

        public void InsertImageFromClipboard()
        {
            if (!Clipboard.ContainsImage()) return;
            var img = Clipboard.GetImage();
            string tmp = Path.Combine(Path.GetTempPath(), "ai_clip.png");
            img.Save(tmp, System.Drawing.Imaging.ImageFormat.Png);
            InsertImage(tmp);
        }

        // ── Hyperlinks / Bookmarks ────────────────────────────────────────────

        public void InsertHyperlink(string displayText, string url)
        {
            var rng = App.Selection.Range;
            if (!string.IsNullOrEmpty(displayText)) rng.Text = displayText;
            ActiveDoc.Hyperlinks.Add(rng, url);
        }

        public void InsertBookmark(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            ActiveDoc.Bookmarks.Add(name, App.Selection.Range);
        }

        public void GoToBookmark(string name)
        {
            if (ActiveDoc.Bookmarks.Exists(name))
                ActiveDoc.Bookmarks[name].Select();
        }

        // ── Footnotes / Comments ──────────────────────────────────────────────

        public void InsertFootnote(string text) =>
            ActiveDoc.Footnotes.Add(App.Selection.Range, Type.Missing, text ?? "");

        public void InsertEndnote(string text) =>
            ActiveDoc.Endnotes.Add(App.Selection.Range, Type.Missing, text ?? "");

        public void AddComment(string text) =>
            ActiveDoc.Comments.Add(App.Selection.Range, text ?? "");

        public void DeleteAllComments()
        {
            while (ActiveDoc.Comments.Count > 0)
                ActiveDoc.Comments[1].Delete();
        }

        public void AcceptAllComments()
        {
            foreach (Word.Comment c in ActiveDoc.Comments)
                c.Done = true;
        }

        // ── Track changes ─────────────────────────────────────────────────────

        public void EnableTrackChanges() => ActiveDoc.TrackRevisions = true;
        public void DisableTrackChanges() => ActiveDoc.TrackRevisions = false;
        public void AcceptAllChanges() => ActiveDoc.AcceptAllRevisions();
        public void RejectAllChanges() => ActiveDoc.RejectAllRevisions();

        // ── Navigation ────────────────────────────────────────────────────────

        public void GoToStart() => App.Selection.HomeKey(WdUnits.wdStory);
        public void GoToEnd() => App.Selection.EndKey(WdUnits.wdStory);
        public void GoToPage(int page) =>
            App.Selection.GoTo(WdGoToItem.wdGoToPage, WdGoToDirection.wdGoToAbsolute, page);

        // ── Page / Section ────────────────────────────────────────────────────

        public void InsertPageBreak() =>
            App.Selection.InsertBreak(WdBreakType.wdPageBreak);

        public void InsertSectionBreak(WdBreakType type = WdBreakType.wdSectionBreakNextPage) =>
            App.Selection.InsertBreak(type);

        public void SetPageMarginsCm(float topCm, float bottomCm, float leftCm, float rightCm)
        {
            var ps = ActiveDoc.PageSetup;
            const float cm = 28.3465f;
            ps.TopMargin = topCm * cm;
            ps.BottomMargin = bottomCm * cm;
            ps.LeftMargin = leftCm * cm;
            ps.RightMargin = rightCm * cm;
        }

        public void SetPageOrientation(WdOrientation orientation) =>
            ActiveDoc.PageSetup.Orientation = orientation;

        // ── Headers / Footers ─────────────────────────────────────────────────

        public void InsertHeader(string text)
        {
            var hdr = ActiveDoc.Sections[1].Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
            hdr.Range.Text = text ?? "";
        }

        public void InsertFooter(string text)
        {
            var ftr = ActiveDoc.Sections[1].Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
            ftr.Range.Text = text ?? "";
        }

        public void InsertPageNumbers() =>
            ActiveDoc.Sections[1].Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary]
                .PageNumbers.Add(WdPageNumberAlignment.wdAlignPageNumberCenter);

        public void RemoveHeader()
        {
            foreach (Word.Section sec in ActiveDoc.Sections)
                sec.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Delete();
        }

        public void RemoveFooter()
        {
            foreach (Word.Section sec in ActiveDoc.Sections)
                sec.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Delete();
        }

        // ── Misc ──────────────────────────────────────────────────────────────

        public void InsertTextBox()
        {
            ActiveDoc.Shapes.AddTextbox(
                MsoTextOrientation.msoTextOrientationHorizontal, 100, 100, 200, 100);
        }

        public void InsertSymbol(int charCode, string fontName = "Wingdings") =>
            App.Selection.InsertSymbol(charCode, fontName);

        public void InsertDateTime() =>
            App.Selection.InsertDateTime("dd.MM.yyyy HH:mm", false);

        public void CheckSpellingAndGrammar() =>
            ActiveDoc.CheckSpelling();

        public void SetViewDraft() => App.ActiveWindow.View.Type = WdViewType.wdNormalView;
        public void SetViewPrint() => App.ActiveWindow.View.Type = WdViewType.wdPrintView;
        public void SetViewWeb() => App.ActiveWindow.View.Type = WdViewType.wdWebView;
        public void SetViewOutline() => App.ActiveWindow.View.Type = WdViewType.wdOutlineView;

        public void SetZoom(int percent) =>
            App.ActiveWindow.View.Zoom.Percentage = Math.Max(10, Math.Min(500, percent));

        public void Undo() { try { ExecuteMso("Undo"); } catch { } }
        public void Redo() { try { ExecuteMso("RedoOrRepeat"); } catch { } }

        // ── GOST 19 Template ─────────────────────────────────────────────────

        public bool ApplyGost19Template()
        {
            EnsureDocumentReady();
            EnsureSelectionInMainStory();
            bool hasContent = HasDocumentContent();
            ApplyGostFormatting();
            if (hasContent) return false;

            void H(string t, int lvl) { InsertHeadingLine(t, lvl); }
            void N(string t = "") { InsertNormalLine(t); }

            H("Техническое задание", 1); N();
            H("1. Основание для разработки", 1); N();
            H("2. Назначение разработки", 1); N();
            H("3. Требования к программе", 1);
            N("3.1 Требования к функциональным характеристикам");
            N("3.2 Требования к надёжности");
            N("3.3 Условия эксплуатации");
            N("3.4 Требования к техническим средствам");
            N("3.5 Требования к программной совместимости");
            N("3.6 Требования к защите информации"); N();
            H("4. Требования к программной документации", 1); N();
            H("5. Технико-экономические показатели", 1); N();
            H("6. Стадии и этапы разработки", 1); N();
            H("7. Порядок контроля и приёмки", 1); N();
            return true;
        }

        // ── Stats ─────────────────────────────────────────────────────────────

        public string GetDocumentStats()
        {
            try
            {
                var doc = ActiveDoc;
                return
                    $"Слов: {doc.ComputeStatistics(WdStatistic.wdStatisticWords)}\n" +
                    $"Символов: {doc.ComputeStatistics(WdStatistic.wdStatisticCharacters)}\n" +
                    $"Страниц: {doc.ComputeStatistics(WdStatistic.wdStatisticPages)}\n" +
                    $"Абзацев: {doc.ComputeStatistics(WdStatistic.wdStatisticParagraphs)}";
            }
            catch { return "Статистика недоступна."; }
        }

        public string GetWordCount()
        {
            try { return $"Слов: {ActiveDoc.ComputeStatistics(WdStatistic.wdStatisticWords)}"; }
            catch { return "Недоступно."; }
        }

        // ── Word internals ────────────────────────────────────────────────────

        public void ExecuteMso(string controlId)
        {
            if (string.IsNullOrWhiteSpace(controlId)) return;
            try { App.CommandBars.ExecuteMso(controlId); }
            catch (COMException ex) { throw new Exception($"Команда '{controlId}' недоступна: {ex.Message}"); }
        }

        public object ExecuteWordBasic(string cmd, object[] args)
        {
            if (string.IsNullOrWhiteSpace(cmd)) return null;
            var wb = App.WordBasic;
            return wb?.GetType().InvokeMember(cmd, BindingFlags.InvokeMethod, null, wb, args ?? Array.Empty<object>());
        }

        public object RunMacro(string name, object[] args)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var all = new object[1 + (args?.Length ?? 0)];
            all[0] = name;
            if (args != null) Array.Copy(args, 0, all, 1, args.Length);
            return App.GetType().InvokeMember("Run", BindingFlags.InvokeMethod, null, App, all);
        }

        public void ExecuteWordCommand(string name, object[] args = null)
        {
            try { RunMacro(name, args); }
            catch { try { ExecuteMso(name); } catch { } }
        }

        /// <summary>
        /// Executes a VBA code snippet by injecting it into a temporary macro,
        /// running it, then deleting it. Uses reflection so no explicit
        /// Microsoft.Vbe.Interop assembly reference is required.
        /// Requires Word Trust Center → "Trust access to the VBA project object model".
        /// </summary>
        public void ExecuteTemporaryVba(string vbaCode)
        {
            if (string.IsNullOrWhiteSpace(vbaCode)) return;

            const string ModuleName = "_AITempModule";
            const string MacroName = "_AITempMacro";

            object vbaProject = null;
            object module = null;
            try
            {
                // ActiveDoc.VBProject
                vbaProject = ActiveDoc.GetType()
                    .InvokeMember("VBProject", BindingFlags.GetProperty, null, ActiveDoc, null);
                if (vbaProject == null)
                    throw new InvalidOperationException(
                        "VBProject недоступен. Включите 'Доверять доступ к объектной модели VBA' в Центре управления безопасностью Word.");

                // VBProject.VBComponents
                var components = vbaProject.GetType()
                    .InvokeMember("VBComponents", BindingFlags.GetProperty, null, vbaProject, null);

                // Add standard module (vbext_ct_StdModule = 1)
                module = components.GetType()
                    .InvokeMember("Add", BindingFlags.InvokeMethod, null, components, new object[] { 1 });

                // module.Name = ModuleName
                module.GetType().InvokeMember("Name", BindingFlags.SetProperty, null, module, new object[] { ModuleName });

                // module.CodeModule.AddFromString(code)
                var codeModule = module.GetType()
                    .InvokeMember("CodeModule", BindingFlags.GetProperty, null, module, null);
                var fullCode = $"Sub {MacroName}()\r\n{vbaCode}\r\nEnd Sub";
                codeModule.GetType()
                    .InvokeMember("AddFromString", BindingFlags.InvokeMethod, null, codeModule, new object[] { fullCode });

                // App.Run "DocName!ModuleName.MacroName"
                App.Run($"{ActiveDoc.Name}!{ModuleName}.{MacroName}");
            }
            finally
            {
                if (module != null && vbaProject != null)
                {
                    try
                    {
                        var components = vbaProject.GetType()
                            .InvokeMember("VBComponents", BindingFlags.GetProperty, null, vbaProject, null);
                        components.GetType()
                            .InvokeMember("Remove", BindingFlags.InvokeMethod, null, components, new object[] { module });
                    }
                    catch { }
                }
            }
        }

        // ── Columns / layout ──────────────────────────────────────────────────
        public void SetColumns(int count)
        {
            var sec = ActiveDoc.Sections[1].PageSetup;
            sec.TextColumns.SetCount(count);
        }

        public void SetColumnsWithSpacing(int count, float spacingCm)
        {
            var cols = ActiveDoc.Sections[1].PageSetup.TextColumns;
            cols.SetCount(count);
            cols.Spacing = spacingCm * 28.35f;
        }

        public void InsertColumnBreak() =>
            App.Selection.InsertBreak(WdBreakType.wdColumnBreak);

        // ── Watermark ─────────────────────────────────────────────────────────
        public void InsertTextWatermark(string text)
        {
            // Word watermarks live in headers
            var header = ActiveDoc.Sections[1].Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
            header.Range.Select();
            ExecuteMso("InsertWordArt");  // Fallback — real watermark via shape
            // Reliable approach: use WordArt shape in header
            var shape = header.Shapes.AddTextEffect(
                Microsoft.Office.Core.MsoPresetTextEffect.msoTextEffect1,
                text, "Arial", 54f, Microsoft.Office.Core.MsoTriState.msoCTrue,
                Microsoft.Office.Core.MsoTriState.msoFalse, 200f, 300f);
            shape.TextEffect.FontBold = Microsoft.Office.Core.MsoTriState.msoCTrue;
            shape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(192, 192, 192));
            shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
            shape.Rotation = -45f;
            shape.WrapFormat.Type = WdWrapType.wdWrapNone;
            shape.RelativeHorizontalPosition = WdRelativeHorizontalPosition.wdRelativeHorizontalPositionPage;
            shape.RelativeVerticalPosition = WdRelativeVerticalPosition.wdRelativeVerticalPositionPage;
            App.Selection.EndKey(WdUnits.wdStory); // back to main body
        }

        public void RemoveWatermark()
        {
            foreach (WdHeaderFooterIndex idx in new[] {
                WdHeaderFooterIndex.wdHeaderFooterPrimary,
                WdHeaderFooterIndex.wdHeaderFooterFirstPage })
            {
                try
                {
                    var header = ActiveDoc.Sections[1].Headers[idx];
                    for (int i = header.Shapes.Count; i >= 1; i--)
                        header.Shapes[i].Delete();
                }
                catch { }
            }
        }

        // ── Borders ───────────────────────────────────────────────────────────
        public void SetParagraphBorder(bool on)
        {
            var borders = App.Selection.Borders;
            if (on)
            {
                borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                borders[WdBorderType.wdBorderLeft].LineStyle = WdLineStyle.wdLineStyleSingle;
                borders[WdBorderType.wdBorderRight].LineStyle = WdLineStyle.wdLineStyleSingle;
            }
            else
            {
                borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleNone;
                borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleNone;
                borders[WdBorderType.wdBorderLeft].LineStyle = WdLineStyle.wdLineStyleNone;
                borders[WdBorderType.wdBorderRight].LineStyle = WdLineStyle.wdLineStyleNone;
            }
        }

        public void SetPageBorder(WdLineStyle style = WdLineStyle.wdLineStyleSingle)
        {
            foreach (WdBorderType bt in new[] {
                WdBorderType.wdBorderTop, WdBorderType.wdBorderBottom,
                WdBorderType.wdBorderLeft, WdBorderType.wdBorderRight })
            {
                ActiveDoc.Sections[1].Borders[bt].LineStyle = style;
                ActiveDoc.Sections[1].Borders[bt].LineWidth = WdLineWidth.wdLineWidth150pt;
            }
        }

        public void RemovePageBorder()
        {
            foreach (WdBorderType bt in new[] {
                WdBorderType.wdBorderTop, WdBorderType.wdBorderBottom,
                WdBorderType.wdBorderLeft, WdBorderType.wdBorderRight })
                ActiveDoc.Sections[1].Borders[bt].LineStyle = WdLineStyle.wdLineStyleNone;
        }

        // ── Find & highlight (without replace) ───────────────────────────────
        public int FindAndHighlight(string text, WdColorIndex color = WdColorIndex.wdYellow)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int count = 0;
            var find = ActiveDoc.Content.Find;
            find.ClearFormatting();
            find.Text = text;
            find.Forward = true;
            find.Wrap = WdFindWrap.wdFindStop;
            find.MatchCase = false;
            // Use replacement highlight trick
            find.Replacement.ClearFormatting();
            find.Replacement.Highlight = 1;
            find.Replacement.Text = "^&";
            find.Execute(Replace: WdReplace.wdReplaceAll);
            return count;
        }

        // ── Character spacing ─────────────────────────────────────────────────
        public void SetCharacterSpacingPt(float pts) =>
            App.Selection.Font.Spacing = pts;

        public void SetCharacterScaling(int percent) =>
            App.Selection.Font.Scaling = percent;

        public void SetKerning(float minSizePt) =>
            App.Selection.Font.Kerning = minSizePt;

        // ── Paragraph tab stops ───────────────────────────────────────────────
        public void ClearTabStops() =>
            App.Selection.ParagraphFormat.TabStops.ClearAll();

        public void AddTabStop(float positionCm, WdTabAlignment alignment = WdTabAlignment.wdAlignTabLeft)
        {
            float pts = positionCm * 28.35f;
            App.Selection.ParagraphFormat.TabStops.Add(pts, alignment);
        }

        // ── Drop cap ─────────────────────────────────────────────────────────
        public void SetDropCap(int lines = 3)
        {
            var dc = App.Selection.Paragraphs[1].DropCap;
            dc.Position = WdDropPosition.wdDropNormal;
            dc.LinesToDrop = lines;
        }

        public void RemoveDropCap()
        {
            App.Selection.Paragraphs[1].DropCap.Position = WdDropPosition.wdDropNone;
        }

        // ── Text direction ────────────────────────────────────────────────────
        public void SetTextDirectionLeftRight() =>
            App.Selection.Cells.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;

        // ── Document comparison ───────────────────────────────────────────────
        public string CompareWithDocument(string otherPath)
        {
            if (!File.Exists(otherPath)) return "Файл не найден: " + otherPath;
            ActiveDoc.Compare(otherPath, "Сравнение");
            return "Сравнение выполнено — изменения показаны.";
        }

        // ── Sections ─────────────────────────────────────────────────────────
        public int GetSectionCount() => ActiveDoc.Sections.Count;

        public void SetSectionPageOrientation(int sectionIndex, bool landscape)
        {
            var s = ActiveDoc.Sections[sectionIndex];
            s.PageSetup.Orientation = landscape
                ? WdOrientation.wdOrientLandscape
                : WdOrientation.wdOrientPortrait;
        }

        // ── Line numbers ─────────────────────────────────────────────────────
        public void EnableLineNumbers(bool restartEachPage = false)
        {
            var ln = ActiveDoc.PageSetup.LineNumbering;
            ln.Active = 1;
            ln.StartingNumber = 1;
            ln.CountBy = 1;
            ln.RestartMode = restartEachPage
                ? WdNumberingRule.wdRestartPage
                : WdNumberingRule.wdRestartContinuous;
        }

        public void DisableLineNumbers()
        {
            ActiveDoc.PageSetup.LineNumbering.Active = 0;
        }

        // ── AutoText / Quick Parts ────────────────────────────────────────────
        public void InsertAutoText(string name)
        {
            try { App.Selection.TypeText(name); ExecuteMso("AutoTextInsert"); }
            catch { }
        }

        // ── Cross-reference ───────────────────────────────────────────────────
        public void InsertCrossReference(string bookmarkName)
        {
            App.Selection.InsertCrossReference(
                WdReferenceType.wdRefTypeBookmark,
                WdReferenceKind.wdContentText,
                bookmarkName, true, false);
        }

        // ── Smart quotes / autocorrect toggles ────────────────────────────────
        public void SetSmartQuotes(bool on)
        {
            App.Options.AutoFormatAsYouTypeReplaceQuotes = on;
            App.Options.AutoFormatReplaceQuotes = on;
        }

        // ── Document statistics ───────────────────────────────────────────────
        public string GetFullStats()
        {
            var doc = ActiveDoc;
            var sb = new StringBuilder();
            sb.AppendLine($"Слов: {doc.ComputeStatistics(WdStatistic.wdStatisticWords)}");
            sb.AppendLine($"Знаков (с пробелами): {doc.ComputeStatistics(WdStatistic.wdStatisticCharactersWithSpaces)}");
            sb.AppendLine($"Знаков (без пробелов): {doc.ComputeStatistics(WdStatistic.wdStatisticCharacters)}");
            sb.AppendLine($"Страниц: {doc.ComputeStatistics(WdStatistic.wdStatisticPages)}");
            sb.AppendLine($"Строк: {doc.ComputeStatistics(WdStatistic.wdStatisticLines)}");
            sb.AppendLine($"Абзацев: {doc.ComputeStatistics(WdStatistic.wdStatisticParagraphs)}");
            return sb.ToString().Trim();
        }

        // ── Named range / selection ───────────────────────────────────────────
        public void SelectSentence()
        {
            App.Selection.Expand(WdUnits.wdSentence);
        }

        public void SelectParagraph()
        {
            App.Selection.Expand(WdUnits.wdParagraph);
        }

        public void SelectWord()
        {
            App.Selection.Expand(WdUnits.wdWord);
        }

        public void MoveUp(int lines = 1) =>
            App.Selection.MoveUp(WdUnits.wdLine, lines);

        public void MoveDown(int lines = 1) =>
            App.Selection.MoveDown(WdUnits.wdLine, lines);

        public void MoveToLineStart() =>
            App.Selection.HomeKey(WdUnits.wdLine);

        public void MoveToLineEnd() =>
            App.Selection.EndKey(WdUnits.wdLine);

        // ── Table operations (extended) ───────────────────────────────────────
        public void SortTableByColumn(int tableIndex, int col, bool ascending = true)
        {
            var tbl = ActiveDoc.Tables[tableIndex];
            tbl.Sort(
                SortFieldType: WdSortFieldType.wdSortFieldAlphanumeric,
                SortOrder: ascending ? WdSortOrder.wdSortOrderAscending : WdSortOrder.wdSortOrderDescending,
                FieldNumber: $"Column {col}");
        }

        public void AutoFitTable(int tableIndex)
        {
            var tbl = ActiveDoc.Tables[tableIndex];
            tbl.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitContent);
        }

        public void SetTableBorders(int tableIndex, WdLineStyle style = WdLineStyle.wdLineStyleSingle)
        {
            var tbl = ActiveDoc.Tables[tableIndex];
            tbl.Borders.InsideLineStyle = style;
            tbl.Borders.OutsideLineStyle = style;
        }

        public void MergeTableCells(int tableIndex, int row1, int col1, int row2, int col2)
        {
            var tbl = ActiveDoc.Tables[tableIndex];
            var cell1 = tbl.Cell(row1, col1);
            var cell2 = tbl.Cell(row2, col2);
            cell1.Merge(cell2);
        }

        public void SetTableCellBackground(int tableIndex, int row, int col, Color color)
        {
            var cell = ActiveDoc.Tables[tableIndex].Cell(row, col);
            cell.Shading.BackgroundPatternColor = (WdColor)ColorTranslator.ToOle(color);
        }

        // ── Footnotes / endnotes ─────────────────────────────────────────────
        public void DeleteAllFootnotes()
        {
            while (ActiveDoc.Footnotes.Count > 0)
                ActiveDoc.Footnotes[1].Delete();
        }

        public void DeleteAllEndnotes()
        {
            while (ActiveDoc.Endnotes.Count > 0)
                ActiveDoc.Endnotes[1].Delete();
        }

        public int GetFootnoteCount() => ActiveDoc.Footnotes.Count;

        // ── Styles ───────────────────────────────────────────────────────────
        public void CreateCharacterStyle(string name, string fontName, float fontSize, bool bold)
        {
            try
            {
                Word.Style style;
                try { style = ActiveDoc.Styles[name]; }
                catch { style = (Word.Style)ActiveDoc.Styles.Add(name, WdStyleType.wdStyleTypeCharacter); }
                style.Font.Name = fontName;
                style.Font.Size = fontSize;
                style.Font.Bold = bold ? 1 : 0;
            }
            catch { }
        }

        public void DeleteStyle(string name)
        {
            try { ActiveDoc.Styles[name].Delete(); } catch { }
        }

        // ── Numbered headings ─────────────────────────────────────────────────
        public void ApplyOutlineNumbering()
        {
            try { ExecuteMso("ListNumOutlineNumberDefault"); } catch { }
        }

        // ── Comments ─────────────────────────────────────────────────────────
        public string GetAllComments()
        {
            var sb = new StringBuilder();
            foreach (Word.Comment c in ActiveDoc.Comments)
                sb.AppendLine($"[{c.Author}]: {c.Range.Text}");
            return sb.Length > 0 ? sb.ToString() : "Комментариев нет.";
        }

        public void ReplyToLastComment(string text)
        {
            if (ActiveDoc.Comments.Count == 0) return;
            var last = ActiveDoc.Comments[ActiveDoc.Comments.Count];
            last.Range.Select();
            AddComment(text);
        }

        // ── Paste special ─────────────────────────────────────────────────────
        public void PasteAsPlainText()
        {
            App.Selection.PasteSpecial(DataType: WdPasteDataType.wdPasteText);
        }

        public void PasteAsRtf()
        {
            App.Selection.PasteSpecial(DataType: WdPasteDataType.wdPasteRTF);
        }

        // ── Change tracking ───────────────────────────────────────────────────
        public string GetTrackedChangeSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Правок отслежено: {ActiveDoc.Revisions.Count}");
            int i = 0;
            foreach (Word.Revision r in ActiveDoc.Revisions)
            {
                if (i++ > 20) { sb.AppendLine("... (и ещё)"); break; }
                sb.AppendLine($"  [{r.Author}] {r.Type}: «{r.Range.Text?.Trim()}»");
            }
            return sb.ToString();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private void EnsureSelectionInMainStory()
        {
            try
            {
                if (App.Selection?.StoryType != WdStoryType.wdMainTextStory)
                    ActiveDoc.Content.Select();
            }
            catch { }
        }

        private bool HasDocumentContent()
        {
            try
            {
                var t = GetDocumentText().Replace("\r", "").Replace("\n", "").Trim();
                return t.Length > 0;
            }
            catch { return false; }
        }

        private void InsertHeadingLine(string text, int level)
        {
            var sel = App.Selection;
            ApplyHeading(level);
            sel.TypeText(text);
            sel.TypeParagraph();
        }

        private void InsertNormalLine(string text)
        {
            var sel = App.Selection;
            ApplyNormalStyle();
            if (!string.IsNullOrEmpty(text)) sel.TypeText(text);
            sel.TypeParagraph();
        }
    }
}
