namespace CodeLearn.Models
{
    /// <summary>Дневной лимит AI-сообщений на пользователя.</summary>
    public class UserAiUsage
    {
        public int      Id           { get; set; }
        public int      UserId       { get; set; }
        public AppUser  User         { get; set; } = null!;
        public DateOnly Date         { get; set; }   // UTC-дата
        public int      MessageCount { get; set; } = 0;
    }
}
