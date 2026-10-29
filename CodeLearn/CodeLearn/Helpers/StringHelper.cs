namespace CodeLearn;

public static class StringHelper
{
    /// <summary>
    /// Removes emoji and decorative symbols from a string, keeping Cyrillic/Latin text.
    /// Uses Rune enumeration to correctly handle surrogate pairs (U+1F000+).
    /// </summary>
    public static string StripEmoji(this string? s)
    {
        if (string.IsNullOrEmpty(s)) return s ?? "";
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (var rune in s.EnumerateRunes())
        {
            int v = rune.Value;
            bool isSymbolOrEmoji =
                (v >= 0x2194 && v <= 0x27BF) || // Misc arrows, technical, symbols, dingbats
                (v >= 0x2B00 && v <= 0x2BFF) || // Misc symbols and arrows extended
                (v >= 0xFE00 && v <= 0xFE0F) || // Variation selectors
                v == 0x200D                     || // Zero-width joiner
                (v >= 0x1F000);                   // All supplementary emoji (🎯🔒📚🐍 etc.)
            if (!isSymbolOrEmoji)
                sb.Append(rune.ToString());
        }
        return sb.ToString().Trim();
    }
}
