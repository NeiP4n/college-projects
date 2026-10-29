using Dapper;
using MathTrainer.Helpers;
using Microsoft.Data.SqlClient;

namespace MathTrainer.Services;

public static class DbInitializer
{
    public static void EnsureCreated()
    {
        try
        {
            CreateDatabase();
            CreateTables();
        }
        catch (Exception ex)
        {
            var msg =
                $"Ошибка подключения к базе данных:\n\n{ex.Message}\n\n" +
                "Убедитесь, что установлен Microsoft SQL Server LocalDB.\n" +
                "Скачать можно с: https://aka.ms/sqllocaldb\n\n" +
                $"Строка подключения:\n{DbConfig.ConnectionString}";

            MessageBox.Show(msg, "Ошибка запуска БД",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }

    private static void CreateDatabase()
    {
        using var master = new SqlConnection(DbConfig.MasterConnectionString);
        master.Execute($"""
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{DbConfig.DatabaseName}')
            BEGIN
                CREATE DATABASE [{DbConfig.DatabaseName}]
            END
        """);
    }

    private static void CreateTables()
    {
        using var conn = new SqlConnection(DbConfig.ConnectionString);

        conn.Execute("""
            IF NOT EXISTS (
                SELECT * FROM sys.objects
                WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type = 'U')
            BEGIN
                CREATE TABLE [dbo].[Users] (
                    [Id]           NVARCHAR(50)  NOT NULL PRIMARY KEY,
                    [Username]     NVARCHAR(100) NOT NULL,
                    [PasswordHash] NVARCHAR(500) NOT NULL,
                    [Salt]         NVARCHAR(100) NOT NULL,
                    [CreatedAt]    DATETIME2     NOT NULL,
                    CONSTRAINT [UQ_Users_Username] UNIQUE ([Username])
                )
            END
        """);

        conn.Execute("""
            IF NOT EXISTS (
                SELECT * FROM sys.objects
                WHERE object_id = OBJECT_ID(N'[dbo].[TestResults]') AND type = 'U')
            BEGIN
                CREATE TABLE [dbo].[TestResults] (
                    [Id]           NVARCHAR(50)   NOT NULL PRIMARY KEY,
                    [UserId]       NVARCHAR(50)   NOT NULL,
                    [Username]     NVARCHAR(100)  NOT NULL,
                    [TopicId]      NVARCHAR(100)  NOT NULL,
                    [TopicName]    NVARCHAR(200)  NOT NULL,
                    [CompletedAt]  DATETIME2      NOT NULL,
                    [CorrectCount] INT            NOT NULL,
                    [TotalCount]   INT            NOT NULL,
                    [AnswersJson]  NVARCHAR(MAX)  NOT NULL,
                    CONSTRAINT [FK_TestResults_Users]
                        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
                )
                CREATE INDEX [IX_TestResults_UserId]
                    ON [dbo].[TestResults] ([UserId])
                CREATE INDEX [IX_TestResults_CompletedAt]
                    ON [dbo].[TestResults] ([CompletedAt] DESC)
            END
        """);
    }
}
