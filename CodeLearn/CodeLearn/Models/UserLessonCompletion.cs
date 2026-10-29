namespace CodeLearn.Models
{
    public class UserLessonCompletion
    {
        public int      Id          { get; set; }
        public int      UserId      { get; set; }
        public AppUser  User        { get; set; } = null!;
        public int      LessonId    { get; set; }
        public Lesson   Lesson      { get; set; } = null!;
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
