namespace MathTrainer.Models;

public class AnswerRecord
{
    public string QuestionText  { get; set; } = string.Empty;
    public string UserAnswer    { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool   IsCorrect     { get; set; }
    public string Explanation   { get; set; } = string.Empty;
}
