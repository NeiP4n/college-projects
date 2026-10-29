using System.Text.Json;
using Dapper;
using MathTrainer.Helpers;
using MathTrainer.Models;
using Microsoft.Data.SqlClient;

namespace MathTrainer.Services;

public class StatsService
{
    public void Save(TestResult r)
    {
        r.AnswersJson = JsonSerializer.Serialize(r.Answers);
        using var conn = new SqlConnection(DbConfig.ConnectionString);
        conn.Execute(
            "INSERT INTO TestResults(Id,UserId,Username,TopicId,TopicName,CompletedAt,CorrectCount,TotalCount,AnswersJson) " +
            "VALUES(@Id,@UserId,@Username,@TopicId,@TopicName,@CompletedAt,@CorrectCount,@TotalCount,@AnswersJson)", r);
    }

    public List<TestResult> Load(string userId)
    {
        using var conn = new SqlConnection(DbConfig.ConnectionString);
        var rows = conn.Query<TestResult>(
            "SELECT * FROM TestResults WHERE UserId = @u ORDER BY CompletedAt DESC",
            new { u = userId }).ToList();

        HydrateAnswers(rows);
        return rows;
    }

    public record TopicStat(string TopicName, double AvgPct, int Count, double BestPct);

    public List<TopicStat> GetTopicStats(string userId)
    {
        using var conn = new SqlConnection(DbConfig.ConnectionString);
        var rows = conn.Query<TestResult>(
            "SELECT * FROM TestResults WHERE UserId = @u", new { u = userId }).ToList();

        return rows
            .GroupBy(r => r.TopicName)
            .Select(g => new TopicStat(
                g.Key,
                g.Average(r => r.TotalCount > 0 ? (double)r.CorrectCount / r.TotalCount * 100 : 0),
                g.Count(),
                g.Max(r => r.TotalCount > 0 ? (double)r.CorrectCount / r.TotalCount * 100 : 0)))
            .OrderByDescending(s => s.AvgPct)
            .ToList();
    }

    private static void HydrateAnswers(List<TestResult> rows)
    {
        foreach (var r in rows)
            if (!string.IsNullOrEmpty(r.AnswersJson))
                r.Answers = JsonSerializer.Deserialize<List<AnswerRecord>>(r.AnswersJson) ?? new();
    }
}
