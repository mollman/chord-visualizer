using ChordVisualizer.Core;
using ChordVisualizer.Core.Voicings;

namespace ChordVisualizer.Tests;

public class ChordDiagramRendererTests
{
    [Fact]
    public void Render_WhenShowingNoteNames_ReplacesFrettedMarkersWithPitchNames()
    {
        var voicing = GetOpenCVoicing();

        var rows = ChordDiagramRenderer.Render(voicing, showNoteNames: true)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(new[] { "|", "|", "|", "|", "C", "|" }, SplitCells(rows[2]));
        Assert.Equal(new[] { "|", "|", "E", "|", "|", "|" }, SplitCells(rows[3]));
        Assert.Equal(new[] { "|", "C", "|", "|", "|", "|" }, SplitCells(rows[4]));
    }

    [Fact]
    public void Render_WhenShowingNoteNames_UsesSharpPitchNames()
    {
        var chord = new ChordSymbol(PitchClass.Fs, ChordQuality.Major, null, "F#");
        var voicing = new ChordVoicing(
            chord,
            "Test",
            new[]
            {
                new StringPosition(0, StringPlayState.Fretted, 2, null),
                new StringPosition(1, StringPlayState.Muted, null, null),
                new StringPosition(2, StringPlayState.Muted, null, null),
                new StringPosition(3, StringPlayState.Muted, null, null),
                new StringPosition(4, StringPlayState.Muted, null, null),
                new StringPosition(5, StringPlayState.Muted, null, null)
            },
            2,
            4);

        var rows = ChordDiagramRenderer.Render(voicing, showNoteNames: true)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(new[] { "F#", "|", "|", "|", "|", "|" }, SplitCells(rows[2]));
    }

    [Fact]
    public void Render_WhenNoteNamesDisabled_KeepsDotMarkers()
    {
        var voicing = GetOpenCVoicing();

        var rows = ChordDiagramRenderer.Render(voicing)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("| | | | ● | ", rows[2]);
        Assert.Equal("| | ● | | | ", rows[3]);
        Assert.Equal("| ● | | | | ", rows[4]);
    }

    private static string[] SplitCells(string row)
    {
        return row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static ChordVoicing GetOpenCVoicing()
    {
        return new VoicingGenerator()
            .GenerateVoicings(ChordSymbol.Parse("C"))
            .Single(candidate =>
                candidate.Positions[0].State == StringPlayState.Muted &&
                candidate.Positions[1].State == StringPlayState.Fretted && candidate.Positions[1].Fret == 3 &&
                candidate.Positions[2].State == StringPlayState.Fretted && candidate.Positions[2].Fret == 2 &&
                candidate.Positions[3].State == StringPlayState.Open &&
                candidate.Positions[4].State == StringPlayState.Fretted && candidate.Positions[4].Fret == 1 &&
                candidate.Positions[5].State == StringPlayState.Open);
    }
}
