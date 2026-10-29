namespace MathTrainer.Models;

public class User
{
    public string   Id           { get; set; } = Guid.NewGuid().ToString();
    public string   Username     { get; set; } = string.Empty;
    public string   PasswordHash { get; set; } = string.Empty;
    public string   Salt         { get; set; } = string.Empty;
    public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
}
