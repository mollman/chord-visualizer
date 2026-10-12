using AppliedHyperkinetics.ChordVisualizer.Core.Theory;

namespace AppliedHyperkinetics.ChordVisualizer.Core.Voicings;

public static class ChordVoicingFilters
{
    public static bool MatchesTriad(
        ChordVoicing voicing,
        TriadInversion? inversion = null)
    {
        if (voicing.Chord.Quality == ChordQuality.Other)
            return false;

        var chordType = ChordTypeDictionary.Types[ChordQualityMapper.ToChordTypeKey(voicing.Chord.Quality)];
        var chordTones = chordType.Intervals
            .Select(interval => (PitchClass)(((int)voicing.Chord.Root + interval.Semitones) % 12))
            .Distinct()
            .ToHashSet();
        if (chordTones.Count != 3)
            return false;

        var sounding = voicing.Positions
            .Where(position => position.State is StringPlayState.Open or StringPlayState.Fretted)
            .OrderBy(position => position.StringIndex)
            .ToArray();
        if (sounding.Length != 3)
            return false;

        if (sounding[1].StringIndex != sounding[0].StringIndex + 1 ||
            sounding[2].StringIndex != sounding[1].StringIndex + 1)
        {
            return false;
        }

        var soundedTones = sounding
            .Select(position => FretboardPitchMap.GetPitchClass(position.StringIndex, position.Fret!.Value))
            .ToHashSet();
        if (!chordTones.SetEquals(soundedTones))
            return false;

        var bassPitch = FretboardPitchMap.GetPitchClass(sounding[0].StringIndex, sounding[0].Fret!.Value);
        if (voicing.Chord.BassOverride is { } bassOverride && bassPitch != bassOverride)
            return false;

        if (inversion is null)
            return true;

        var inversionToneIndex = inversion.Value switch
        {
            TriadInversion.RootPosition => 0,
            TriadInversion.FirstInversion => 1,
            TriadInversion.SecondInversion => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(inversion))
        };
        var bassTone = chordType.Intervals
            .Select(interval => (PitchClass)(((int)voicing.Chord.Root + interval.Semitones) % 12))
            .Distinct()
            .ElementAt(inversionToneIndex);

        return bassPitch == bassTone;
    }
}
