using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace OllamaAI
{
    public class KnowledgeTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Instruction { get; set; }
        public string OutputFormat { get; set; }
        public string Example { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }

        public string ToPromptBlock()
        {
            return
                "Название шаблона: " + (Name ?? "") + "\n" +
                "Категория: " + (Category ?? "") + "\n" +
                "Инструкция:\n" + (Instruction ?? "") + "\n\n" +
                "Формат вывода:\n" + (OutputFormat ?? "") + "\n\n" +
                "Пример:\n" + (Example ?? "");
        }

        public KnowledgeTemplate Clone()
        {
            return new KnowledgeTemplate
            {
                Id = Id,
                Name = Name,
                Category = Category,
                Instruction = Instruction,
                OutputFormat = OutputFormat,
                Example = Example,
                IsPinned = IsPinned,
                CreatedUtc = CreatedUtc,
                UpdatedUtc = UpdatedUtc
            };
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Category))
            {
                return Name ?? "";
            }

            return (Name ?? "") + " [" + Category + "]";
        }
    }

    public class KnowledgeBaseData
    {
        public List<KnowledgeTemplate> Templates { get; set; }

        public KnowledgeBaseData()
        {
            Templates = new List<KnowledgeTemplate>();
        }
    }

    public static class KnowledgeBaseStore
    {
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

        private static string FilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "OllamaAI.VSTO",
                    "knowledge-base.json");
            }
        }

        public static List<KnowledgeTemplate> Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    var defaults = BuildDefaults();
                    Save(defaults);
                    return defaults;
                }

                var text = File.ReadAllText(FilePath);
                var data = Json.Deserialize<KnowledgeBaseData>(text) ?? new KnowledgeBaseData();
                if (data.Templates == null)
                {
                    data.Templates = new List<KnowledgeTemplate>();
                }

                if (data.Templates.Count == 0)
                {
                    data.Templates = BuildDefaults();
                    Save(data.Templates);
                }

                return Sort(data.Templates);
            }
            catch
            {
                return BuildDefaults();
            }
        }

        public static void Save(List<KnowledgeTemplate> templates)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var normalized = templates ?? new List<KnowledgeTemplate>();
                foreach (var t in normalized)
                {
                    EnsureTemplateMeta(t);
                }

                var data = new KnowledgeBaseData { Templates = normalized };
                var text = Json.Serialize(data);
                File.WriteAllText(FilePath, text);
            }
            catch
            {
            }
        }

        public static void Upsert(KnowledgeTemplate template)
        {
            if (template == null)
            {
                return;
            }

            var all = Load();
            EnsureTemplateMeta(template);

            var existing = all.FirstOrDefault(x => x.Id == template.Id);
            if (existing == null)
            {
                all.Add(template.Clone());
            }
            else
            {
                existing.Name = template.Name;
                existing.Category = template.Category;
                existing.Instruction = template.Instruction;
                existing.OutputFormat = template.OutputFormat;
                existing.Example = template.Example;
                existing.IsPinned = template.IsPinned;
                existing.UpdatedUtc = DateTime.UtcNow;
            }

            Save(all);
        }

        public static void Delete(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return;
            }

            var all = Load();
            all.RemoveAll(x => x.Id == templateId);
            Save(all);
        }

        public static KnowledgeTemplate Create(string name, string category, string instruction, string outputFormat, string example)
        {
            var now = DateTime.UtcNow;
            return new KnowledgeTemplate
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = (name ?? "").Trim(),
                Category = (category ?? "").Trim(),
                Instruction = (instruction ?? "").Trim(),
                OutputFormat = (outputFormat ?? "").Trim(),
                Example = (example ?? "").Trim(),
                IsPinned = false,
                CreatedUtc = now,
                UpdatedUtc = now
            };
        }

        private static List<KnowledgeTemplate> Sort(List<KnowledgeTemplate> templates)
        {
            return templates
                .OrderByDescending(x => x.IsPinned)
                .ThenBy(x => x.Category ?? "", StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name ?? "", StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void EnsureTemplateMeta(KnowledgeTemplate template)
        {
            if (template == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(template.Id))
            {
                template.Id = Guid.NewGuid().ToString("N");
            }

            if (template.CreatedUtc == default(DateTime))
            {
                template.CreatedUtc = DateTime.UtcNow;
            }

            template.UpdatedUtc = DateTime.UtcNow;
        }

        private static List<KnowledgeTemplate> BuildDefaults()
        {
            return new List<KnowledgeTemplate>
            {
                Create(
                    "Официальное письмо",
                    "Бизнес",
                    "Напиши формальное письмо в вежливом, деловом стиле с четкими формулировками и запросом действия.",
                    "Вывод блоками: Тема, Вступление, Основные пункты (список), Заключение, Подпись.",
                    "Тема: Запрос на продление договора\nВступление: ..."),
                Create(
                    "Техническое задание",
                    "Инженерия",
                    "Сформируй ТЗ, готовое к реализации: требования должны быть измеримыми, с учетом edge-case сценариев.",
                    "Разделы: Цель, Область, Функциональные требования, Нефункциональные требования, Риски, Критерии приемки.",
                    "Цель: Разработать...\nОбласть: ..."),
                Create(
                    "Резюме для руководства",
                    "Аналитика",
                    "Сделай краткое резюме документа для принятия решения. Подчеркни результаты, риски и дальнейшие действия.",
                    "Максимум 8 пунктов + 3 конкретных следующих шага.",
                    "- Ключевой вывод 1\n- Ключевой вывод 2")
            };
        }
    }
}
