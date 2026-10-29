namespace CodeLearn.Models
{
    public class LoginRecord
    {
        public int Id        { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser   { get; set; } = null!;
        public DateTime Date      { get; set; }
    }
}
