using AppliedHyperkinetics.ChordVisualizer.Core.Theory;

namespace AppliedHyperkinetics.ChordVisualizer.Core.Voicings;

public sealed class VoicingGenerator
{
    public int MaxFretSpan { get; set; } = 4;

    public IReadOnlyList<ChordVoicing> GenerateVoicings(
        ChordSymbol chord,
        bool onlyConsecutiveTriadVoicings = false,
        VoicingGenerationScope scope = VoicingGenerationScope.All,
        bool triadOnly = false,
        TriadInversion? inversion = null)
    {
        if (!Enum.IsDefined(scope))
            throw new ArgumentOutOfRangeException(nameof(scope));
        if (inversion is { } selectedInversion && !Enum.IsDefined(selectedInversion))
            throw new ArgumentOutOfRangeException(nameof(inversion));

        var typeKey = ChordQualityMapper.ToChordTypeKey(chord.Quality);
        var chordType = ChordTypeDictionary.Types[typeKey];
        var pitchClasses = chordType.Intervals
            .Select(i => (PitchClass)(((int)chord.Root + i.Semitones) % 12))
            .ToList();
        var chordToneClasses = pitchClasses.Distinct().ToHashSet();
        var requiresTriad = triadOnly || onlyConsecutiveTriadVoicings || inversion.HasValue;
        if (requiresTriad && chordToneClasses.Count != 3)
            return Array.Empty<ChordVoicing>();

        var inversionBass = inversion switch
        {
            TriadInversion.RootPosition => pitchClasses[0],
            TriadInversion.FirstInversion => pitchClasses[1],
            TriadInversion.SecondInversion => pitchClasses[2],
            _ => (PitchClass?)null
        };
        if (chord.BassOverride is { } requestedBass && inversionBass is { } triadBass && requestedBass != triadBass)
            return Array.Empty<ChordVoicing>();

        var candidatePitchClasses = pitchClasses.AsEnumerable();
        if (chord.BassOverride is { } bassOverride)
            candidatePitchClasses = candidatePitchClasses.Append(bassOverride);
        var distinctCandidatePitchClasses = candidatePitchClasses.Distinct().ToList();
        var choicesPerString = Enumerable.Range(0, 6)
            .Select(stringIndex => new List<StringPosition>
            {
                new(stringIndex, StringPlayState.Muted, null, null)
            })
            .ToArray();

        foreach (var pitchClass in distinctCandidatePitchClasses)
        {
            foreach (var (stringIndex, fret) in PitchPositionFinder.FindPositions(pitchClass))
            {
                var state = fret == 0 ? StringPlayState.Open : StringPlayState.Fretted;
                choicesPerString[stringIndex].Add(new StringPosition(stringIndex, state, fret, null));
            }
        }

        var remainingPitchClasses = new HashSet<PitchClass>[choicesPerString.Length + 1];
        remainingPitchClasses[^1] = [];
        for (var stringIndex = choicesPerString.Length - 1; stringIndex >= 0; stringIndex--)
        {
            remainingPitchClasses[stringIndex] = new HashSet<PitchClass>(remainingPitchClasses[stringIndex + 1]);
            foreach (var position in choicesPerString[stringIndex])
            {
                if (position.State is StringPlayState.Open or StringPlayState.Fretted)
                {
                    remainingPitchClasses[stringIndex].Add(
                        FretboardPitchMap.GetPitchClass(stringIndex, position.Fret!.Value));
                }
            }
        }

        var voicings = new List<ChordVoicing>();
        var combo = new List<StringPosition>(choicesPerString.Length);
        Search(0, 0, int.MaxValue, 0, new HashSet<PitchClass>());
        return voicings
            .OrderBy(CalculatePlayabilityScore)
            .ToList();

        void Search(
            int stringIndex,
            int soundingCount,
            int minimumFret,
            int maximumFret,
            HashSet<PitchClass> soundedPitchClasses)
        {
            if (stringIndex == choicesPerString.Length)
            {
                if (soundingCount < chordToneClasses.Count ||
                    !chordToneClasses.IsSubsetOf(soundedPitchClasses))
                    return;

                var voicing = new ChordVoicing(
                    chord,
                    CreateLabel(combo),
                    combo,
                    minimumFret == int.MaxValue ? 0 : minimumFret,
                    MaxFretSpan);
                if (requiresTriad &&
                    !ChordVoicingFilters.MatchesTriad(
                        voicing,
                        inversion))
                    return;
                if (scope == VoicingGenerationScope.Common && !IsCommonlyPlayable(combo))
                    return;

                voicings.Add(voicing);
                return;
            }

            foreach (var position in choicesPerString[stringIndex])
            {
                var nextSoundingCount = soundingCount;
                var nextMinimumFret = minimumFret;
                var nextMaximumFret = maximumFret;
                var nextPitchClasses = soundedPitchClasses;

                if (position.State is StringPlayState.Open or StringPlayState.Fretted)
                {
                    if (requiresTriad && soundingCount == 3)
                        continue;

                    var pitchClass = FretboardPitchMap.GetPitchClass(stringIndex, position.Fret!.Value);
                    if (soundingCount == 0 &&
                        ((chord.BassOverride is { } requiredBass && pitchClass != requiredBass) ||
                         (inversionBass is { } requiredInversionBass && pitchClass != requiredInversionBass)))
                        continue;

                    nextSoundingCount++;
                    nextPitchClasses = new HashSet<PitchClass>(soundedPitchClasses) { pitchClass };
                    if (position.State == StringPlayState.Fretted)
                    {
                        nextMinimumFret = Math.Min(minimumFret, position.Fret!.Value);
                        nextMaximumFret = Math.Max(maximumFret, position.Fret.Value);
                        if (nextMaximumFret - nextMinimumFret > MaxFretSpan)
                            continue;
                    }
                }

                if (chordToneClasses.Any(pitchClass =>
                        !nextPitchClasses.Contains(pitchClass) &&
                        !remainingPitchClasses[stringIndex + 1].Contains(pitchClass)))
                    continue;

                combo.Add(position);
                Search(
                    stringIndex + 1,
                    nextSoundingCount,
                    nextMinimumFret,
                    nextMaximumFret,
                    nextPitchClasses);
                combo.RemoveAt(combo.Count - 1);
            }
        }

        string CreateLabel(IReadOnlyList<StringPosition> positions)
        {
            var sounding = positions
                .Where(position => position.State is StringPlayState.Open or StringPlayState.Fretted)
                .ToArray();
            var fretted = sounding
                .Where(position => position.State == StringPlayState.Fretted)
                .Select(position => position.Fret!.Value)
                .ToArray();
            var minFret = fretted.Length > 0 ? fretted.Min() : 0;
            var maxFret = fretted.Length > 0 ? fretted.Max() : 0;

            return sounding.Any(position => position.State == StringPlayState.Open)
                ? minFret == 0 ? "Open position" : $"Open position, frets {minFret}-{maxFret}"
                : $"Fret {minFret}-{maxFret}";
        }
    }

