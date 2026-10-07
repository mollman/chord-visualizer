namespace ChordVisualizer.Core.Theory;

public static class ChordQualityResolver
{
    public static ChordType Resolve(string symbol)
    {
        // Examples:
        // "Cmaj7" → maj7
        // "Dm7"   → m7
        // "C9"    → 9
        // "Cadd9" → add9

        foreach (var kvp in ChordTypeDictionary.Types)
        {
            if (symbol.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;

            if (symbol.Contains(kvp.Value.Alias, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        // Default: major triad
        return ChordTypeDictionary.Types["major"];
    }
}
