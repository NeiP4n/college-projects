namespace CodeLearn.Models
{
    // Type: "text" | "task" | "code" | "quiz" | "button"
    // Content: JSON строка, схема зависит от Type
    public class ContentBlock
    {
        public int    Id       { get; set; }
        public int?    LessonId { get; set; }       // nullable → SetNull при удалении урока
        public Lesson? Lesson   { get; set; }
        public string Type     { get; set; } = "text";
        public string Content  { get; set; } = "{}";
        public int    Order    { get; set; }
    }
}