    public IReadOnlyList<ChordVoicingFamily> GenerateVoicingFamilies(
        ChordSymbol chord,
        int minimumLowestFret = 0,
        int maximumLowestFret = 22,
        bool onlyConsecutiveTriadVoicings = false,
        VoicingGenerationScope scope = VoicingGenerationScope.All,
        bool triadOnly = false,
        TriadInversion? inversion = null)
    {
        if (minimumLowestFret < 0 || maximumLowestFret > 22 || minimumLowestFret > maximumLowestFret)
            throw new ArgumentOutOfRangeException(nameof(minimumLowestFret), "The lowest-fret range must be within 0-22 and ordered from minimum to maximum.");

        return ChordVoicingFamilyBuilder.Build(
                GenerateVoicings(
                    chord,
                    onlyConsecutiveTriadVoicings,
                    scope,
                    triadOnly,
                    inversion))
            .Where(family => family.BestVoicing.DisplayStartFret >= minimumLowestFret &&
                             family.BestVoicing.DisplayStartFret <= maximumLowestFret)
            .ToList();
    }

    private static bool IsCommonlyPlayable(IReadOnlyList<StringPosition> positions)
    {
        var fretted = positions
            .Where(position => position.State == StringPlayState.Fretted)
            .ToArray();
        if (fretted.Length <= 4)
            return true;

        for (var startString = 0; startString < positions.Count; startString++)
        {
            var start = positions[startString];
            if (start.State != StringPlayState.Fretted)
                continue;

            var barreFret = start.Fret!.Value;
            for (var endString = startString + 1; endString < positions.Count; endString++)
            {
                var end = positions[endString];
                if (end.State != StringPlayState.Fretted || end.Fret < barreFret)
                    break;

                var barreStringCount = endString - startString + 1;
                if (fretted.Length - (barreStringCount - 1) <= 4)
                    return true;
            }
        }

        return false;
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

}
