namespace CodeLearn.Models
{
    public class UserLessonProgress
    {
        public int      Id                  { get; set; }
        public int      UserId              { get; set; }
        public AppUser  User                { get; set; } = null!;
        public int      LessonId            { get; set; }
        public Lesson   Lesson              { get; set; } = null!;
        public int      CompletedBlockCount { get; set; }
        public DateTime UpdatedAt           { get; set; } = DateTime.UtcNow;
    }
}
