namespace CodeLearn.Models
{
    public class CourseSelectionViewModel
    {
        public string NickName { get; set; } = "";
        public int TotalModules { get; set; }
        public List<CourseCardViewModel> Courses { get; set; } = new();
    }

    public class CourseCardViewModel
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Track { get; set; } = "";
        public string Tag { get; set; } = "";
        public string Color { get; set; } = "#ffffff";
        public string State { get; set; } = "locked";
        public string Badge { get; set; } = "LOCK";
        public string Categories { get; set; } = "";
        public int LessonsCount { get; set; }
        public int ModulesCount { get; set; }
        public int EstimatedHours { get; set; }
        public int ProgressPercent { get; set; }
        public string Requirement { get; set; } = "";
        public bool IsAvailable => State == "available";
        public string Icon { get; set; } = "csharp";
    }
}
