using ChordVisualizer.Core;
using ChordVisualizer.Core.Voicings;

namespace ChordVisualizer.Tests;

public class VoicingGeneratorSlashChordTests
{
    [Fact]
    public void GenerateVoicings_ForSlashChord_PreservesChordMetadataAndReturnsVoicings()
    {
        var chord = ChordSymbol.Parse("C/E");
        var generator = new VoicingGenerator();

        var voicings = generator.GenerateVoicings(chord);

        Assert.NotEmpty(voicings);
        Assert.All(voicings, voicing =>
        {
            Assert.Equal(chord.Text, voicing.Chord.Text);
            Assert.Equal(chord.Root, voicing.Chord.Root);
            Assert.Equal(chord.Quality, voicing.Chord.Quality);
            Assert.Equal(chord.BassOverride, voicing.Chord.BassOverride);
            Assert.Contains(voicing.Positions, p => p.State == StringPlayState.Fretted);
        });
    }

    [Fact]
    public void GenerateVoicings_ForSlashChord_UsesBassOverrideAsLowestSoundingPitch()
    {
        var chord = ChordSymbol.Parse("G/B");
        var voicings = new VoicingGenerator().GenerateVoicings(chord);

        Assert.NotEmpty(voicings);
        Assert.All(voicings, voicing =>
        {
            var lowestString = Assert.Single(voicing.Positions
                .Where(position => position.State is StringPlayState.Open or StringPlayState.Fretted)
                .OrderBy(position => position.StringIndex)
                .Take(1));
            var bassPitch = FretboardPitchMap.GetPitchClass(
                lowestString.StringIndex,
                lowestString.Fret!.Value);

            Assert.Equal(chord.BassOverride, bassPitch);
        });
    }
}
