using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WordAIAgent.Services
{
    /// <summary>
    /// Manages a library of templates and reference documents.
    /// Templates are stored as file paths + metadata in a JSON sidecar.
    /// </summary>
    public class TemplateManager
    {
        public class TemplateEntry
        {
            public string Name { get; set; }
            public string FilePath { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public DateTime AddedAt { get; set; } = DateTime.Now;
            public TemplateType Type { get; set; } = TemplateType.Reference;
        }

        public enum TemplateType
        {
            /// <summary>Insert as content at cursor.</summary>
            Reference,
            /// <summary>Apply as Word template (.dotx / .docx styles).</summary>
            StyleTemplate,
            /// <summary>Load full text as AI context.</summary>
            ContextDocument
        }

        private readonly string _indexPath;
        private List<TemplateEntry> _entries = new List<TemplateEntry>();

        public TemplateManager()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WordAIAgent");
            Directory.CreateDirectory(dir);
            _indexPath = Path.Combine(dir, "templates.json");
            Load();
        }

        public IReadOnlyList<TemplateEntry> All => _entries.AsReadOnly();

        public IReadOnlyList<TemplateEntry> ByCategory(string category) =>
            _entries.Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase))
                    .ToList().AsReadOnly();

        // ── CRUD ─────────────────────────────────────────────────────────────

        public TemplateEntry Add(string filePath, string name = null,
            string description = null, string category = "Общие",
            TemplateType type = TemplateType.Reference)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден: " + filePath);

            // Remove duplicate by path
            _entries.RemoveAll(e => string.Equals(e.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

            var entry = new TemplateEntry
            {
                Name = name ?? Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath,
                Description = description ?? "",
                Category = category ?? "Общие",
                Type = type,
                AddedAt = DateTime.Now
            };
            _entries.Add(entry);
            Save();
            return entry;
        }

        public bool Remove(string nameOrPath)
        {
            var removed = _entries.RemoveAll(e =>
                string.Equals(e.Name, nameOrPath, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.FilePath, nameOrPath, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) Save();
            return removed > 0;
        }

        public TemplateEntry Find(string nameOrPath)
        {
            return _entries.FirstOrDefault(e =>
                string.Equals(e.Name, nameOrPath, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.FilePath, nameOrPath, StringComparison.OrdinalIgnoreCase) ||
                e.Name.IndexOf(nameOrPath, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // ── Text extraction for AI context ────────────────────────────────────

        /// <summary>
        /// Returns text content of a template for use as AI context.
        /// Truncated to maxChars if needed.
        /// </summary>
        public string GetContextText(TemplateEntry entry, int maxChars = 6000)
        {
            if (entry == null || !File.Exists(entry.FilePath))
                return "";

            string text;
            var ext = Path.GetExtension(entry.FilePath).ToLowerInvariant();
            if (ext == ".txt" || ext == ".md")
                text = File.ReadAllText(entry.FilePath);
            else
                text = ThisAddIn.WordService.ReadFileText(entry.FilePath);

            text = text ?? "";
            if (text.Length > maxChars)
                text = text.Substring(0, maxChars) + "\n...[обрезано]";
            return text;
        }

        /// <summary>
        /// Build a context block for all active (context-type) templates.
        /// </summary>
        public string BuildContextBlock(int maxTotalChars = 8000)
        {
            var contextEntries = _entries
                .Where(e => e.Type == TemplateType.ContextDocument && File.Exists(e.FilePath))
                .ToList();

            if (contextEntries.Count == 0) return "";

            var perEntry = maxTotalChars / Math.Max(1, contextEntries.Count);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("REFERENCE_DOCUMENTS:");

            foreach (var entry in contextEntries)
            {
                sb.AppendLine($"--- [{entry.Name}] ---");
                sb.AppendLine(GetContextText(entry, perEntry));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        // ── Actions ───────────────────────────────────────────────────────────

        public void ApplyToDocument(TemplateEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (!File.Exists(entry.FilePath))
                throw new FileNotFoundException("Файл шаблона не найден: " + entry.FilePath);

            switch (entry.Type)
            {
                case TemplateType.StyleTemplate:
                    ThisAddIn.WordService.ApplyTemplate(entry.FilePath);
                    break;
                case TemplateType.Reference:
                    ThisAddIn.WordService.InsertFileContent(entry.FilePath);
                    break;
                case TemplateType.ContextDocument:
                    // Context-only: just confirm loaded
                    break;
            }
        }

        // ── Persistence ───────────────────────────────────────────────────────

        private void Load()
        {
            try
            {
                if (File.Exists(_indexPath))
                    _entries = JsonConvert.DeserializeObject<List<TemplateEntry>>(
                        File.ReadAllText(_indexPath)) ?? new List<TemplateEntry>();
            }
            catch { _entries = new List<TemplateEntry>(); }

            // Remove entries for deleted files
            _entries = _entries.Where(e => File.Exists(e.FilePath)).ToList();
        }

        private void Save()
        {
            try
            {
                File.WriteAllText(_indexPath,
                    JsonConvert.SerializeObject(_entries, Formatting.Indented));
            }
            catch { }
        }

        // ── Category helpers ──────────────────────────────────────────────────

        public List<string> GetCategories() =>
            _entries.Select(e => e.Category).Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c).ToList();

        public string SummaryForPrompt()
        {
            if (_entries.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("LOADED_TEMPLATES:");
            foreach (var e in _entries)
                sb.AppendLine($"  [{e.Name}] ({e.Category}) — {e.Type} — {Path.GetFileName(e.FilePath)}");
            return sb.ToString();
        }
    }
}
