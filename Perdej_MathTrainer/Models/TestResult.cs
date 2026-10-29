using System.Text.Json.Serialization;

namespace MathTrainer.Models;

public class TestResult
{
    public string   Id           { get; set; } = Guid.NewGuid().ToString();
    public string   UserId       { get; set; } = string.Empty;
    public string   Username     { get; set; } = string.Empty;
    public string   TopicId      { get; set; } = string.Empty;
    public string   TopicName    { get; set; } = string.Empty;
    public DateTime CompletedAt  { get; set; } = DateTime.UtcNow;
    public int      CorrectCount { get; set; }
    public int      TotalCount   { get; set; }
    public string   AnswersJson  { get; set; } = string.Empty;

    [JsonIgnore]
    public List<AnswerRecord> Answers { get; set; } = new();

    [JsonIgnore]
    public double Percentage =>
        TotalCount > 0 ? (double)CorrectCount / TotalCount * 100 : 0;

    [JsonIgnore]
    public string Grade =>
        Percentage >= 90 ? "5" :
        Percentage >= 75 ? "4" :
        Percentage >= 60 ? "3" : "2";
}
