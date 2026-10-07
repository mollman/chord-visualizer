using ChordVisualizer.Core;
using ChordVisualizer.Core.Voicings;

namespace ChordVisualizer.Tests;

public class VoicingFamilyTests
{
    [Fact]
    public void GenerateVoicingFamilies_GroupsMuteVariantsAndKeepsDifferentBassNotesSeparate()
    {
        var generator = new VoicingGenerator();
        var chord = ChordSymbol.Parse("Emaj7");
        var generated = generator.GenerateVoicings(chord);
        var families = generator.GenerateVoicingFamilies(chord);
        var groupedVoicings = families
            .SelectMany(family => new[] { family.BestVoicing }.Concat(family.Alternatives))
            .ToList();

        Assert.Equal(generated.Count, groupedVoicings.Count);
        Assert.Equal(generated.Count, groupedVoicings.Distinct().Count());

        Assert.All(families, family =>
        {
            var members = new[] { family.BestVoicing }.Concat(family.Alternatives).ToList();
            var expectedFrettedNotes = GetFrettedSignature(family.BestVoicing);
            var expectedBass = GetBassPitch(family.BestVoicing);

            Assert.All(members, member =>
            {
                Assert.Equal(expectedFrettedNotes, GetFrettedSignature(member));
                Assert.Equal(expectedBass, GetBassPitch(member));
            });
        });

        var rootBassFamily = Assert.Single(families, family =>
            GetBassPitch(family.BestVoicing) == PitchClass.E &&
            GetFrettedSignature(family.BestVoicing) == "2:1,3:1");
        var rootBassMembers = new[] { rootBassFamily.BestVoicing }.Concat(rootBassFamily.Alternatives).ToList();

        Assert.NotEmpty(rootBassFamily.Alternatives);
        Assert.Equal(StringPlayState.Open, rootBassFamily.BestVoicing.Positions[0].State);
        Assert.Equal(StringPlayState.Open, rootBassFamily.BestVoicing.Positions[5].State);
        Assert.Contains(rootBassMembers, voicing =>
            voicing.Positions[0].State == StringPlayState.Open &&
            voicing.Positions[1].State == StringPlayState.Muted &&
            voicing.Positions[2].State == StringPlayState.Fretted &&
            voicing.Positions[3].State == StringPlayState.Fretted &&
            voicing.Positions[4].State == StringPlayState.Open &&
            voicing.Positions[5].State == StringPlayState.Open);
        Assert.Contains(rootBassMembers, voicing =>
            voicing.Positions[0].State == StringPlayState.Open &&
            voicing.Positions[1].State == StringPlayState.Muted &&
            voicing.Positions[2].State == StringPlayState.Fretted &&
            voicing.Positions[3].State == StringPlayState.Fretted &&
            voicing.Positions[4].State == StringPlayState.Open &&
            voicing.Positions[5].State == StringPlayState.Muted);
        Assert.Equal("2:1,3:1", GetFrettedSignature(rootBassFamily.BestVoicing));
        Assert.Equal(PitchClass.E, GetBassPitch(rootBassFamily.BestVoicing));

        Assert.Contains(families, family =>
            GetBassPitch(family.BestVoicing) == PitchClass.Ds &&
            GetFrettedSignature(family.BestVoicing) == "2:1,3:1");
    }

    [Fact]
    public void GenerateVoicingFamilies_FiltersByInclusiveLowestFrettedFretRange()
    {
        var generator = new VoicingGenerator();
        var chord = ChordSymbol.Parse("Emaj7");

        var allFamilies = generator.GenerateVoicingFamilies(chord);
        var filteredFamilies = generator.GenerateVoicingFamilies(chord, 7, 9);
        var expectedFamilies = allFamilies
            .Where(family => family.BestVoicing.DisplayStartFret is >= 7 and <= 9)
            .ToList();

        Assert.NotEmpty(filteredFamilies);
        Assert.Equal(
            expectedFamilies.Select(GetFamilySignature),
            filteredFamilies.Select(GetFamilySignature));
        Assert.All(filteredFamilies, family =>
            Assert.InRange(family.BestVoicing.DisplayStartFret, 7, 9));
    }

    [Fact]
    public void GenerateVoicingFamilies_RejectsInvalidLowestFretRange()
    {
        var generator = new VoicingGenerator();
        var chord = ChordSymbol.Parse("Emaj7");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            generator.GenerateVoicingFamilies(chord, 10, 9));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            generator.GenerateVoicingFamilies(chord, -1, 9));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            generator.GenerateVoicingFamilies(chord, 7, 23));
    }

    [Fact]
    public void GenerateVoicingFamilies_ConsecutiveTriadsUseThreeAdjacentStringsAndAllChordTones()
    {
        var chord = ChordSymbol.Parse("C");
        var families = new VoicingGenerator().GenerateVoicingFamilies(
            chord,
            onlyConsecutiveTriadVoicings: true);
        var chordTones = new HashSet<PitchClass>
        {
            PitchClass.C,
            PitchClass.E,
            PitchClass.G
        };

        Assert.NotEmpty(families);
        Assert.All(families.SelectMany(family => new[] { family.BestVoicing }.Concat(family.Alternatives)), voicing =>
        {
            var sounding = voicing.Positions
                .Where(position => position.State is StringPlayState.Open or StringPlayState.Fretted)
                .ToList();
            var soundedTones = sounding
                .Select(position => FretboardPitchMap.GetPitchClass(position.StringIndex, position.Fret!.Value))
                .ToHashSet();
            var stringIndices = sounding.Select(position => position.StringIndex).Order().ToList();

            Assert.Equal(3, sounding.Count);
            Assert.Equal(stringIndices[0] + 1, stringIndices[1]);
            Assert.Equal(stringIndices[1] + 1, stringIndices[2]);
            Assert.Equal(chordTones, soundedTones);
        });
        Assert.Contains(
            families.SelectMany(family => new[] { family.BestVoicing }.Concat(family.Alternatives)),
            voicing => voicing.Positions.Any(position => position.State == StringPlayState.Open));
    }

    [Fact]
    public void GenerateVoicingFamilies_ConsecutiveTriadsDoesNotReturnExtendedChordsAsTriads()
    {
        var families = new VoicingGenerator().GenerateVoicingFamilies(
            ChordSymbol.Parse("Emaj7"),
            onlyConsecutiveTriadVoicings: true);

        Assert.Empty(families);
    }

    private static string GetFrettedSignature(ChordVoicing voicing)
    {
        return string.Join(",", voicing.Positions
            .Where(position => position.State == StringPlayState.Fretted)
            .OrderBy(position => position.StringIndex)
            .Select(position => $"{position.StringIndex}:{position.Fret}"));
    }

    private static string GetFamilySignature(ChordVoicingFamily family)
    {
        return $"{GetFrettedSignature(family.BestVoicing)}|{GetBassPitch(family.BestVoicing)}|{family.Alternatives.Count}";
    }

    private static PitchClass GetBassPitch(ChordVoicing voicing)
    {
        var lowestSoundingPosition = voicing.Positions
            .Where(position => position.State is StringPlayState.Open or StringPlayState.Fretted)
            .MinBy(position => position.StringIndex)!;

        return FretboardPitchMap.GetPitchClass(
            lowestSoundingPosition.StringIndex,
            lowestSoundingPosition.Fret!.Value);
    }
}
