namespace ChordVisualizer.Core.Theory;

public static class ChordTypeDictionary
{
    public static readonly IReadOnlyDictionary<string, ChordType> Types =
        new Dictionary<string, ChordType>(StringComparer.OrdinalIgnoreCase)
        {
            { "major",    new("major", "M",  new[] { "1P", "3M", "5P" }) },
            { "minor",    new("minor", "m",  new[] { "1P", "3m", "5P" }) },
            { "aug",      new("augmented", "aug", new[] { "1P", "3M", "5A" }) },
            { "dim",      new("diminished", "dim", new[] { "1P", "3m", "5d" }) },

            // Seventh chords
            { "7",        new("dominant7", "7", new[] { "1P", "3M", "5P", "7m" }) },
            { "maj7",     new("major7", "maj7", new[] { "1P", "3M", "5P", "7M" }) },
            { "m7",       new("minor7", "m7", new[] { "1P", "3m", "5P", "7m" }) },
            { "mMaj7",    new("minorMajor7", "mMaj7", new[] { "1P", "3m", "5P", "7M" }) },
            { "dim7",     new("diminished7", "dim7", new[] { "1P", "3m", "5d", "7d" }) },
            { "m7b5",     new("halfDiminished", "ø", new[] { "1P", "3m", "5d", "7m" }) },

            // Extended chords
            { "9",        new("dominant9", "9", new[] { "1P", "3M", "5P", "7m", "9M" }) },
            { "maj9",     new("major9", "maj9", new[] { "1P", "3M", "5P", "7M", "9M" }) },
            { "m9",       new("minor9", "m9", new[] { "1P", "3m", "5P", "7m", "9M" }) },

            // Suspended
            { "sus2",     new("suspended2", "sus2", new[] { "1P", "2M", "5P" }) },
            { "sus4",     new("suspended4", "sus4", new[] { "1P", "4P", "5P" }) },

            // Add chords
            { "add9",     new("add9", "add9", new[] { "1P", "3M", "5P", "9M" }) },
            { "6",        new("sixth", "6", new[] { "1P", "3M", "5P", "6M" }) },
            { "m6",       new("minorSixth", "m6", new[] { "1P", "3m", "5P", "6M" }) },
        };
}
