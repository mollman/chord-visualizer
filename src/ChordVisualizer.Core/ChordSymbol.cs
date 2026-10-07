using System;

namespace ChordVisualizer.Core;

public sealed class ChordSymbol
{
    public PitchClass Root { get; }
    public ChordQuality Quality { get; }
    public PitchClass? BassOverride { get; }
    public string Text { get; }

    public ChordSymbol(PitchClass root, ChordQuality quality, PitchClass? bassOverride, string text)
    {
        Root = root;
        Quality = quality;
        BassOverride = bassOverride;
        Text = text;
    }

    public static ChordSymbol Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Chord text cannot be empty.", nameof(value));
        }

        var normalized = value.Trim();
        var bassOverride = default(PitchClass?);

        if (normalized.Contains('/'))
        {
            var parts = normalized.Split('/', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                throw new ArgumentException("Slash chords must use the format Root/Note.", nameof(value));
            }

            bassOverride = ParsePitchClass(parts[1]);
            normalized = parts[0];
        }

        var root = ParseRoot(normalized, out var remainder);
        var quality = ParseQuality(remainder);

        return new ChordSymbol(root, quality, bassOverride, value.Trim());
    }

    private static PitchClass ParseRoot(string value, out string remainder)
    {
        foreach (var token in RootTokens)
        {
            if (value.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            {
                remainder = value.Substring(token.Length);
                return ParsePitchClass(token);
            }
        }

        throw new FormatException($"'{value}' is not a valid chord root.");
    }

    private static ChordQuality ParseQuality(string value)
    {
        var normalized = value.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            return ChordQuality.Major;
        }

        if (normalized.Equals("m", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("min", StringComparison.OrdinalIgnoreCase))
        {
            return ChordQuality.Minor;
        }

        if (normalized.Equals("maj7", StringComparison.OrdinalIgnoreCase))
        {
            return ChordQuality.Major7;
        }

        if (normalized.Equals("m7", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("min7", StringComparison.OrdinalIgnoreCase))
        {
            return ChordQuality.Minor7;
        }

        if (normalized.Equals("7", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("dom7", StringComparison.OrdinalIgnoreCase))
        {
            return ChordQuality.Dominant7;
        }

        if (normalized.Equals("add9", StringComparison.OrdinalIgnoreCase))
        {
            return ChordQuality.Add9;
        }

        if (normalized.Equals("sus4", StringComparison.OrdinalIgnoreCase))
        {
            return ChordQuality.Sus4;
        }

        throw new FormatException($"Unsupported chord quality '{normalized}'.");
    }

    public static PitchClass ParsePitchClass(string value)
    {
        var normalized = value.Trim();

        foreach (var token in RootTokens)
        {
            if (token.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.Parse<PitchClass>(token.Replace("#", "s"), ignoreCase: true);
            }
        }

        throw new FormatException($"'{value}' is not a valid pitch class.");
    }

    private static readonly string[] RootTokens =
    {
        "C", "C#", "Db",
        "D", "D#", "Eb",
        "E",
        "F", "F#", "Gb",
        "G", "G#", "Ab",
        "A", "A#", "Bb",
        "B"
    };
}
