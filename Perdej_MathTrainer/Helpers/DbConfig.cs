namespace MathTrainer.Helpers;

public static class DbConfig
{
    public static string ServerName { get; set; } = @"(localdb)\MSSQLLocalDB";
    public static string DatabaseName { get; set; } = "MathTrainer";

    public static string ConnectionString =>
        $"Server={ServerName};Database={DatabaseName};" +
        "Integrated Security=true;TrustServerCertificate=true;" +
        "Connection Timeout=30;MultipleActiveResultSets=true;";

    public static string MasterConnectionString =>
        $"Server={ServerName};Database=master;" +
        "Integrated Security=true;TrustServerCertificate=true;" +
        "Connection Timeout=30;";
}
