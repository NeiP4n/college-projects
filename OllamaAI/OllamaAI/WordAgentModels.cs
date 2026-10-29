using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Word = Microsoft.Office.Interop.Word;

namespace OllamaAI
{
    internal sealed class AgentAction
    {
        public string Tool { get; }
        public Dictionary<string, object> Params { get; }

        public AgentAction(string tool, Dictionary<string, object> parameters)
        {
            Tool = tool ?? "";
            Params = parameters ?? new Dictionary<string, object>();
        }

        public string GetString(string key, string fallback = "")
        {
            if (Params != null && Params.ContainsKey(key) && Params[key] != null)
            {
                return Params[key].ToString();
            }

            return fallback;
        }

        public int GetInt(string key, int fallback = 0)
        {
            if (Params != null && Params.ContainsKey(key) && Params[key] != null)
            {
                if (int.TryParse(Params[key].ToString(), out var value))
                {
                    return value;
                }

                if (double.TryParse(Params[key].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
                {
                    return (int)System.Math.Round(dbl);
                }
            }

            return fallback;
        }

        public double GetDouble(string key, double fallback = 0)
        {
            if (Params != null && Params.ContainsKey(key) && Params[key] != null)
            {
                if (double.TryParse(Params[key].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    return value;
                }
            }

            return fallback;
        }

        public bool GetBool(string key, bool fallback = false)
        {
            if (Params != null && Params.ContainsKey(key) && Params[key] != null)
            {
                if (bool.TryParse(Params[key].ToString(), out var value))
                {
                    return value;
                }

                var text = Params[key].ToString().Trim().ToLowerInvariant();
                if (text == "1" || text == "yes" || text == "да")
                {
                    return true;
                }

                if (text == "0" || text == "no" || text == "нет")
                {
                    return false;
                }
            }

            return fallback;
        }

        public bool? GetNullableBool(string key)
        {
            if (Params != null && Params.ContainsKey(key) && Params[key] != null)
            {
                if (bool.TryParse(Params[key].ToString(), out var value))
                {
                    return value;
                }

                var text = Params[key].ToString().Trim().ToLowerInvariant();
                if (text == "1" || text == "yes" || text == "да")
                {
                    return true;
                }

                if (text == "0" || text == "no" || text == "нет")
                {
                    return false;
                }
            }

            return null;
        }

        public List<List<string>> GetMatrix(string key)
        {
            if (Params == null || !Params.ContainsKey(key) || Params[key] == null)
            {
                return null;
            }

            var raw = Params[key] as ArrayList;
            if (raw == null)
            {
                return null;
            }

            var result = new List<List<string>>();
            foreach (var row in raw)
            {
                var rowList = row as ArrayList;
                if (rowList == null)
                {
                    continue;
                }

                var converted = new List<string>();
                foreach (var cell in rowList)
                {
                    converted.Add(cell != null ? cell.ToString() : "");
                }
                result.Add(converted);
            }

            return result;
        }
    }

    internal sealed class AgentToolResult
    {
        public string Tool { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public static AgentToolResult Ok(string tool, string message, Dictionary<string, object> data = null)
        {
            return new AgentToolResult { Tool = tool, Success = true, Message = message, Data = data ?? new Dictionary<string, object>() };
        }

        public static AgentToolResult Fail(string tool, string message)
        {
            return new AgentToolResult { Tool = tool, Success = false, Message = message, Data = new Dictionary<string, object>() };
        }
    }

    internal static class AgentResponseParser
    {
        public static bool TryParse(string content, out List<AgentAction> actions, out string finalNote)
        {
            actions = new List<AgentAction>();
            finalNote = "";
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            if (!TryExtractJson(content, out var json))
            {
                return false;
            }

            var serializer = new JavaScriptSerializer { MaxJsonLength = 1024 * 1024 };
            Dictionary<string, object> root = null;
            try
            {
                root = serializer.DeserializeObject(json) as Dictionary<string, object>;
            }
            catch
            {
                return false;
            }

            if (root == null)
            {
                return false;
            }

            if (root.ContainsKey("final") && root["final"] != null)
            {
                finalNote = root["final"].ToString();
            }

            if (!root.ContainsKey("actions") || root["actions"] == null)
            {
                return true;
            }

            var raw = root["actions"] as ArrayList;
            if (raw == null)
            {
                return true;
            }

            foreach (var entry in raw)
            {
                var dict = entry as Dictionary<string, object>;
                if (dict == null)
                {
                    continue;
                }

                var tool = dict.ContainsKey("tool") && dict["tool"] != null ? dict["tool"].ToString() : "";
                var paramsDict = dict.ContainsKey("params") ? dict["params"] as Dictionary<string, object> : null;
                actions.Add(new AgentAction(tool, paramsDict ?? new Dictionary<string, object>()));
            }

            return true;
        }

        private static bool TryExtractJson(string text, out string json)
        {
            json = "";
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start < 0 || end <= start)
            {
                return false;
            }

            json = text.Substring(start, end - start + 1);
            return true;
        }
    }

    internal sealed class DocumentSnapshot
    {
        public string Title { get; set; }
        public int WordCount { get; set; }
        public int ParagraphCount { get; set; }
        public int TableCount { get; set; }
        public int SectionCount { get; set; }
        public List<ParagraphInfo> Paragraphs { get; set; } = new List<ParagraphInfo>();
        public List<TableInfo> Tables { get; set; } = new List<TableInfo>();
        public List<HeadingInfo> Headings { get; set; } = new List<HeadingInfo>();
        public List<StyleInfo> Styles { get; set; } = new List<StyleInfo>();
        public List<SectionInfo> Sections { get; set; } = new List<SectionInfo>();
        public SelectionInfo Selection { get; set; }
    }

    internal sealed class ParagraphInfo
    {
        public int Index { get; set; }
        public string Text { get; set; }
        public string Style { get; set; }
        public int OutlineLevel { get; set; }
        public bool IsHeading { get; set; }
        public bool IsList { get; set; }
    }

    internal sealed class HeadingInfo
    {
        public int ParagraphIndex { get; set; }
        public string Text { get; set; }
        public int Level { get; set; }
    }

    internal sealed class TableInfo
    {
        public int Index { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string Preview { get; set; }
    }

    internal sealed class StyleInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    internal sealed class SectionInfo
    {
        public int Start { get; set; }
        public int End { get; set; }
        public string PageSetup { get; set; }
    }

    internal sealed class SelectionInfo
    {
        public bool HasSelection { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string Text { get; set; }
    }

    internal sealed class AgentMemoryEntry
    {
        public DateTime TimestampUtc { get; set; }
        public string UserPrompt { get; set; }
        public List<string> Tools { get; set; } = new List<string>();
        public string ResultSummary { get; set; }

        public static AgentMemoryEntry FromStep(string userPrompt, List<AgentAction> actions, List<AgentToolResult> results)
        {
            var entry = new AgentMemoryEntry
            {
                TimestampUtc = DateTime.UtcNow,
                UserPrompt = userPrompt ?? ""
            };

            if (actions != null)
            {
                foreach (var action in actions)
                {
                    if (action != null && !string.IsNullOrWhiteSpace(action.Tool))
                    {
                        entry.Tools.Add(action.Tool);
                    }
                }
            }

            if (results != null && results.Count > 0)
            {
                entry.ResultSummary = string.Join("; ", results.Select(r => r != null ? r.Message : ""));
            }

            return entry;
        }
    }

    internal sealed class AgentMemory
    {
        public string DocumentKey { get; set; }
        public List<AgentMemoryEntry> Entries { get; } = new List<AgentMemoryEntry>();

        public void AddEntry(AgentMemoryEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            Entries.Add(entry);
            if (Entries.Count > 20)
            {
                Entries.RemoveRange(0, Entries.Count - 20);
            }
        }

        public string BuildSummary(int maxEntries = 6)
        {
            if (Entries.Count == 0)
            {
                return "";
            }

            var start = Math.Max(0, Entries.Count - maxEntries);
            var sb = new StringBuilder();
            for (var i = start; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (entry == null)
                {
                    continue;
                }

                var when = entry.TimestampUtc.ToLocalTime().ToString("HH:mm");
                var tools = entry.Tools != null && entry.Tools.Count > 0
                    ? string.Join(", ", entry.Tools.Distinct())
                    : "нет";
                sb.AppendLine($"[{when}] {Trim(entry.UserPrompt, 80)} | tools: {tools} | result: {Trim(entry.ResultSummary, 120)}");
            }

            return sb.ToString().Trim();
        }

        private static string Trim(string text, int max)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            text = text.Trim();
            return text.Length <= max ? text : text.Substring(0, max) + "...";
        }
    }

    internal static class AgentMemoryStore
    {
        private static readonly Dictionary<string, AgentMemory> Memory = new Dictionary<string, AgentMemory>(StringComparer.OrdinalIgnoreCase);

        public static AgentMemory Get(Word.Document doc)
        {
            var key = BuildKey(doc);
            if (!Memory.TryGetValue(key, out var memory))
            {
                memory = new AgentMemory { DocumentKey = key };
                Memory[key] = memory;
            }

            return memory;
        }

        private static string BuildKey(Word.Document doc)
        {
            if (doc == null)
            {
                return "unknown";
            }

            var key = doc.FullName;
            if (string.IsNullOrWhiteSpace(key))
            {
                key = doc.Name ?? "unsaved";
            }

            return key;
        }
    }
}
