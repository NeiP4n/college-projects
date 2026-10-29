namespace CodeLearn.Models
{
    public class Lesson
    {
        public int            Id       { get; set; }
        public int?            ModuleId { get; set; }           // nullable → SetNull при удалении модуля
        public LearningModule? Module   { get; set; }
        public string         Title    { get; set; } = "";
        public int            Order    { get; set; }
        public bool           IsLocked { get; set; } = true;

        public List<ContentBlock> ContentBlocks { get; set; } = new();
    }
}
