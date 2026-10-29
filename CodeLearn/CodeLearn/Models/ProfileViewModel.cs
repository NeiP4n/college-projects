namespace CodeLearn.Models
{
    public class ProfileViewModel
    {
        public string NickName { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public int CurrentModule { get; set; }
        public int TotalDays { get; set; }
        public int Streak { get; set; }
    }
}
