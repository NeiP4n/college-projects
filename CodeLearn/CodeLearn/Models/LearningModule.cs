namespace CodeLearn.Models
{
    public class LearningModule
    {
        public int    Id          { get; set; }
        public int?   CourseId    { get; set; }
        public Course? Course     { get; set; }
        public string Title       { get; set; } = "";
        public string Description { get; set; } = "";
        public int    Order       { get; set; }
        public bool   IsLocked    { get; set; } = true;

        // Позиция, цвет и размер шрифта метки на карте
        public double MapX         { get; set; } = 50;
        public double MapY         { get; set; } = 50;
        public string Color        { get; set; } = "#ffffff";
        public int    LabelFontSize { get; set; } = 9;

        // Контрольная задача — отдельный тип узла на карте
        public bool   IsChallenge  { get; set; } = false;

        public List<Lesson> Lessons { get; set; } = new();
    }
}
