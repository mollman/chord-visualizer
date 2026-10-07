using System;

namespace ChordVisualizer.Core.Theory;

public static class ChordQualityMapper
{
    public static string ToChordTypeKey(ChordQuality quality)
    {
        return quality switch
        {
            ChordQuality.Major      => "major",
            ChordQuality.Minor      => "minor",
            ChordQuality.Dominant7  => "7",
            ChordQuality.Major7     => "maj7",
            ChordQuality.Minor7     => "m7",
            ChordQuality.Add9       => "add9",
            ChordQuality.Sus4       => "sus4",
            // extend as needed
            _ => "major"
        };
    }
}
