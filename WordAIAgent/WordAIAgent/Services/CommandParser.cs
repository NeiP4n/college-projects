using System.Collections.Generic;
using System.Text.RegularExpressions;
using WordAIAgent.Models;

namespace WordAIAgent.Services
{
    public class CommandParseResult
    {
        public List<ParsedCommand> Commands { get; } = new List<ParsedCommand>();
        public List<string> UnknownCommands { get; } = new List<string>();
    }

    public static class CommandParser
    {
        private static readonly Regex ClosedCommandRegex = new Regex(
            @"\[CMD:(?<name>[A-Za-z0-9_]+)\](?<payload>[\s\S]*?)\[/CMD\]",
            RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex OpenCommandRegex = new Regex(
            @"\[CMD:(?<name>[A-Za-z0-9_]+)\](?<payload>[^\r\n]*?)(?=(\[CMD:|$))",
            RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static CommandParseResult Parse(string response)
        {
            var result = new CommandParseResult();
            if (string.IsNullOrWhiteSpace(response))
                return result;

            var matches = ClosedCommandRegex.Matches(response);
            foreach (Match match in matches)
            {
                var name = (match.Groups["name"].Value ?? string.Empty).Trim().ToUpperInvariant();
                var payload = match.Groups["payload"].Value?.Trim() ?? string.Empty;

                if (CommandCatalog.IsSupported(name))
                    result.Commands.Add(new ParsedCommand(name, payload));
                else if (!string.IsNullOrWhiteSpace(name))
                    result.UnknownCommands.Add(name);
            }

            var remaining = matches.Count > 0 ? ClosedCommandRegex.Replace(response, "\n") : response;
            var openMatches = OpenCommandRegex.Matches(remaining);
            foreach (Match match in openMatches)
            {
                var name = (match.Groups["name"].Value ?? string.Empty).Trim().ToUpperInvariant();
                var payload = match.Groups["payload"].Value ?? string.Empty;
                payload = Regex.Replace(payload, @"\[/CMD\]", "", RegexOptions.IgnoreCase).Trim();

                if (CommandCatalog.IsSupported(name))
                    result.Commands.Add(new ParsedCommand(name, payload));
                else if (!string.IsNullOrWhiteSpace(name))
                    result.UnknownCommands.Add(name);
            }

            return result;
        }

        public static string StripCommands(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return string.Empty;

            var withoutClosed = ClosedCommandRegex.Replace(response, "");
            var withoutOpen = OpenCommandRegex.Replace(withoutClosed, "");
            return withoutOpen.Trim();
        }
    }
}
