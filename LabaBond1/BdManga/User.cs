using BdManga;
public class User
{
    public string Login { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public User(string login, string password, string email)
    {
        Login = login;
        Password = password;
        Email = email;
    }
}
