using System.Collections.Generic;

namespace WordAIAgent.Models
{
    public class TemplateProfile
    {
        public string SourcePath { get; set; }
        public int Pages { get; set; }
        public List<string> Outline { get; set; } = new List<string>();
        public List<TableInfo> Tables { get; set; } = new List<TableInfo>();
        public List<string> Styles { get; set; } = new List<string>();
        public string Excerpt { get; set; }
    }
}
