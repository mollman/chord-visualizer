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

    [Theory]
    [InlineData("C9", "9")]
    [InlineData("Cdom9", "9")]
    public void ParseDatabaseChord_PreservesItsSuffix(string text, string expectedSuffix)
    {
        var chord = ChordSymbol.Parse(text);

        Assert.Equal(ChordQuality.Other, chord.Quality);
        Assert.Equal(expectedSuffix, chord.Suffix);
    }
}
