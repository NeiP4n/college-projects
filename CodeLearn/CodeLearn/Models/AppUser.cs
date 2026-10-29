using Microsoft.AspNetCore.Identity;

namespace CodeLearn.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string   GoogleId      { get; set; } = "";
        public string   NickName      { get; set; } = "";
        public string   AvatarUrl     { get; set; } = "";
        public int      CurrentModule { get; set; } = 0;
        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;

        public List<LoginRecord> LoginRecords { get; set; } = new();
    }
}
