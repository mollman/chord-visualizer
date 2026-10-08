using ChordVisualizer.Core;
using System.Text.Json;

namespace ChordVisualizer.Tests;

public class ChordsDbChordVoicingProviderTests
{
    [Fact]
    public void GetVoicings_ReturnsAllCuratedC9Positions()
    {
        using var provider = new ChordsDbChordVoicingProvider();

        var voicings = provider.GetVoicings(ChordSymbol.Parse("C9"));

        Assert.Equal(5, voicings.Count);
        Assert.Equal("Position 1", voicings[0].Label);
        Assert.Equal(StringPlayState.Open, voicings[0].Positions[0].State);
        Assert.Equal(3, voicings[0].Positions[1].Fret);
        Assert.Equal(2, voicings[0].Positions[1].Finger);
    }

    [Fact]
    public void GetVoicings_MapsDatabaseBarresToStringRanges()
    {
        using var provider = new ChordsDbChordVoicingProvider();

        var voicings = provider.GetVoicings(ChordSymbol.Parse("C9"));
        var barreShape = voicings[1];

        Assert.Equal(2, barreShape.Barres.Count);
        Assert.Contains(barreShape.Barres, barre =>
            barre.Fret == 3 && barre.Finger == 2 && barre.StartStringIndex == 0 && barre.EndStringIndex == 1);
        Assert.Contains(barreShape.Barres, barre =>
            barre.Fret == 3 && barre.Finger == 3 && barre.StartStringIndex == 3 && barre.EndStringIndex == 4);
    }

    [Fact]
    public void GetVoicings_ReturnsNoPositionsForUnknownSuffix()
    {
        using var provider = new ChordsDbChordVoicingProvider();

        Assert.Empty(provider.GetVoicings(ChordSymbol.Parse("Cunknown")));
    }

    [Fact]
    public void GetVoicings_ResolvesEverySnapshotRootAndSuffix()
    {
        using var provider = new ChordsDbChordVoicingProvider();
        using var stream = typeof(ChordsDbChordVoicingProvider).Assembly
            .GetManifestResourceStream("ChordVisualizer.Core.Data.chords-db-guitar.json")!;
        using var database = JsonDocument.Parse(stream);
        var chordRoots = database.RootElement.GetProperty("chords");
        var chordCount = 0;

        foreach (var root in chordRoots.EnumerateObject())
        {
            foreach (var chordRecord in root.Value.EnumerateArray())
            {
                var suffix = chordRecord.GetProperty("suffix").GetString();
                var inputRoot = root.Name.Replace("sharp", "#", StringComparison.OrdinalIgnoreCase);
                var chord = ChordSymbol.Parse(inputRoot + suffix);
                var voicings = provider.GetVoicings(chord);

                Assert.True(voicings.Count > 0, $"No voicings resolved for {root.Name}{suffix} (parsed suffix: {chord.Suffix}).");
                chordCount++;
            }
        }

        Assert.True(chordCount > 0);
    }
}