using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Word = Microsoft.Office.Interop.Word;

namespace OllamaAI
{
    internal sealed class WordAgentController
    {
        private readonly OllamaService _ollama;
        private readonly JavaScriptSerializer _json;
        private readonly string _systemPrompt;

        public WordAgentController(OllamaService ollama, string systemPrompt)
        {
            _ollama = ollama ?? throw new ArgumentNullException(nameof(ollama));
            _systemPrompt = systemPrompt ?? "";
            _json = new JavaScriptSerializer { MaxJsonLength = 1024 * 1024 };
        }

        public async System.Threading.Tasks.Task RunAsync(
            Word.Application app,
            string model,
            string userPrompt,
            Action<Action> runOnUi,
            int maxSteps = 4)
        {
            if (app == null || app.ActiveDocument == null)
            {
                return;
            }

            var extractor = new DocumentExtractor();
            var history = new List<AgentToolResult>();
            var memory = AgentMemoryStore.Get(app.ActiveDocument);
            for (var step = 0; step < maxSteps; step++)
            {
                runOnUi(() =>
                {
                    try
                    {
                        app.StatusBar = $"Агент: шаг {step + 1}/{maxSteps}";
                        System.Windows.Forms.Application.DoEvents();
                    }
                    catch
                    {
                    }
                });
                DocumentSnapshot snapshot = null;
                runOnUi(() => { snapshot = extractor.Extract(app.ActiveDocument, app.Selection); });

                var prompt = BuildAgentPrompt(userPrompt, snapshot, history, memory, step + 1, maxSteps);
                var response = await _ollama.GenerateAsync(model, prompt, _systemPrompt).ConfigureAwait(false);

                var parsed = AgentResponseParser.TryParse(response, out var actions, out var finalNote);
                if (!parsed)
                {
                    var repairPrompt = BuildAgentPrompt(userPrompt, snapshot, history, memory, step + 1, maxSteps) +
                        "\n\nПредыдущий ответ был некорректным JSON. Исправь формат и верни только JSON.\n" +
                        "Ответ модели:\n" + (response ?? "");

                    var repaired = await _ollama.GenerateAsync(model, repairPrompt, _systemPrompt).ConfigureAwait(false);
                    parsed = AgentResponseParser.TryParse(repaired, out actions, out finalNote);
                    if (!parsed)
                    {
                        runOnUi(() =>
                        {
                            System.Windows.Forms.MessageBox.Show(
                                "Агент вернул некорректный формат. Уточните запрос и повторите.",
                                "Ollama AI",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Warning);
                        });
                        break;
                    }
                }

                if (actions.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(finalNote))
                    {
                        runOnUi(() =>
                        {
                            System.Windows.Forms.MessageBox.Show(
                                finalNote,
                                "Ollama AI",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        });
                    }
                    break;
                }

                var executor = new WordToolExecutor();
                var stepResults = new List<AgentToolResult>();

                foreach (var action in actions)
                {
                    if (action == null || string.IsNullOrWhiteSpace(action.Tool))
                    {
                        stepResults.Add(AgentToolResult.Fail("unknown", "ÐŸÑƒÑÑ‚Ð°Ñ ÐºÐ¾Ð¼Ð°Ð½Ð´Ð°."));
                        continue;
                    }

                    if (string.Equals(action.Tool, "summarize", StringComparison.OrdinalIgnoreCase))
                    {
                        var summaryResult = await ExecuteSummarizeAsync(action, app, model, runOnUi).ConfigureAwait(false);
                        stepResults.Add(summaryResult);
                        continue;
                    }

                    AgentToolResult result = null;
                    runOnUi(() => { result = executor.Execute(app, action); });
                    stepResults.Add(result ?? AgentToolResult.Fail(action.Tool, "ÐÐµÐ¸Ð·Ð²ÐµÑÑ‚Ð½Ð°Ñ Ð¾ÑˆÐ¸Ð±ÐºÐ°."));

                    runOnUi(() =>
                    {
                        try
                        {
                            System.Windows.Forms.Application.DoEvents();
                        }
                        catch
                        {
                        }
                    });
                }

                history.AddRange(stepResults);
                memory?.AddEntry(AgentMemoryEntry.FromStep(userPrompt, actions, stepResults));

                if (actions.Any(a => a != null && string.Equals(a.Tool, "done", StringComparison.OrdinalIgnoreCase)))
                {
                    break;
                }
            }

            runOnUi(() =>
            {
                try
                {
                    app.StatusBar = "";
                }
                catch
                {
                }
            });
        }

        private async System.Threading.Tasks.Task<AgentToolResult> ExecuteSummarizeAsync(
            AgentAction action,
            Word.Application app,
            string model,
            Action<Action> runOnUi)
        {
            var scope = action.GetString("scope", "selection");
            var location = action.GetString("location", "after_selection");

            string source = null;
            runOnUi(() =>
            {
                var range = WordToolExecutor.ResolveScopeRange(app, scope);
                source = range != null ? range.Text : "";
            });

            if (string.IsNullOrWhiteSpace(source))
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµÑ‚ Ñ‚ÐµÐºÑÑ‚Ð° Ð´Ð»Ñ ÐºÑ€Ð°Ñ‚ÐºÐ¾Ð³Ð¾ Ð¿ÐµÑ€ÐµÑÐºÐ°Ð·Ð°.");
            }

            var prompt =
                "Ð¡Ð´ÐµÐ»Ð°Ð¹ ÐºÑ€Ð°Ñ‚ÐºÐ¾Ðµ Ñ€ÐµÐ·ÑŽÐ¼Ðµ Ñ‚ÐµÐºÑÑ‚Ð° (5-8 Ð¿ÑƒÐ½ÐºÑ‚Ð¾Ð²), Ð±ÐµÐ· Markdown, Ð¾Ð±Ñ‹Ñ‡Ð½Ñ‹Ð¹ Ñ‚ÐµÐºÑÑ‚.\n\n" +
                source;

            var summary = await _ollama.GenerateAsync(model, prompt, _systemPrompt).ConfigureAwait(false);

            runOnUi(() =>
            {
                var range = WordToolExecutor.ResolveInsertionRange(app, location, action.GetString("heading", ""));
                if (range == null)
                {
                    range = app.Selection?.Range ?? app.ActiveDocument.Content;
                }
                range.Text = summary ?? "";
            });

