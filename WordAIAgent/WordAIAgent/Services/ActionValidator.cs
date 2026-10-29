using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using WordAIAgent.Models;

namespace WordAIAgent.Services
{
    public class ActionValidator
    {
        public int MaxInsertLength { get; set; } = 15000;

        private readonly HashSet<string> _allowed;

        public ActionValidator(IEnumerable<string> allowedActions)
        {
            _allowed = new HashSet<string>(allowedActions ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }

        public ValidationResult Validate(List<(string action, JObject ps)> actions, DocumentContext ctx)
        {
            var result = new ValidationResult();
            if (actions == null || actions.Count == 0)
            {
                result.Errors.Add("Пустой план действий.");
                return result;
            }

            bool hasInsertTable = actions.Any(a => a.action == "InsertTable");
            bool needsCursorGuard = false;

            foreach (var (action, ps) in actions)
            {
                if (!_allowed.Contains(action))
                {
                    result.Errors.Add($"Недопустимое действие: {action}");
                    continue;
                }

                switch (action)
                {
                    case "InsertText":
                    case "InsertTextAtEnd":
                    case "AppendParagraph":
                        {
                            var text = S(ps, "text");
                            if (text == null) { result.Errors.Add("InsertText: отсутствует text."); break; }
                            if (MaxInsertLength > 0 && text.Length > MaxInsertLength)
                            {
                                ps["text"] = text.Substring(0, MaxInsertLength);
                                result.Warnings.Add($"InsertText: текст усечён до {MaxInsertLength} символов.");
                            }
                            needsCursorGuard = true;
                            break;
                        }
                    case "InsertTable":
                        {
                            int rows = I(ps, "rows", 2);
                            int cols = I(ps, "cols", I(ps, "columns", 2));
                            if (rows < 1 || rows > 50) { result.Errors.Add("InsertTable: rows вне диапазона 1..50."); break; }
                            if (cols < 1 || cols > 50) { result.Errors.Add("InsertTable: cols вне диапазона 1..50."); break; }
                            needsCursorGuard = true;
                            break;
                        }
                    case "FillTableCell":
                        {
                            int table = I(ps, "tableIndex", I(ps, "table", 1));
                            if (ctx != null && ctx.Tables > 0 && table > ctx.Tables)
                                result.Errors.Add($"FillTableCell: таблица {table} не существует.");
                            if (!hasInsertTable && (ctx == null || ctx.Tables == 0))
                                result.Errors.Add("FillTableCell: нет таблиц в документе.");
                            break;
                        }
                    case "GoToPage":
                        {
                            int page = I(ps, "page", 1);
                            if (page < 1) result.Errors.Add("GoToPage: page должен быть > 0.");
                            break;
                        }
                    case "SetFontSize":
                        {
                            float size = F(ps, "size", 12);
                            if (size < 6 || size > 72) result.Errors.Add("SetFontSize: размер вне 6..72.");
                            break;
                        }
                    case "SetLineSpacingMultiple":
                        {
                            float v = F(ps, "value", 1);
                            if (v < 0.8f || v > 3f) result.Errors.Add("SetLineSpacingMultiple: значение вне 0.8..3.0.");
                            break;
                        }
                    case "SetPageMarginsCm":
                        {
                            float top = F(ps, "top", 2);
                            float bottom = F(ps, "bottom", 2);
                            float left = F(ps, "left", 2);
                            float right = F(ps, "right", 2);
                            if (!IsMarginOk(top) || !IsMarginOk(bottom) || !IsMarginOk(left) || !IsMarginOk(right))
                                result.Errors.Add("SetPageMarginsCm: поля вне 0.5..10 см.");
                            break;
                        }
                    case "InsertChart":
                        {
                            int type = I(ps, "type", 5);
                            if (type < 1 || type > 100) result.Errors.Add("InsertChart: тип вне 1..100.");
                            needsCursorGuard = true;
                            break;
                        }
                    case "InsertPieChart":
                    case "InsertImage":
                    case "InsertImageFromClipboard":
                    case "InsertSmartArt":
                    case "InsertEquation":
                    case "InsertCaption":
                    case "InsertCitation":
                    case "InsertBibliography":
                    case "InsertTextBox":
                        needsCursorGuard = true;
                        break;
                    case "ExecuteVba":
                        result.Errors.Add("ExecuteVba запрещён.");
                        break;
                    case "ExecuteMso":
                        {
                            var id = S(ps, "controlId") ?? S(ps, "name");
                            if (!IsAllowedMso(id))
                                result.Errors.Add($"ExecuteMso: запрещённый controlId '{id}'.");
                            break;
                        }
                }
            }

            if (needsCursorGuard && ctx != null)
            {
                if (ctx.Selection != null &&
                    (ctx.Selection.InHeader || ctx.Selection.InFooter || ctx.Selection.InTable ||
                     ctx.Selection.InShape || ctx.Selection.InTextBox))
                {
                    result.Warnings.Add("CursorGuard: скорректирована позиция курсора.");
                    result.RequiresCursorGuard = true;
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        private static bool IsAllowedMso(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            var allow = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "InsertSmartArtGraphic","InsertEquation","InsertCaption","InsertCitation",
                "InsertBibliography","InsertChart"
            };
            return allow.Contains(id.Trim());
        }

        private static bool IsMarginOk(float v) => v >= 0.5f && v <= 10f;

        private static string S(JObject ps, string key) => ps?[key]?.ToString();
        private static int I(JObject ps, string key, int def)
        {
            var t = ps?[key];
            if (t == null) return def;
            return int.TryParse(t.ToString(), out int v) ? v : def;
        }
        private static float F(JObject ps, string key, float def)
        {
            var t = ps?[key];
            if (t == null) return def;
            return float.TryParse(t.ToString(), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : def;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public bool RequiresCursorGuard { get; set; }
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
    }
}
