namespace MathTrainer.Models;

public class Question
{
    public string   Text         { get; set; } = string.Empty;
    public string[] Options      { get; set; } = new string[4];
    public int      CorrectIndex { get; set; }
    public string   Explanation  { get; set; } = string.Empty;
}
