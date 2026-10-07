using ChordVisualizer.Core;
using ChordVisualizer.Core.Voicings;

namespace ChordVisualizer.Tests;

public class VoicingGeneratorTests
{
    [Fact]
    public void GenerateVoicings_ForCommonChord_ReturnsUsableVoicings()
    {
        var chord = ChordSymbol.Parse("C");
        var generator = new VoicingGenerator();

        var voicings = generator.GenerateVoicings(chord);

        Assert.NotEmpty(voicings);
        Assert.All(voicings, voicing =>
        {
            Assert.Equal(chord.Text, voicing.Chord.Text);
            Assert.Equal(6, voicing.Positions.Count);
            Assert.NotNull(voicing.Label);
            Assert.Contains(voicing.Positions, p => p.State == StringPlayState.Fretted);
            Assert.True(voicing.DisplayFretSpan > 0);
        });
    }

    [Fact]
    public void GenerateVoicings_RespectsConfiguredMaxFretSpan()
    {
        var generator = new VoicingGenerator { MaxFretSpan = 2 };
        var voicings = generator.GenerateVoicings(ChordSymbol.Parse("Am"));

        Assert.NotEmpty(voicings);
        Assert.All(voicings, voicing =>
        {
            var fretted = voicing.Positions
                .Where(p => p.State == StringPlayState.Fretted)
                .Select(p => p.Fret ?? 0)
                .ToList();

            Assert.NotEmpty(fretted);
            var minFret = fretted.Min();
            var maxFret = fretted.Max();

            Assert.True(maxFret - minFret <= generator.MaxFretSpan,
                $"Expected fret span <= {generator.MaxFretSpan}, but found {maxFret - minFret} for {voicing.Chord.Text}.");
        });
    }

    [Fact]
    public void GenerateVoicings_ReturnsAllMatchesIncludingHigherFretPositions()
    {
        var voicings = new VoicingGenerator().GenerateVoicings(ChordSymbol.Parse("C"));

        Assert.True(voicings.Count > 20);
        Assert.Contains(voicings, voicing => voicing.DisplayStartFret > 1);
        Assert.All(voicings, voicing => Assert.Equal("C", voicing.Chord.Text));
    }
}
