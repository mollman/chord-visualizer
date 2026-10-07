using System;

namespace ChordVisualizer.Core;

public sealed class ChordVoicing
{
    public ChordSymbol Chord { get; }
    public string Label { get; }
    public IReadOnlyList<StringPosition> Positions { get; }
    public int DisplayStartFret { get; }
    public int DisplayFretSpan { get; }

    public ChordVoicing(
        ChordSymbol chord,
        string label,
        IEnumerable<StringPosition> positions,
        int displayStartFret,
        int displayFretSpan)
    {
        Chord = chord;
        Label = label;
        Positions = positions.ToList();
        DisplayStartFret = displayStartFret;
        DisplayFretSpan = displayFretSpan;
    }

    public static ChordVoicing CreateVoicing(ChordSymbol chord, string label)
    {
        var positions = chord.Quality switch
        {
            ChordQuality.Major => new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Muted, null, null),
                new StringPosition(2, StringPlayState.Open, 0, null),
                new StringPosition(3, StringPlayState.Fretted, 1, 1),
                new StringPosition(4, StringPlayState.Open, 0, null),
                new StringPosition(5, StringPlayState.Open, 0, null)
            },
            ChordQuality.Minor => new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Muted, null, null),
                new StringPosition(2, StringPlayState.Fretted, 1, 1),
                new StringPosition(3, StringPlayState.Fretted, 3, 2),
                new StringPosition(4, StringPlayState.Open, 0, null),
                new StringPosition(5, StringPlayState.Muted, null, null)
            },
            ChordQuality.Dominant7 => new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Muted, null, null),
                new StringPosition(2, StringPlayState.Fretted, 2, 2),
                new StringPosition(3, StringPlayState.Fretted, 3, 3),
                new StringPosition(4, StringPlayState.Fretted, 2, 1),
                new StringPosition(5, StringPlayState.Muted, null, null)
            },
            ChordQuality.Major7 => new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Muted, null, null),
                new StringPosition(2, StringPlayState.Fretted, 2, 2),
                new StringPosition(3, StringPlayState.Fretted, 1, 1),
                new StringPosition(4, StringPlayState.Open, 0, null),
                new StringPosition(5, StringPlayState.Open, 0, null)
            },
            ChordQuality.Minor7 => new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Muted, null, null),
                new StringPosition(2, StringPlayState.Fretted, 1, 1),
                new StringPosition(3, StringPlayState.Fretted, 3, 2),
                new StringPosition(4, StringPlayState.Open, 0, null),
                new StringPosition(5, StringPlayState.Muted, null, null)
            },
            ChordQuality.Add9 => new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Fretted, 1, 1),
                new StringPosition(2, StringPlayState.Open, 0, null),
                new StringPosition(3, StringPlayState.Fretted, 2, 2),
                new StringPosition(4, StringPlayState.Open, 0, null),
                new StringPosition(5, StringPlayState.Fretted, 3, 3)
            },
            ChordQuality.Sus4 => new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Muted, null, null),
                new StringPosition(2, StringPlayState.Fretted, 2, 1),
                new StringPosition(3, StringPlayState.Fretted, 3, 3),
                new StringPosition(4, StringPlayState.Open, 0, null),
                new StringPosition(5, StringPlayState.Muted, null, null)
            },
            _ => new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Muted, null, null),
                new StringPosition(2, StringPlayState.Fretted, 2, 1),
                new StringPosition(3, StringPlayState.Fretted, 2, 2),
                new StringPosition(4, StringPlayState.Fretted, 0, null),
                new StringPosition(5, StringPlayState.Muted, null, null)
            }
        };

        return new ChordVoicing(chord, label, positions, 0, 4);
    }
}
