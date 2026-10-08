using System;

namespace ChordVisualizer.Core;

public sealed class ChordSymbol
{
    public PitchClass Root { get; }
    public ChordQuality Quality { get; }
    public PitchClass? BassOverride { get; }
    public string Text { get; }
    public string Suffix { get; }

    public ChordSymbol(PitchClass root, ChordQuality quality, PitchClass? bassOverride, string text, string? suffix = null)
    {
        Root = root;
        Quality = quality;
        BassOverride = bassOverride;
        Text = text;
        Suffix = suffix ?? GetDefaultSuffix(quality);
    }

    public static ChordSymbol Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Chord text cannot be empty.", nameof(value));
        }

        var normalized = value.Trim();
        var bassOverride = default(PitchClass?);
        string? bassOverrideText = null;

        if (normalized.Contains('/'))
        {
            var parts = normalized.Split('/', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                throw new ArgumentException("Slash chords must use the format Root/Note.", nameof(value));
            }

            bassOverrideText = parts[1];
            bassOverride = ParsePitchClass(parts[1]);
            normalized = parts[0];
        }

        var root = ParseRoot(normalized, out var remainder);
        var (quality, suffix) = ParseQuality(remainder);
        if (bassOverrideText is not null)
            suffix = $"{remainder.Trim()}/{bassOverrideText}";

        return new ChordSymbol(root, quality, bassOverride, value.Trim(), suffix);
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

    private static (ChordQuality Quality, string Suffix) ParseQuality(string value)
    {
        var normalized = value.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            return (ChordQuality.Major, "major");
        }

        if (normalized.Equals("m", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("min", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Minor, "minor");
        }

        if (normalized.Equals("maj", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Major, "major");
        }

        if (normalized.Equals("maj7", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Major7, "maj7");
        }

        if (normalized.Equals("m7", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("min7", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Minor7, "m7");
        }

        if (normalized.Equals("7", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("dom7", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Dominant7, "7");
        }

        if (normalized.Equals("add9", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Add9, "add9");
        }

        if (normalized.Equals("sus4", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Sus4, "sus4");
        }

        if (normalized.Equals("dom7", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Dominant7, "7");
        }

        if (normalized.Equals("dom9", StringComparison.OrdinalIgnoreCase))
        {
            return (ChordQuality.Other, "9");
        }

        return (ChordQuality.Other, normalized);
    }

    private static string GetDefaultSuffix(ChordQuality quality)
    {
        return quality switch
        {
            ChordQuality.Major => "major",
            ChordQuality.Minor => "minor",
            ChordQuality.Dominant7 => "7",
            ChordQuality.Major7 => "maj7",
            ChordQuality.Minor7 => "m7",
            ChordQuality.Add9 => "add9",
            ChordQuality.Sus4 => "sus4",
            _ => string.Empty
        };
    }

    public static PitchClass ParsePitchClass(string value)
    {
        var normalized = value.Trim();

        foreach (var token in RootTokens)
        {
            if (token.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                var pitchClassToken = token switch
                {
                    "Db" => "Cs",
                    "Eb" => "Ds",
                    "Gb" => "Fs",
                    "Ab" => "Gs",
                    "Bb" => "As",
                    _ => token.Replace("#", "s", StringComparison.Ordinal)
                };
                return Enum.Parse<PitchClass>(pitchClassToken, ignoreCase: true);
            }
        }

        throw new FormatException($"'{value}' is not a valid pitch class.");
    }

    private static readonly string[] RootTokens =
    {
        "C#", "Db",
        "D#", "Eb",
        "F#", "Gb",
        "G#", "Ab",
        "A#", "Bb",
        "C", "D", "E", "F", "G", "A", "B"
    };
}
