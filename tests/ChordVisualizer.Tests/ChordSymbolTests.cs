using ChordVisualizer.Core;

namespace ChordVisualizer.Tests;

public class ChordSymbolTests
{
    [Fact]
    public void ParseMajorChord_UsesExpectedRootAndQuality()
    {
        var chord = ChordSymbol.Parse("C");

        Assert.Equal(PitchClass.C, chord.Root);
        Assert.Equal(ChordQuality.Major, chord.Quality);
        Assert.Equal("C", chord.Text);
    }

    [Fact]
    public void ParseSlashChord_StoresBassOverride()
    {
        var chord = ChordSymbol.Parse("C/E");

        Assert.Equal(PitchClass.C, chord.Root);
        Assert.Equal(ChordQuality.Major, chord.Quality);
        Assert.Equal(PitchClass.E, chord.BassOverride);
        Assert.Equal("C/E", chord.Text);
    }
}
