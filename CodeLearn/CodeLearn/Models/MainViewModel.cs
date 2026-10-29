namespace CodeLearn.Models
{
    public class MainViewModel
    {
        public string NickName { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public int Streak { get; set; }
        public int TotalDays { get; set; }
        public int CurrentModule { get; set; }
        public int TotalErrors { get; set; }
        public List<LearningModule> Modules { get; set; } = new();
    }
}
