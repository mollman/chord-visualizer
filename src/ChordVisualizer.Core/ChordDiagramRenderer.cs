using System;
using System.Text;
using ChordVisualizer.Core.Voicings;

namespace ChordVisualizer.Core;

public static class ChordDiagramRenderer
{
    public static string Render(ChordVoicing voicing, bool showNoteNames = false)
    {
        var strings = 6;
        var frets = voicing.DisplayFretSpan;

        var grid = new string[frets + 2, strings];

        for (var fret = 0; fret <= frets + 1; fret++)
        {
            for (var stringIndex = 0; stringIndex < strings; stringIndex++)
            {
                grid[fret, stringIndex] = "|";
            }
        }

        foreach (var position in voicing.Positions)
        {
            if (position.State == StringPlayState.Muted)
            {
                grid[0, position.StringIndex] = "x";
            }
            else if (position.State == StringPlayState.Open)
            {
                grid[0, position.StringIndex] = "o";
            }
            else if (position.State == StringPlayState.Fretted && position.Fret.HasValue)
            {
                var fretRow = position.Fret.Value - voicing.DisplayStartFret + 1;
                if (fretRow >= 1 && fretRow <= frets + 1)
                {
                    grid[fretRow, position.StringIndex] = showNoteNames
                        ? GetPitchName(position.StringIndex, position.Fret.Value)
                        : position.Finger?.ToString() ?? "●";
                }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine($"{voicing.Chord.Text} ({voicing.Label})");

        for (var fret = 0; fret <= frets + 1; fret++)
        {
            for (var stringIndex = 0; stringIndex < strings; stringIndex++)
            {
                var cell = grid[fret, stringIndex];
                sb.Append(showNoteNames ? cell.PadLeft(2) : cell).Append(' ');
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetPitchName(int stringIndex, int fret)
    {
        return FretboardPitchMap.GetPitchClass(stringIndex, fret)
            .ToString()
            .Replace("s", "#", StringComparison.Ordinal);
    }
}
