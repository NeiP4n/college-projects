namespace WordAIAgent.Models
{
    public class ParsedCommand
    {
        public string Name { get; }
        public string Payload { get; }

        public ParsedCommand(string name, string payload)
        {
            Name = name;
            Payload = payload ?? string.Empty;
        }
    }
}
