namespace CodeLearn.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Description { get; set; } = "";
        public string Track { get; set; } = "BACKEND";
        public string Tag { get; set; } = "";
        public string Color { get; set; } = "#ffffff";
        public string State { get; set; } = "available";
        public string Badge { get; set; } = "START";
        public string Categories { get; set; } = "backend";
        public int EstimatedHours { get; set; } = 6;
        public int Order { get; set; }
        public bool IsPublished { get; set; } = true;

        public List<LearningModule> Modules { get; set; } = new();
    }
}
