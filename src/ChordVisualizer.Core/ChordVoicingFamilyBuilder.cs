namespace ChordVisualizer.Core;

public static class ChordVoicingFamilyBuilder
{
    public static IReadOnlyList<ChordVoicingFamily> Build(
        IEnumerable<ChordVoicing> voicings,
        IEnumerable<ChordVoicing>? preferredVoicings = null)
    {
        var seenVoicings = new HashSet<string>(StringComparer.Ordinal);
        var candidates = (preferredVoicings ?? [])
            .Concat(voicings)
            .Where(voicing => seenVoicings.Add(GetVoicingKey(voicing)))
            .ToList();

        return candidates
            .GroupBy(GetFamilyKey)
            .Select(group => new ChordVoicingFamily(group.First(), group.Skip(1)))
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
        var bassPitch = Voicings.FretboardPitchMap.GetPitchClass(
            lowestSoundingPosition.StringIndex,
            lowestSoundingPosition.Fret!.Value);

        return $"{frettedPattern}|{bassPitch}";
    }

    private static string GetVoicingKey(ChordVoicing voicing)
    {
        return string.Join(",", voicing.Positions
            .OrderBy(position => position.StringIndex)
            .Select(position => $"{position.StringIndex}:{position.State}:{position.Fret}"));
    }
}