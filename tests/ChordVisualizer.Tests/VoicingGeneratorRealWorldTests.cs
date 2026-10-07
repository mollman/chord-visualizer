using ChordVisualizer.Core;
using ChordVisualizer.Core.Voicings;

namespace ChordVisualizer.Tests;

public class VoicingGeneratorRealWorldTests
{
    [Theory]
    [InlineData("C")]
    [InlineData("Am")]
    [InlineData("G7")]
    [InlineData("C/E")]
    public void GenerateVoicings_ForCommonUserInputs_ReturnsAtLeastOnePlayableVoicing(string chordText)
    {
        var chord = ChordSymbol.Parse(chordText);
        var generator = new VoicingGenerator();

        var voicings = generator.GenerateVoicings(chord);

        Assert.NotEmpty(voicings);
        Assert.All(voicings, voicing =>
        {
            Assert.Equal(chord.Text, voicing.Chord.Text);
            Assert.NotNull(voicing.Label);
            Assert.True(voicing.Positions.Count == 6, $"Expected 6-string voicing for {chordText}, but found {voicing.Positions.Count} positions.");
            Assert.Contains(voicing.Positions, p => p.State == StringPlayState.Fretted || p.State == StringPlayState.Open);
            var fretted = voicing.Positions
                .Where(p => p.State == StringPlayState.Fretted)
                .Select(p => p.Fret ?? 0)
                .ToList();

            if (fretted.Count > 0)
            {
                var min = fretted.Min();
                var max = fretted.Max();
                Assert.True(max - min <= generator.MaxFretSpan, $"Voicing span {max - min} exceeds configured max {generator.MaxFretSpan}.");
            }
        });
    }

    [Fact]
    public void GenerateVoicings_WhenMaxFretSpanIsTight_StillReturnsOnlyValidShapes()
    {
        var generator = new VoicingGenerator { MaxFretSpan = 2 };

        var voicings = generator.GenerateVoicings(ChordSymbol.Parse("C"));

        Assert.NotEmpty(voicings);
        Assert.All(voicings, voicing =>
        {
            var fretted = voicing.Positions
                .Where(p => p.State == StringPlayState.Fretted)
                .Select(p => p.Fret ?? 0)
                .ToList();

            Assert.NotEmpty(fretted);
            Assert.True(fretted.Max() - fretted.Min() <= generator.MaxFretSpan);
        });
    }

    [Fact]
    public void GenerateVoicings_ShouldNotDuplicateTheSameShapeAcrossVoicings()
    {
        var generator = new VoicingGenerator();
        var voicings = generator.GenerateVoicings(ChordSymbol.Parse("G"));

        var signatures = new HashSet<string>(StringComparer.Ordinal);

        foreach (var voicing in voicings)
        {
            var signature = string.Join("|",
                voicing.Positions
                    .OrderBy(p => p.StringIndex)
                    .Select(p => $"{p.StringIndex}:{p.State}:{p.Fret ?? -1}"));

            Assert.DoesNotContain(signature, signatures);
            signatures.Add(signature);
        }
    }

    [Fact]
    public void GenerateVoicings_ForC_IncludesTheCommonOpenPositionShape()
    {
        var generator = new VoicingGenerator();

        var voicings = generator.GenerateVoicings(ChordSymbol.Parse("C"));

        Assert.Contains(voicings, voicing =>
            voicing.Positions[0].State == StringPlayState.Muted &&
            voicing.Positions[1].State == StringPlayState.Fretted && voicing.Positions[1].Fret == 3 &&
            voicing.Positions[2].State == StringPlayState.Fretted && voicing.Positions[2].Fret == 2 &&
            voicing.Positions[3].State == StringPlayState.Open &&
            voicing.Positions[4].State == StringPlayState.Fretted && voicing.Positions[4].Fret == 1 &&
            voicing.Positions[5].State == StringPlayState.Open);
    }

    [Fact]
    public void GenerateVoicings_ForC_RepresentsOpenStringsAsOpenAndIncludesEveryChordTone()
    {
        var voicings = new VoicingGenerator().GenerateVoicings(ChordSymbol.Parse("C"));
        var chordTones = new[] { PitchClass.C, PitchClass.E, PitchClass.G };

        Assert.All(voicings, voicing =>
        {
            Assert.All(voicing.Positions.Where(p => p.State == StringPlayState.Fretted), position =>
                Assert.True(position.Fret > 0, "Open-string notes must use Open state, not Fretted at fret zero."));

            var soundedPitches = voicing.Positions
                .Where(position => position.State is StringPlayState.Open or StringPlayState.Fretted)
                .Select(position => FretboardPitchMap.GetPitchClass(
                    position.StringIndex,
                    position.State == StringPlayState.Open ? 0 : position.Fret!.Value))
                .Distinct()
                .ToHashSet();

            Assert.All(chordTones, tone => Assert.Contains(tone, soundedPitches));
        });
    }

    [Fact]
    public void Render_ForCommonOpenCShape_ShowsOpenMarkersAndFretsBelowNut()
    {
        var voicing = new VoicingGenerator()
            .GenerateVoicings(ChordSymbol.Parse("C"))
            .Single(candidate =>
                candidate.Positions[0].State == StringPlayState.Muted &&
                candidate.Positions[1].State == StringPlayState.Fretted && candidate.Positions[1].Fret == 3 &&
                candidate.Positions[2].State == StringPlayState.Fretted && candidate.Positions[2].Fret == 2 &&
                candidate.Positions[3].State == StringPlayState.Open &&
                candidate.Positions[4].State == StringPlayState.Fretted && candidate.Positions[4].Fret == 1 &&
                candidate.Positions[5].State == StringPlayState.Open);

        var diagramRows = ChordDiagramRenderer.Render(voicing)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("x | | o | o ", diagramRows[1]);
        Assert.Equal("| | | | ● | ", diagramRows[2]);
        Assert.Equal("| | ● | | | ", diagramRows[3]);
        Assert.Equal("| ● | | | | ", diagramRows[4]);
    }
}
