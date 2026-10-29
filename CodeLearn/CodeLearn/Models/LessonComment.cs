namespace CodeLearn.Models
{
    public class LessonComment
    {
        public int      Id        { get; set; }
        public int      LessonId  { get; set; }
        public Lesson   Lesson    { get; set; } = null!;
        public int      UserId    { get; set; }
        public AppUser  User      { get; set; } = null!;
        public string   Text      { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
