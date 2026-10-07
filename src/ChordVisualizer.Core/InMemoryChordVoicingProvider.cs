using System;

namespace ChordVisualizer.Core;

public sealed class InMemoryChordVoicingProvider : IChordVoicingProvider
{
    public IReadOnlyList<ChordVoicing> GetVoicings(ChordSymbol chord)
    {
        return new[]
        {
            ChordVoicing.CreateVoicing(chord, chord.Quality switch
            {
                ChordQuality.Major => "Open major",
                ChordQuality.Minor => "Open minor",
                ChordQuality.Dominant7 => "Power 7",
                ChordQuality.Major7 => "Major 7",
                ChordQuality.Minor7 => "Minor 7",
                ChordQuality.Add9 => "Add9",
                ChordQuality.Sus4 => "Sus4",
                _ => "Custom"
            })
        };
    }
}