            return AgentToolResult.Ok(action.Tool, "Ð ÐµÐ·ÑŽÐ¼Ðµ Ð²ÑÑ‚Ð°Ð²Ð»ÐµÐ½Ð¾.", new Dictionary<string, object> { { "length", summary?.Length ?? 0 } });
        }

        private string BuildAgentPrompt(
            string userPrompt,
            DocumentSnapshot snapshot,
            List<AgentToolResult> history,
            AgentMemory memory,
            int step,
            int maxSteps)
        {
            var trimmedHistory = new List<AgentToolResult>();
            if (history != null && history.Count > 0)
            {
                var start = Math.Max(0, history.Count - 6);
                trimmedHistory = history.Skip(start).ToList();
            }

            var payload = new Dictionary<string, object>
            {
                { "request", userPrompt ?? "" },
                { "document", snapshot },
                { "history", trimmedHistory }
            };

            var json = _json.Serialize(payload);

            var sb = new StringBuilder();
            sb.AppendLine("Ð¢Ñ‹ Ð°Ð³ÐµÐ½Ñ‚, ÑƒÐ¿Ñ€Ð°Ð²Ð»ÑÑŽÑ‰Ð¸Ð¹ Microsoft Word Ñ‡ÐµÑ€ÐµÐ· Ð½Ð°Ð±Ð¾Ñ€ Ð¸Ð½ÑÑ‚Ñ€ÑƒÐ¼ÐµÐ½Ñ‚Ð¾Ð².");
            sb.AppendLine("ÐžÑ‚Ð²ÐµÑ‡Ð°Ð¹ Ð¡Ð¢Ð ÐžÐ“Ðž JSON Ð±ÐµÐ· Markdown Ð¸ Ð±ÐµÐ· ÐºÐ¾Ð¼Ð¼ÐµÐ½Ñ‚Ð°Ñ€Ð¸ÐµÐ².");
            sb.AppendLine("Ð¤Ð¾Ñ€Ð¼Ð°Ñ‚ Ð¾Ñ‚Ð²ÐµÑ‚Ð°:");
            sb.AppendLine("{\"actions\":[{\"tool\":\"...\",\"params\":{...}}],\"final\":\"\"}");
            sb.AppendLine("Если запрос про форматирование (курсив, жирный, выравнивание, шрифт, размер),");
            sb.AppendLine("НЕ вставляй текст. Используй format_text с scope=document или scope=selection.");
            sb.AppendLine("Если нужно создать/изменить таблицу — используй create_table/write_cell/format_table.");
            sb.AppendLine("Ð•ÑÐ»Ð¸ Ð½ÑƒÐ¶Ð½Ð¾ Ð²ÑÑ‚Ð°Ð²Ð¸Ñ‚ÑŒ Ñ‚ÐµÐºÑÑ‚ â€” Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·ÑƒÐ¹ insert_text.");
            sb.AppendLine("Ð•ÑÐ»Ð¸ Ð´ÐµÐ¹ÑÑ‚Ð²Ð¸Ð¹ Ð½Ðµ Ñ‚Ñ€ÐµÐ±ÑƒÐµÑ‚ÑÑ â€” Ð²ÐµÑ€Ð½Ð¸ Ð¿ÑƒÑÑ‚Ð¾Ð¹ ÑÐ¿Ð¸ÑÐ¾Ðº actions Ð¸ Ð¾Ð¿Ñ†Ð¸Ð¾Ð½Ð°Ð»ÑŒÐ½Ð¾ final.");
            sb.AppendLine("Ð”Ð¾ÑÑ‚ÑƒÐ¿Ð½Ñ‹Ðµ Ð¸Ð½ÑÑ‚Ñ€ÑƒÐ¼ÐµÐ½Ñ‚Ñ‹:");
            sb.AppendLine(ToolCatalog.Text);
            sb.AppendLine($"Ð¨Ð°Ð³ {step} Ð¸Ð· {maxSteps}.");
            var memorySummary = memory?.BuildSummary();
            if (!string.IsNullOrWhiteSpace(memorySummary))
            {
                sb.AppendLine("ÐŸÐ°Ð¼ÑÑ‚ÑŒ (Ð¿Ð¾ÑÐ»ÐµÐ´Ð½Ð¸Ðµ Ð´ÐµÐ¹ÑÑ‚Ð²Ð¸Ñ):");
                sb.AppendLine(memorySummary);
            }
            sb.AppendLine("Ð”Ð°Ð½Ð½Ñ‹Ðµ:");
            sb.AppendLine(json);
            return sb.ToString();
        }
    }

    internal static class ToolCatalog
    {
        public static readonly string Text =
            "get_document_structure\n" +
            "insert_text {text, location, heading, replace}\n" +
            "replace_text {target_text, new_text, scope, match_case, whole_word, first_only}\n" +
            "delete_text {target_text, scope, match_case, whole_word}\n" +
            "add_paragraph {text, location, heading}\n" +
            "delete_paragraph {index}\n" +
            "create_heading {text, level, location, heading}\n" +
            "format_text {scope, target_text, bold, italic, underline, strike, all_caps, small_caps, font_name, font_size, font_color, highlight_color, alignment, line_spacing, space_before, space_after, left_indent, right_indent, first_line_indent}\n" +
            "apply_style {style, scope, target_text}\n" +
            "create_table {rows, cols, data, location, heading, borders}\n" +
            "add_row {table}\n" +
            "add_column {table}\n" +
            "delete_row {table, row}\n" +
            "delete_column {table, column}\n" +
            "write_cell {table, row, column, text}\n" +
            "format_table {table, style, borders, auto_fit, header_bold}\n" +
            "add_formula {formula, location, heading}\n" +
            "calculate {expression}\n" +
            "calculate_table {table, row, column, expression}\n" +
            "move_text {source_text, destination, heading, scope}\n" +
            "summarize {scope, location, heading}\n" +
            "undo {steps}\n" +
            "save_document";
    }

    internal sealed class DocumentExtractor
    {
        private const int MaxText = 240;

        public DocumentSnapshot Extract(Word.Document doc, Word.Selection selection)
        {
            var snapshot = new DocumentSnapshot
            {
                Title = doc.Name ?? "",
                WordCount = doc.Words?.Count ?? 0,
                ParagraphCount = doc.Paragraphs?.Count ?? 0,
                TableCount = doc.Tables?.Count ?? 0,
                SectionCount = doc.Sections?.Count ?? 0
            };

            snapshot.Selection = BuildSelection(selection);

            var paragraphIndex = 0;
            foreach (Word.Paragraph paragraph in doc.Paragraphs)
            {
                paragraphIndex++;
                var text = CleanText(paragraph.Range.Text);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var styleName = GetStyleName(paragraph.Range.get_Style());
                var outlineLevel = (int)paragraph.OutlineLevel;
                var isHeading = outlineLevel >= 1 && outlineLevel <= 9;
                var info = new ParagraphInfo
                {
                    Index = paragraphIndex,
                    Text = Clip(text, MaxText),
                    Style = styleName,
                    OutlineLevel = outlineLevel,
                    IsHeading = isHeading,
                    IsList = paragraph.Range.ListFormat != null && paragraph.Range.ListFormat.ListType != Word.WdListType.wdListNoNumbering
                };
                snapshot.Paragraphs.Add(info);

                if (isHeading)
                {
                    snapshot.Headings.Add(new HeadingInfo
                    {
                        ParagraphIndex = paragraphIndex,
                        Text = Clip(text, MaxText),
                        Level = outlineLevel
                    });
                }
            }

            var tableIndex = 0;
            foreach (Word.Table table in doc.Tables)
            {
                tableIndex++;
                var info = new TableInfo
                {
                    Index = tableIndex,
                    Rows = table.Rows.Count,
                    Columns = table.Columns.Count,
                    Preview = BuildTablePreview(table)
                };
                snapshot.Tables.Add(info);
            }

            foreach (Word.Section section in doc.Sections)
            {
                var range = section.Range;
                snapshot.Sections.Add(new SectionInfo
                {
                    Start = range.Start,
                    End = range.End,
                    PageSetup = section.PageSetup != null ? section.PageSetup.PaperSize.ToString() : ""
                });
            }

            foreach (Word.Style style in doc.Styles)
            {
                var name = GetStyleName(style);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                snapshot.Styles.Add(new StyleInfo
                {
                    Name = name,
                    Type = style.Type.ToString()
                });
            }

            return snapshot;
        }

        private static SelectionInfo BuildSelection(Word.Selection selection)
        {
            if (selection == null || selection.Range == null)
            {
                return new SelectionInfo { HasSelection = false };
            }

            var range = selection.Range;
            var text = CleanText(range.Text);
            return new SelectionInfo
            {
                HasSelection = range.Start != range.End,
                Start = range.Start,
                End = range.End,
                Text = Clip(text, MaxText)
            };
        }

        private static string GetStyleName(object style)
        {
            var wordStyle = style as Word.Style;
            if (wordStyle != null)
            {
                return wordStyle.NameLocal ?? "";
            }

            return style != null ? style.ToString() : "";
        }

        private static string BuildTablePreview(Word.Table table)
        {
            var sb = new StringBuilder();
            var rowLimit = Math.Min(3, table.Rows.Count);
            var colLimit = Math.Min(4, table.Columns.Count);

            for (var r = 1; r <= rowLimit; r++)
            {
                var cells = new List<string>();
                for (var c = 1; c <= colLimit; c++)
                {
                    var text = CleanText(table.Cell(r, c).Range.Text);
                    cells.Add(Clip(text, 60));
                }

                sb.AppendLine(string.Join(" | ", cells));
            }

            return sb.ToString().Trim();
        }

        private static string Clip(string text, int max)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            text = text.Trim();
            if (text.Length <= max)
            {
                return text;
            }

            return text.Substring(0, max) + "...";
        }

        private static string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            return Regex.Replace(text, @"[\r\a]+", " ").Trim();
        }
    }

    internal sealed class WordToolExecutor
    {
        public AgentToolResult Execute(Word.Application app, AgentAction action)
        {
            var doc = app?.ActiveDocument;
            if (doc == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð”Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚ Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½.");
            }

            var tool = action.Tool.ToLowerInvariant();

            switch (tool)
            {
                case "get_document_structure":
                    return HandleGetStructure(doc, app.Selection);
                case "insert_text":
                    return HandleInsertText(doc, app, action);
                case "replace_text":
                    return HandleReplaceText(doc, app, action);
                case "delete_text":
                    return HandleDeleteText(doc, app, action);
                case "add_paragraph":
                    return HandleAddParagraph(doc, app, action);
                case "delete_paragraph":
                    return HandleDeleteParagraph(doc, action);
                case "create_heading":
                    return HandleCreateHeading(doc, app, action);
                case "format_text":
                    return HandleFormatText(doc, app, action);
                case "apply_style":
                    return HandleApplyStyle(doc, app, action);
                case "create_table":
                    return HandleCreateTable(doc, app, action);
                case "add_row":
                    return HandleAddRow(doc, app, action);
                case "add_column":
                    return HandleAddColumn(doc, app, action);
                case "delete_row":
                    return HandleDeleteRow(doc, app, action);
                case "delete_column":
                    return HandleDeleteColumn(doc, app, action);
                case "write_cell":
                    return HandleWriteCell(doc, app, action);
                case "format_table":
                    return HandleFormatTable(doc, app, action);
                case "add_formula":
                    return HandleAddFormula(doc, app, action);
                case "calculate":
                    return HandleCalculate(action);
                case "calculate_table":
                    return HandleCalculateTable(doc, app, action);
                case "move_text":
                    return HandleMoveText(doc, app, action);
                case "undo":
                    return HandleUndo(app, action);
                case "save_document":
                    doc.Save();
                    return AgentToolResult.Ok(action.Tool, "Ð”Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚ ÑÐ¾Ñ…Ñ€Ð°Ð½ÐµÐ½.");
                case "done":
                    return AgentToolResult.Ok(action.Tool, "Ð—Ð°Ð²ÐµÑ€ÑˆÐµÐ½Ð¾.");
                default:
                    return AgentToolResult.Fail(action.Tool, "Ð˜Ð½ÑÑ‚Ñ€ÑƒÐ¼ÐµÐ½Ñ‚ Ð½Ðµ Ð¿Ð¾Ð´Ð´ÐµÑ€Ð¶Ð¸Ð²Ð°ÐµÑ‚ÑÑ.");
            }
        }

        public static Word.Range ResolveScopeRange(Word.Application app, string scope)
        {
            if (app == null || app.ActiveDocument == null)
            {
                return null;
            }

            var scopeValue = (scope ?? "").Trim().ToLowerInvariant();
            if (scopeValue == "document" || scopeValue == "doc" || scopeValue == "Ð²ÐµÑÑŒ Ð´Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚")
            {
                return app.ActiveDocument.Content.Duplicate;
            }

            if (scopeValue == "selection" || scopeValue == "Ð²Ñ‹Ð´ÐµÐ»ÐµÐ½Ð¸Ðµ" || scopeValue == "selected")
            {
                return app.Selection?.Range?.Duplicate;
            }

            if (scopeValue == "paragraph")
            {
                return app.Selection?.Paragraphs.Count > 0 ? app.Selection.Paragraphs[1].Range.Duplicate : null;
            }

            if (scopeValue.StartsWith("heading"))
            {
                var headingText = scopeValue.Substring("heading".Length).Trim(':', ' ');
                return FindHeadingRange(app.ActiveDocument, headingText);
            }

            return app.Selection?.Range?.Duplicate ?? app.ActiveDocument.Content.Duplicate;
        }

        public static Word.Range ResolveInsertionRange(Word.Application app, string location, string heading)
        {
            if (app == null || app.ActiveDocument == null)
            {
                return null;
            }

            var loc = (location ?? "").Trim().ToLowerInvariant();
            if (loc == "document_start" || loc == "start")
            {
                var range = app.ActiveDocument.Range(0, 0);
                range.Collapse(Word.WdCollapseDirection.wdCollapseStart);
                return range;
            }

            if (loc == "document_end" || loc == "end")
            {
                var range = app.ActiveDocument.Content.Duplicate;
                range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                return range;
            }

            if (loc == "after_heading" || loc == "before_heading")
            {
                var headingRange = FindHeadingRange(app.ActiveDocument, heading);
                if (headingRange != null)
                {
                    headingRange.Collapse(loc == "after_heading" ? Word.WdCollapseDirection.wdCollapseEnd : Word.WdCollapseDirection.wdCollapseStart);
                    return headingRange;
                }
            }

            if (loc == "selection" || loc == "replace_selection")
            {
                return app.Selection?.Range?.Duplicate;
            }

            if (loc == "after_selection")
            {
                var range = app.Selection?.Range?.Duplicate;
                if (range != null)
                {
                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                    return range;
                }
            }

            var fallback = app.Selection?.Range?.Duplicate ?? app.ActiveDocument.Content.Duplicate;
            fallback.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
            return fallback;
        }

        private AgentToolResult HandleGetStructure(Word.Document doc, Word.Selection selection)
        {
            var extractor = new DocumentExtractor();
            var snapshot = extractor.Extract(doc, selection);
            var json = new JavaScriptSerializer { MaxJsonLength = 1024 * 1024 }.Serialize(snapshot);
            return AgentToolResult.Ok("get_document_structure", "Ð¡Ñ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð° Ð¿Ð¾Ð»ÑƒÑ‡ÐµÐ½Ð°.", new Dictionary<string, object> { { "json", json } });
        }

        private AgentToolResult HandleInsertText(Word.Document doc, Word.Application app, AgentAction action)
        {
            var text = action.GetString("text", "");
            if (string.IsNullOrWhiteSpace(text))
            {
                return AgentToolResult.Fail(action.Tool, "ÐŸÑƒÑÑ‚Ð¾Ð¹ Ñ‚ÐµÐºÑÑ‚.");
            }

            var location = action.GetString("location", "cursor");
            var heading = action.GetString("heading", "");
            var replace = action.GetBool("replace", false);

            var range = ResolveInsertionRange(app, location, heading) ?? doc.Content;
            if (replace)
            {
                range.Text = "";
            }

            range.Text = text;
            return AgentToolResult.Ok(action.Tool, "Ð¢ÐµÐºÑÑ‚ Ð²ÑÑ‚Ð°Ð²Ð»ÐµÐ½.");
        }

        private AgentToolResult HandleReplaceText(Word.Document doc, Word.Application app, AgentAction action)
        {
            var targetText = action.GetString("target_text", "");
            var newText = action.GetString("new_text", "");
            if (string.IsNullOrWhiteSpace(targetText))
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµÑ‚ target_text.");
            }

            var scope = action.GetString("scope", "document");
            var range = ResolveScopeRange(app, scope);
            if (range == null)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµ Ð½Ð°Ð¹Ð´ÐµÐ½ Ð´Ð¸Ð°Ð¿Ð°Ð·Ð¾Ð½.");
            }

            var matchCase = action.GetBool("match_case", false);
            var wholeWord = action.GetBool("whole_word", false);
            var firstOnly = action.GetBool("first_only", false);

            var find = range.Find;
            find.ClearFormatting();
            find.Text = targetText;
            find.MatchCase = matchCase;
            find.MatchWholeWord = wholeWord;
            find.Forward = true;
            find.Wrap = Word.WdFindWrap.wdFindStop;
            find.Replacement.ClearFormatting();
            find.Replacement.Text = newText ?? "";

            var replaceMode = firstOnly ? Word.WdReplace.wdReplaceOne : Word.WdReplace.wdReplaceAll;
            var found = find.Execute(Replace: replaceMode);
            return found ? AgentToolResult.Ok(action.Tool, "Ð—Ð°Ð¼ÐµÐ½Ð° Ð²Ñ‹Ð¿Ð¾Ð»Ð½ÐµÐ½Ð°.") : AgentToolResult.Fail(action.Tool, "Ð¡Ð¾Ð²Ð¿Ð°Ð´ÐµÐ½Ð¸Ñ Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ñ‹.");
        }

        private AgentToolResult HandleDeleteText(Word.Document doc, Word.Application app, AgentAction action)
        {
            var targetText = action.GetString("target_text", "");
            if (string.IsNullOrWhiteSpace(targetText))
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµÑ‚ target_text.");
            }

            var scope = action.GetString("scope", "document");
            var range = ResolveScopeRange(app, scope);
            if (range == null)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµ Ð½Ð°Ð¹Ð´ÐµÐ½ Ð´Ð¸Ð°Ð¿Ð°Ð·Ð¾Ð½.");
            }

            var matchCase = action.GetBool("match_case", false);
            var wholeWord = action.GetBool("whole_word", false);

            var find = range.Find;
            find.ClearFormatting();
            find.Text = targetText;
            find.MatchCase = matchCase;
            find.MatchWholeWord = wholeWord;
            find.Forward = true;
            find.Wrap = Word.WdFindWrap.wdFindStop;
            find.Replacement.ClearFormatting();
            find.Replacement.Text = "";
            var found = find.Execute(Replace: Word.WdReplace.wdReplaceAll);
            return found ? AgentToolResult.Ok(action.Tool, "Ð¢ÐµÐºÑÑ‚ ÑƒÐ´Ð°Ð»ÐµÐ½.") : AgentToolResult.Fail(action.Tool, "Ð¡Ð¾Ð²Ð¿Ð°Ð´ÐµÐ½Ð¸Ñ Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ñ‹.");
        }

        private AgentToolResult HandleAddParagraph(Word.Document doc, Word.Application app, AgentAction action)
        {
            var text = action.GetString("text", "");
            var range = ResolveInsertionRange(app, action.GetString("location", "cursor"), action.GetString("heading", "")) ?? doc.Content;
            range.Text = text;
            range.InsertParagraphAfter();
            return AgentToolResult.Ok(action.Tool, "ÐÐ±Ð·Ð°Ñ† Ð´Ð¾Ð±Ð°Ð²Ð»ÐµÐ½.");
        }

        private AgentToolResult HandleDeleteParagraph(Word.Document doc, AgentAction action)
        {
            var index = action.GetInt("index", -1);
            if (index <= 0 || index > doc.Paragraphs.Count)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµÐ²ÐµÑ€Ð½Ñ‹Ð¹ Ð¸Ð½Ð´ÐµÐºÑ Ð°Ð±Ð·Ð°Ñ†Ð°.");
            }

            doc.Paragraphs[index].Range.Delete();
            return AgentToolResult.Ok(action.Tool, "ÐÐ±Ð·Ð°Ñ† ÑƒÐ´Ð°Ð»ÐµÐ½.");
        }

        private AgentToolResult HandleCreateHeading(Word.Document doc, Word.Application app, AgentAction action)
        {
            var text = action.GetString("text", "");
            if (string.IsNullOrWhiteSpace(text))
            {
                return AgentToolResult.Fail(action.Tool, "ÐŸÑƒÑÑ‚Ð¾Ð¹ Ð·Ð°Ð³Ð¾Ð»Ð¾Ð²Ð¾Ðº.");
            }

            var level = action.GetInt("level", 1);
            level = Math.Max(1, Math.Min(9, level));
            var range = ResolveInsertionRange(app, action.GetString("location", "cursor"), action.GetString("heading", "")) ?? doc.Content;
            range.Text = text;
            range.InsertParagraphAfter();
            try
            {
                range.set_Style(doc.Styles["Heading " + level]);
            }
            catch
            {
            }

            return AgentToolResult.Ok(action.Tool, "Ð—Ð°Ð³Ð¾Ð»Ð¾Ð²Ð¾Ðº Ð´Ð¾Ð±Ð°Ð²Ð»ÐµÐ½.");
        }

        private AgentToolResult HandleFormatText(Word.Document doc, Word.Application app, AgentAction action)
        {
            var range = ResolveTargetRange(doc, app, action);
            if (range == null)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµ Ð½Ð°Ð¹Ð´ÐµÐ½ Ð´Ð¸Ð°Ð¿Ð°Ð·Ð¾Ð½.");
            }

            ApplyFormatting(range, action);
            return AgentToolResult.Ok(action.Tool, "Ð¤Ð¾Ñ€Ð¼Ð°Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð¸Ðµ Ð¿Ñ€Ð¸Ð¼ÐµÐ½ÐµÐ½Ð¾.");
        }

        private AgentToolResult HandleApplyStyle(Word.Document doc, Word.Application app, AgentAction action)
        {
            var styleName = action.GetString("style", "");
            if (string.IsNullOrWhiteSpace(styleName))
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµ ÑƒÐºÐ°Ð·Ð°Ð½ ÑÑ‚Ð¸Ð»ÑŒ.");
            }

            var range = ResolveTargetRange(doc, app, action);
            if (range == null)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµ Ð½Ð°Ð¹Ð´ÐµÐ½ Ð´Ð¸Ð°Ð¿Ð°Ð·Ð¾Ð½.");
            }

            var style = FindStyle(doc, styleName);
            if (style == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð¡Ñ‚Ð¸Ð»ÑŒ Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½.");
            }

            range.set_Style(style);
            return AgentToolResult.Ok(action.Tool, "Ð¡Ñ‚Ð¸Ð»ÑŒ Ð¿Ñ€Ð¸Ð¼ÐµÐ½ÐµÐ½.");
        }

        private AgentToolResult HandleCreateTable(Word.Document doc, Word.Application app, AgentAction action)
        {
            var rows = action.GetInt("rows", 2);
            var cols = action.GetInt("cols", 2);
            rows = Math.Max(1, rows);
            cols = Math.Max(1, cols);

            var range = ResolveInsertionRange(app, action.GetString("location", "cursor"), action.GetString("heading", "")) ?? doc.Content;
            var table = doc.Tables.Add(range, rows, cols);
            var borders = action.GetBool("borders", true);
            table.Borders.Enable = borders ? 1 : 0;

            var data = action.GetMatrix("data");
            if (data != null && data.Count > 0)
            {
                FillTable(table, data);
            }

            return AgentToolResult.Ok(action.Tool, "Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° ÑÐ¾Ð·Ð´Ð°Ð½Ð°.", new Dictionary<string, object> { { "rows", rows }, { "cols", cols } });
        }

        private AgentToolResult HandleAddRow(Word.Document doc, Word.Application app, AgentAction action)
        {
            var table = ResolveTable(doc, app, action);
            if (table == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð°.");
            }

            table.Rows.Add();
            return AgentToolResult.Ok(action.Tool, "Ð¡Ñ‚Ñ€Ð¾ÐºÐ° Ð´Ð¾Ð±Ð°Ð²Ð»ÐµÐ½Ð°.");
        }

        private AgentToolResult HandleAddColumn(Word.Document doc, Word.Application app, AgentAction action)
        {
            var table = ResolveTable(doc, app, action);
            if (table == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð°.");
            }

            table.Columns.Add();
            return AgentToolResult.Ok(action.Tool, "Ð¡Ñ‚Ð¾Ð»Ð±ÐµÑ† Ð´Ð¾Ð±Ð°Ð²Ð»ÐµÐ½.");
        }

        private AgentToolResult HandleDeleteRow(Word.Document doc, Word.Application app, AgentAction action)
        {
            var table = ResolveTable(doc, app, action);
            if (table == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð°.");
            }

            var row = action.GetInt("row", -1);
            if (row <= 0 || row > table.Rows.Count)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµÐ²ÐµÑ€Ð½Ñ‹Ð¹ Ð½Ð¾Ð¼ÐµÑ€ ÑÑ‚Ñ€Ð¾ÐºÐ¸.");
            }

            table.Rows[row].Delete();
            return AgentToolResult.Ok(action.Tool, "Ð¡Ñ‚Ñ€Ð¾ÐºÐ° ÑƒÐ´Ð°Ð»ÐµÐ½Ð°.");
        }

        private AgentToolResult HandleDeleteColumn(Word.Document doc, Word.Application app, AgentAction action)
        {
            var table = ResolveTable(doc, app, action);
            if (table == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð°.");
            }

            var column = action.GetInt("column", -1);
            if (column <= 0 || column > table.Columns.Count)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµÐ²ÐµÑ€Ð½Ñ‹Ð¹ Ð½Ð¾Ð¼ÐµÑ€ ÑÑ‚Ð¾Ð»Ð±Ñ†Ð°.");
            }

            table.Columns[column].Delete();
            return AgentToolResult.Ok(action.Tool, "Ð¡Ñ‚Ð¾Ð»Ð±ÐµÑ† ÑƒÐ´Ð°Ð»ÐµÐ½.");
        }

        private AgentToolResult HandleWriteCell(Word.Document doc, Word.Application app, AgentAction action)
        {
            var table = ResolveTable(doc, app, action);
            if (table == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð°.");
            }

            var row = action.GetInt("row", 1);
            var column = action.GetInt("column", 1);
            if (row <= 0 || column <= 0 || row > table.Rows.Count || column > table.Columns.Count)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµÐ²ÐµÑ€Ð½Ð°Ñ ÑÑ‡ÐµÐ¹ÐºÐ°.");
            }

            table.Cell(row, column).Range.Text = action.GetString("text", "");
            return AgentToolResult.Ok(action.Tool, "Ð¯Ñ‡ÐµÐ¹ÐºÐ° Ð·Ð°Ð¿Ð¾Ð»Ð½ÐµÐ½Ð°.");
        }

        private AgentToolResult HandleFormatTable(Word.Document doc, Word.Application app, AgentAction action)
        {
            var table = ResolveTable(doc, app, action);
            if (table == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð°.");
            }

            var styleName = action.GetString("style", "");
            if (!string.IsNullOrWhiteSpace(styleName))
            {
                try
                {
                    table.set_Style(styleName);
                }
                catch
                {
                }
            }

            var borders = action.GetNullableBool("borders");
            if (borders.HasValue)
            {
                table.Borders.Enable = borders.Value ? 1 : 0;
            }

            var autoFit = action.GetString("auto_fit", "");
            if (!string.IsNullOrWhiteSpace(autoFit))
            {
                if (autoFit.Equals("contents", StringComparison.OrdinalIgnoreCase))
                {
                    table.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitContent);
                }
                else if (autoFit.Equals("window", StringComparison.OrdinalIgnoreCase))
                {
                    table.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);
                }
            }

            if (action.GetBool("header_bold", false) && table.Rows.Count > 0)
            {
                table.Rows[1].Range.Bold = 1;
            }

            return AgentToolResult.Ok(action.Tool, "Ð¤Ð¾Ñ€Ð¼Ð°Ñ‚ Ñ‚Ð°Ð±Ð»Ð¸Ñ†Ñ‹ Ð¿Ñ€Ð¸Ð¼ÐµÐ½ÐµÐ½.");
        }

        private AgentToolResult HandleAddFormula(Word.Document doc, Word.Application app, AgentAction action)
        {
            var formula = action.GetString("formula", "");
            if (string.IsNullOrWhiteSpace(formula))
            {
                return AgentToolResult.Fail(action.Tool, "Ð¤Ð¾Ñ€Ð¼ÑƒÐ»Ð° Ð¿ÑƒÑÑ‚Ð°Ñ.");
            }

            var range = ResolveInsertionRange(app, action.GetString("location", "cursor"), action.GetString("heading", "")) ?? doc.Content;
            range.Text = formula;
            try
            {
                doc.OMaths.Add(range);
            }
            catch
            {
            }

            return AgentToolResult.Ok(action.Tool, "Ð¤Ð¾Ñ€Ð¼ÑƒÐ»Ð° Ð²ÑÑ‚Ð°Ð²Ð»ÐµÐ½Ð°.");
        }

        private AgentToolResult HandleCalculate(AgentAction action)
        {
            var expression = action.GetString("expression", "");
            if (string.IsNullOrWhiteSpace(expression))
            {
                return AgentToolResult.Fail(action.Tool, "ÐŸÑƒÑÑ‚Ð¾Ðµ Ð²Ñ‹Ñ€Ð°Ð¶ÐµÐ½Ð¸Ðµ.");
            }

            try
            {
                var result = new DataTable().Compute(expression, null);
                return AgentToolResult.Ok(action.Tool, "Ð’Ñ‹Ñ‡Ð¸ÑÐ»ÐµÐ½Ð¾.", new Dictionary<string, object> { { "result", result?.ToString() ?? "" } });
            }
            catch (Exception ex)
            {
                return AgentToolResult.Fail(action.Tool, "ÐžÑˆÐ¸Ð±ÐºÐ° Ð²Ñ‹Ñ‡Ð¸ÑÐ»ÐµÐ½Ð¸Ñ: " + ex.Message);
            }
        }

        private AgentToolResult HandleCalculateTable(Word.Document doc, Word.Application app, AgentAction action)
        {
            var table = ResolveTable(doc, app, action);
            if (table == null)
            {
                return AgentToolResult.Fail(action.Tool, "Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð°.");
            }

            var expression = action.GetString("expression", "");
            if (string.IsNullOrWhiteSpace(expression))
            {
                return AgentToolResult.Fail(action.Tool, "ÐŸÑƒÑÑ‚Ð¾Ðµ Ð²Ñ‹Ñ€Ð°Ð¶ÐµÐ½Ð¸Ðµ.");
            }

            var row = action.GetInt("row", 1);
            var column = action.GetInt("column", 1);
            if (row <= 0 || column <= 0 || row > table.Rows.Count || column > table.Columns.Count)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµÐ²ÐµÑ€Ð½Ð°Ñ ÑÑ‡ÐµÐ¹ÐºÐ°.");
            }

            try
            {
                var result = new DataTable().Compute(expression, null);
                table.Cell(row, column).Range.Text = result?.ToString() ?? "";
                return AgentToolResult.Ok(action.Tool, "Ð—Ð°Ð¿Ð¸ÑÐ°Ð½Ð¾ Ð² Ñ‚Ð°Ð±Ð»Ð¸Ñ†Ñƒ.", new Dictionary<string, object> { { "result", result?.ToString() ?? "" } });
            }
            catch (Exception ex)
            {
                return AgentToolResult.Fail(action.Tool, "ÐžÑˆÐ¸Ð±ÐºÐ° Ð²Ñ‹Ñ‡Ð¸ÑÐ»ÐµÐ½Ð¸Ñ: " + ex.Message);
            }
        }

        private AgentToolResult HandleMoveText(Word.Document doc, Word.Application app, AgentAction action)
        {
            var sourceText = action.GetString("source_text", "");
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return AgentToolResult.Fail(action.Tool, "ÐŸÑƒÑÑ‚Ð¾Ð¹ source_text.");
            }

            var scope = action.GetString("scope", "document");
            var scopeRange = ResolveScopeRange(app, scope);
            if (scopeRange == null)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµ Ð½Ð°Ð¹Ð´ÐµÐ½ Ð´Ð¸Ð°Ð¿Ð°Ð·Ð¾Ð½.");
            }

            var find = scopeRange.Find;
            find.ClearFormatting();
            find.Text = sourceText;
            find.MatchCase = action.GetBool("match_case", false);
            find.MatchWholeWord = action.GetBool("whole_word", false);
            find.Forward = true;
            find.Wrap = Word.WdFindWrap.wdFindStop;
            if (!find.Execute())
            {
                return AgentToolResult.Fail(action.Tool, "Ð¢ÐµÐºÑÑ‚ Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½.");
            }

            var foundRange = scopeRange.Duplicate;
            var text = foundRange.Text;
            foundRange.Text = "";

            var destination = action.GetString("destination", "cursor");
            var insertRange = ResolveInsertionRange(app, destination, action.GetString("heading", ""));
            if (insertRange == null)
            {
                return AgentToolResult.Fail(action.Tool, "ÐÐµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð¾ Ð¼ÐµÑÑ‚Ð¾ Ð²ÑÑ‚Ð°Ð²ÐºÐ¸.");
            }

            insertRange.Text = text;
            return AgentToolResult.Ok(action.Tool, "Ð¢ÐµÐºÑÑ‚ Ð¿ÐµÑ€ÐµÐ½ÐµÑÐµÐ½.");
        }

        private AgentToolResult HandleUndo(Word.Application app, AgentAction action)
        {
            if (app == null)
            {
                return AgentToolResult.Fail(action.Tool, "Приложение Word не найдено.");
            }

            var steps = action.GetInt("steps", 1);
            if (steps < 1)
            {
                steps = 1;
            }

            var undone = 0;
            for (var i = 0; i < steps; i++)
            {
                try
                {
                    if (app.CommandBars != null && !app.CommandBars.GetEnabledMso("Undo"))
                    {
                        break;
                    }

                    app.CommandBars.ExecuteMso("Undo");
                    undone++;
                }
                catch
                {
                    break;
                }
            }

            if (undone == 0)
            {
                return AgentToolResult.Fail(action.Tool, "Нет действий для отмены.");
            }

            return AgentToolResult.Ok(action.Tool, $"Отменено действий: {undone}.");
        }
        private static Word.Range ResolveTargetRange(Word.Document doc, Word.Application app, AgentAction action)
        {
            var targetText = action.GetString("target_text", "");
            if (!string.IsNullOrWhiteSpace(targetText))
            {
                var range = doc.Content.Duplicate;
                var find = range.Find;
                find.ClearFormatting();
                find.Text = targetText;
                find.MatchCase = action.GetBool("match_case", false);
                find.MatchWholeWord = action.GetBool("whole_word", false);
                find.Forward = true;
                find.Wrap = Word.WdFindWrap.wdFindStop;
                if (find.Execute())
                {
                    return range;
                }
            }

            return ResolveScopeRange(app, action.GetString("scope", "selection"));
        }

        private static void ApplyFormatting(Word.Range range, AgentAction action)
        {
            var bold = action.GetNullableBool("bold");
            if (bold.HasValue)
            {
                range.Font.Bold = bold.Value ? 1 : 0;
            }

            var italic = action.GetNullableBool("italic");
            if (italic.HasValue)
            {
                range.Font.Italic = italic.Value ? 1 : 0;
            }

            var underline = action.GetNullableBool("underline");
            if (underline.HasValue)
            {
                range.Font.Underline = underline.Value ? Word.WdUnderline.wdUnderlineSingle : Word.WdUnderline.wdUnderlineNone;
            }

            var strike = action.GetNullableBool("strike");
            if (strike.HasValue)
            {
                range.Font.StrikeThrough = strike.Value ? 1 : 0;
            }

            var allCaps = action.GetNullableBool("all_caps");
            if (allCaps.HasValue)
            {
                range.Font.AllCaps = allCaps.Value ? 1 : 0;
            }

            var smallCaps = action.GetNullableBool("small_caps");
            if (smallCaps.HasValue)
            {
                range.Font.SmallCaps = smallCaps.Value ? 1 : 0;
            }

            var fontName = action.GetString("font_name", "");
            if (!string.IsNullOrWhiteSpace(fontName))
            {
                range.Font.Name = fontName;
            }

            var fontSize = action.GetDouble("font_size", 0);
            if (fontSize > 0)
            {
                range.Font.Size = (float)fontSize;
            }

            var fontColor = action.GetString("font_color", "");
            if (!string.IsNullOrWhiteSpace(fontColor))
            {
                var color = TryParseColor(fontColor);
                if (color.HasValue)
                {
                    range.Font.Color = (Word.WdColor)ColorTranslator.ToOle(color.Value);
                }
            }

            var highlight = action.GetString("highlight_color", "");
            if (!string.IsNullOrWhiteSpace(highlight))
            {
                if (TryParseHighlight(highlight, out var idx))
                {
                    range.HighlightColorIndex = idx;
                }
            }

            var alignment = action.GetString("alignment", "");
            if (!string.IsNullOrWhiteSpace(alignment))
            {
                range.ParagraphFormat.Alignment = ParseAlignment(alignment);
            }

            var lineSpacing = action.GetDouble("line_spacing", 0);
            if (lineSpacing > 0)
            {
                range.ParagraphFormat.LineSpacing = (float)lineSpacing;
            }

            var spaceBefore = action.GetDouble("space_before", double.NaN);
            if (!double.IsNaN(spaceBefore))
            {
                range.ParagraphFormat.SpaceBefore = (float)spaceBefore;
            }

            var spaceAfter = action.GetDouble("space_after", double.NaN);
            if (!double.IsNaN(spaceAfter))
            {
                range.ParagraphFormat.SpaceAfter = (float)spaceAfter;
            }

            var leftIndent = action.GetDouble("left_indent", double.NaN);
            if (!double.IsNaN(leftIndent))
            {
                range.ParagraphFormat.LeftIndent = (float)leftIndent;
            }

            var rightIndent = action.GetDouble("right_indent", double.NaN);
            if (!double.IsNaN(rightIndent))
            {
                range.ParagraphFormat.RightIndent = (float)rightIndent;
            }

            var firstLine = action.GetDouble("first_line_indent", double.NaN);
            if (!double.IsNaN(firstLine))
            {
                range.ParagraphFormat.FirstLineIndent = (float)firstLine;
            }
        }

        private static Word.Table ResolveTable(Word.Document doc, Word.Application app, AgentAction action)
        {
            var index = action.GetInt("table", -1);
            if (index <= 0 && action.Params != null && action.Params.ContainsKey("index"))
            {
                index = action.GetInt("index", -1);
            }

            if (index > 0 && index <= doc.Tables.Count)
            {
                return doc.Tables[index];
            }

            if (app.Selection != null && app.Selection.Range != null && app.Selection.Range.Tables.Count > 0)
            {
                return app.Selection.Range.Tables[1];
            }

            return null;
        }

        private static void FillTable(Word.Table table, List<List<string>> data)
        {
            var rowCount = Math.Min(table.Rows.Count, data.Count);
            for (var r = 0; r < rowCount; r++)
            {
                var row = data[r];
                if (row == null)
                {
                    continue;
                }

                var colCount = Math.Min(table.Columns.Count, row.Count);
                for (var c = 0; c < colCount; c++)
                {
                    table.Cell(r + 1, c + 1).Range.Text = row[c] ?? "";
                }
            }
        }

        private static Word.Style FindStyle(Word.Document doc, string styleName)
        {
            if (doc == null || string.IsNullOrWhiteSpace(styleName))
            {
                return null;
            }

            foreach (Word.Style style in doc.Styles)
            {
                var name = style.NameLocal ?? "";
                if (string.Equals(name, styleName, StringComparison.OrdinalIgnoreCase))
                {
                    return style;
                }
            }

            var normalized = styleName.Trim().ToLowerInvariant();
            if (normalized == "heading 1" || normalized == "Ð·Ð°Ð³Ð¾Ð»Ð¾Ð²Ð¾Ðº 1")
            {
                return doc.Styles["Heading 1"];
            }

            if (normalized == "heading 2" || normalized == "Ð·Ð°Ð³Ð¾Ð»Ð¾Ð²Ð¾Ðº 2")
            {
                return doc.Styles["Heading 2"];
            }

            if (normalized == "heading 3" || normalized == "Ð·Ð°Ð³Ð¾Ð»Ð¾Ð²Ð¾Ðº 3")
            {
                return doc.Styles["Heading 3"];
            }

            return null;
        }

        private static Word.Range FindHeadingRange(Word.Document doc, string headingText)
        {
            if (doc == null)
            {
                return null;
            }

            var search = (headingText ?? "").Trim();
            foreach (Word.Paragraph paragraph in doc.Paragraphs)
            {
                var level = (int)paragraph.OutlineLevel;
                if (level < 1 || level > 9)
                {
                    continue;
                }

                var text = paragraph.Range.Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(search) || text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return paragraph.Range.Duplicate;
                }
            }

            return null;
        }

        private static Word.WdParagraphAlignment ParseAlignment(string alignment)
        {
            var value = alignment.Trim().ToLowerInvariant();
            if (value.Contains("justify") || value.Contains("Ð¿Ð¾ ÑˆÐ¸Ñ€Ð¸Ð½Ðµ"))
            {
                return Word.WdParagraphAlignment.wdAlignParagraphJustify;
            }

            if (value.Contains("center") || value.Contains("Ñ†ÐµÐ½Ñ‚Ñ€"))
            {
                return Word.WdParagraphAlignment.wdAlignParagraphCenter;
            }

            if (value.Contains("right") || value.Contains("Ð¿Ñ€Ð°Ð²"))
            {
                return Word.WdParagraphAlignment.wdAlignParagraphRight;
            }

            return Word.WdParagraphAlignment.wdAlignParagraphLeft;
        }

        private static bool TryParseHighlight(string value, out Word.WdColorIndex colorIndex)
        {
            colorIndex = Word.WdColorIndex.wdNoHighlight;
            var key = (value ?? "").Trim().ToLowerInvariant();
            switch (key)
            {
                case "yellow":
                case "Ð¶ÐµÐ»Ñ‚Ñ‹Ð¹":
                    colorIndex = Word.WdColorIndex.wdYellow;
                    return true;
                case "green":
                case "Ð·ÐµÐ»ÐµÐ½Ñ‹Ð¹":
                    colorIndex = Word.WdColorIndex.wdBrightGreen;
                    return true;
                case "turquoise":
                case "Ð±Ð¸Ñ€ÑŽÐ·Ð¾Ð²Ñ‹Ð¹":
                    colorIndex = Word.WdColorIndex.wdTurquoise;
                    return true;
                case "pink":
                case "Ñ€Ð¾Ð·Ð¾Ð²Ñ‹Ð¹":
                    colorIndex = Word.WdColorIndex.wdPink;
                    return true;
                case "blue":
                case "ÑÐ¸Ð½Ð¸Ð¹":
                    colorIndex = Word.WdColorIndex.wdBlue;
                    return true;
                case "red":
                case "ÐºÑ€Ð°ÑÐ½Ñ‹Ð¹":
                    colorIndex = Word.WdColorIndex.wdRed;
                    return true;
                default:
                    return false;
            }
        }

        private static Color? TryParseColor(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            if (trimmed.StartsWith("#") && trimmed.Length == 7)
            {
                if (int.TryParse(trimmed.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
                {
                    var r = (rgb >> 16) & 0xFF;
                    var g = (rgb >> 8) & 0xFF;
                    var b = rgb & 0xFF;
                    return Color.FromArgb(r, g, b);
                }
            }

            try
            {
                return Color.FromName(trimmed);
            }
            catch
            {
                return null;
            }
        }
    }


}






