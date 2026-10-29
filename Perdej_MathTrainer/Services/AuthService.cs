using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dapper;
using MathTrainer.Helpers;
using MathTrainer.Models;
using Microsoft.Data.SqlClient;

namespace MathTrainer.Services;

public partial class AuthService
{
    public (bool ok, string msg, User? user) Register(string username, string password)
    {
        username = username.Trim();

        if (!UsernameRegex().IsMatch(username))
            return (false, "Логин: 3-32 символа, буквы, цифры, _", null);

        if (password.Length < 6)
            return (false, "Пароль минимум 6 символов", null);

        try
        {
            using var conn = new SqlConnection(DbConfig.ConnectionString);
            var exists = conn.QueryFirstOrDefault<User>(
                "SELECT TOP 1 * FROM Users WHERE Username = @u", new { u = username });

            if (exists != null)
                return (false, "Пользователь уже существует", null);

            var salt = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            var hash = Hash(password, salt);
            var user = new User { Username = username, PasswordHash = hash, Salt = salt };

            conn.Execute(
                "INSERT INTO Users(Id,Username,PasswordHash,Salt,CreatedAt) " +
                "VALUES(@Id,@Username,@PasswordHash,@Salt,@CreatedAt)", user);

            return (true, "Регистрация успешна", user);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка БД: {ex.Message}", null);
        }
    }

    public (bool ok, string msg, User? user) Login(string username, string password)
    {
        try
        {
            using var conn = new SqlConnection(DbConfig.ConnectionString);
            var user = conn.QueryFirstOrDefault<User>(
                "SELECT TOP 1 * FROM Users WHERE Username = @u",
                new { u = username.Trim() });

            if (user == null || user.PasswordHash != Hash(password, user.Salt))
                return (false, "Неверный логин или пароль", null);

            return (true, "Успешный вход", user);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка БД: {ex.Message}", null);
        }
    }

    private static string Hash(string password, string salt)
    {
        var saltBytes = Convert.FromHexString(salt);
        return Convert.ToHexString(
            Rfc2898DeriveBytes.Pbkdf2(
                password, saltBytes, 600_000, HashAlgorithmName.SHA256, 32));
    }

    [GeneratedRegex(@"^[a-zA-Z0-9а-яА-Я_]{3,32}$")]
    private static partial Regex UsernameRegex();
}
