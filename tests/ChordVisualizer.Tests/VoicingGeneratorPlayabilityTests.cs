using ChordVisualizer.Core;
using ChordVisualizer.Core.Voicings;

namespace ChordVisualizer.Tests;

public class VoicingGeneratorPlayabilityTests
{
    [Theory]
    [InlineData("Fmaj7")]
    [InlineData("Dsus4")]
    [InlineData("G/B")]
    [InlineData("F7")]
    public void GenerateVoicings_ForExtendedOrSlashChords_ReturnsPlayableVoicings(string chordText)
    {
        var chord = ChordSymbol.Parse(chordText);
        var generator = new VoicingGenerator { MaxFretSpan = 4 };

        var voicings = generator.GenerateVoicings(chord);

        Assert.NotEmpty(voicings);
        Assert.All(voicings, voicing =>
        {
            Assert.Equal(chord.Text, voicing.Chord.Text);
            Assert.Equal(6, voicing.Positions.Count);

            var fretted = voicing.Positions
                .Where(p => p.State == StringPlayState.Fretted)
                .Select(p => p.Fret ?? 0)
                .ToList();

            Assert.Contains(voicing.Positions, position =>
                position.State is StringPlayState.Open or StringPlayState.Fretted);
            if (fretted.Count > 0)
            {
                Assert.True(fretted.Max() - fretted.Min() <= generator.MaxFretSpan,
                    $"Voicing for {chordText} exceeded max fret span.");
            }
        });
    }

    [Fact]
    public void GenerateVoicings_ShouldPreferLowerFretVoicingsFirst()
    {
        var generator = new VoicingGenerator();
        var voicings = generator.GenerateVoicings(ChordSymbol.Parse("C"));

        Assert.NotEmpty(voicings);

        var lowestFret = voicings.Min(v => v.DisplayStartFret);
        var bestVoicing = voicings.First();

        Assert.Equal(lowestFret, bestVoicing.DisplayStartFret);
        Assert.Contains(bestVoicing.Positions, p => p.State == StringPlayState.Open || p.State == StringPlayState.Fretted);
    }
}
