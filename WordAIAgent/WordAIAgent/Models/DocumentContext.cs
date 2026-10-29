using System;
using System.Collections.Generic;

namespace WordAIAgent.Models
{
    public class DocumentContext
    {
        public int Pages { get; set; }
        public int Tables { get; set; }
        public int Images { get; set; }
        public bool Headers { get; set; }
        public bool Footers { get; set; }
        public List<string> Styles { get; set; } = new List<string>();
        public List<string> Outline { get; set; } = new List<string>();
        public List<TableInfo> TableInfo { get; set; } = new List<TableInfo>();
        public SelectionContext Selection { get; set; } = new SelectionContext();
    }

    public class SelectionContext
    {
        public bool Empty { get; set; }
        public bool InTable { get; set; }
        public bool InHeader { get; set; }
        public bool InFooter { get; set; }
        public bool InShape { get; set; }
        public bool InTextBox { get; set; }
        public int Page { get; set; }
        public int ParagraphIndex { get; set; }
    }

    public class TableInfo
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
    }
}
