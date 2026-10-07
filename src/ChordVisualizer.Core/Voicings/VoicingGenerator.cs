using ChordVisualizer.Core.Theory;

namespace ChordVisualizer.Core.Voicings;

public sealed class VoicingGenerator
{
    public int MaxFretSpan { get; set; } = 4;

    public IReadOnlyList<ChordVoicing> GenerateVoicings(ChordSymbol chord, bool onlyConsecutiveTriadVoicings = false)
    {
        // 1. Get chord type from quality
        var typeKey = ChordQualityMapper.ToChordTypeKey(chord.Quality);
        var chordType = ChordTypeDictionary.Types[typeKey];

        // 2. Convert intervals to pitch classes
        var pitchClasses = chordType.Intervals
            .Select(i => (PitchClass)(((int)chord.Root + i.Semitones) % 12))
            .ToList();
        var candidatePitchClasses = pitchClasses.AsEnumerable();
        if (chord.BassOverride is { } bassOverride)
            candidatePitchClasses = candidatePitchClasses.Append(bassOverride);
        var distinctCandidatePitchClasses = candidatePitchClasses.Distinct().ToList();

        // 3. Build choices per string (muted, open when it is a chord tone, and fretted notes)
        var choicesPerString = new List<List<StringPosition>>();

        for (int s = 0; s < 6; s++)
        {
            var positions = new List<StringPosition>
            {
                new StringPosition(s, StringPlayState.Muted, null, null)
            };

            foreach (var pc in distinctCandidatePitchClasses)
            {
                foreach (var (stringIndex, fret) in PitchPositionFinder.FindPositions(pc))
                {
                    if (stringIndex == s)
                    {
                        var state = fret == 0 ? StringPlayState.Open : StringPlayState.Fretted;
                        if (positions.Any(position => position.State == state && position.Fret == fret))
                            continue;

                        positions.Add(new StringPosition(
                            s,
                            state,
                            fret,
                            null));
                    }
                }
            }

            choicesPerString.Add(positions);
        }

        // 4. Generate combinations and filter by fret span
        var voicings = new List<ChordVoicing>();

        foreach (var combo in Cartesian(choicesPerString))
        {
            var sounding = combo
                .Where(position => position.State is StringPlayState.Open or StringPlayState.Fretted)
                .ToList();
            if (sounding.Count < pitchClasses.Distinct().Count())
                continue;

            var soundedPitchClasses = sounding
                .Select(position => FretboardPitchMap.GetPitchClass(
                    position.StringIndex,
                    position.Fret!.Value))
                .ToHashSet();
            if (!pitchClasses.All(soundedPitchClasses.Contains))
                continue;

            if (onlyConsecutiveTriadVoicings)
            {
                var soundingStringIndices = sounding
                    .Select(position => position.StringIndex)
                    .Order()
                    .ToList();
                if (soundingStringIndices.Count != 3 ||
                    soundingStringIndices[1] != soundingStringIndices[0] + 1 ||
                    soundingStringIndices[2] != soundingStringIndices[1] + 1)
                {
                    continue;
                }
            }

            if (chord.BassOverride.HasValue)
            {
                var lowestSoundingPosition = sounding.MinBy(position => position.StringIndex)!;
                var bassPitch = FretboardPitchMap.GetPitchClass(
                    lowestSoundingPosition.StringIndex,
                    lowestSoundingPosition.Fret!.Value);
                if (bassPitch != chord.BassOverride.Value)
                    continue;
            }

            var fretted = sounding
                .Where(position => position.State == StringPlayState.Fretted)
                .Select(position => position.Fret!.Value)
                .ToList();
            var minFret = fretted.Count > 0 ? fretted.Min() : 0;
            var maxFret = fretted.Count > 0 ? fretted.Max() : 0;

            if (maxFret - minFret > MaxFretSpan)
                continue;

            var label = sounding.Any(position => position.State == StringPlayState.Open)
                ? minFret == 0 ? "Open position" : $"Open position, frets {minFret}-{maxFret}"
                : $"Fret {minFret}-{maxFret}";
            var voicing = new ChordVoicing(
                chord,
                label,
                combo,
                minFret,
                MaxFretSpan);

            voicings.Add(voicing);
        }

        return voicings
            .OrderBy(v => CalculatePlayabilityScore(v))
            .ToList();
    }

    public IReadOnlyList<ChordVoicingFamily> GenerateVoicingFamilies(
        ChordSymbol chord,
        int minimumLowestFret = 0,
        int maximumLowestFret = 22,
        bool onlyConsecutiveTriadVoicings = false)
    {
        if (minimumLowestFret < 0 || maximumLowestFret > 22 || minimumLowestFret > maximumLowestFret)
            throw new ArgumentOutOfRangeException(nameof(minimumLowestFret), "The lowest-fret range must be within 0-22 and ordered from minimum to maximum.");

        return GenerateVoicings(chord, onlyConsecutiveTriadVoicings)
            .GroupBy(GetFamilyKey)
            .Select(group => new ChordVoicingFamily(group.First(), group.Skip(1)))
            .Where(family => family.BestVoicing.DisplayStartFret >= minimumLowestFret &&
                             family.BestVoicing.DisplayStartFret <= maximumLowestFret)
            .ToList();
    }

    private static string GetFamilyKey(ChordVoicing voicing)
    {
        var frettedPattern = string.Join(",", voicing.Positions
            .Where(position => position.State == StringPlayState.Fretted)
            .OrderBy(position => position.StringIndex)
            .Select(position => $"{position.StringIndex}:{position.Fret}"));
        var lowestSoundingPosition = voicing.Positions
            .Where(position => position.State is StringPlayState.Open or StringPlayState.Fretted)
            .MinBy(position => position.StringIndex)!;
        var bassPitch = FretboardPitchMap.GetPitchClass(
            lowestSoundingPosition.StringIndex,
            lowestSoundingPosition.Fret!.Value);

        return $"{frettedPattern}|{bassPitch}";
    }

    private static int CalculatePlayabilityScore(ChordVoicing voicing)
    {
        var fretted = voicing.Positions
            .Where(p => p.State == StringPlayState.Fretted)
            .Select(p => p.Fret ?? 0)
            .ToList();

        var openCount = voicing.Positions.Count(p => p.State == StringPlayState.Open);
        var mutedCount = voicing.Positions.Count(p => p.State == StringPlayState.Muted);
        var minFret = fretted.Count > 0 ? fretted.Min() : 0;
        var maxFret = fretted.Count > 0 ? fretted.Max() : 0;
        var fretSpan = maxFret - minFret;

        // Lower is better: lower starting fret, more open strings, shorter span, fewer muted strings.
        // This keeps the first voicing closest to the nut and easiest to play.
        return (minFret * 1000)
            + (fretSpan * 100)
            + (mutedCount * 10)
            - (openCount * 5);
    }

    private static IEnumerable<List<T>> Cartesian<T>(List<List<T>> lists)
    {
        IEnumerable<List<T>> result = new List<List<T>> { new List<T>() };

        foreach (var list in lists)
        {
            result =
                from r in result
                from item in list
                select new List<T>(r) { item };
        }

        return result;
    }
}

