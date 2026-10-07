namespace ChordVisualizer.Core.Voicings;

public static class PitchPositionFinder
{
    public static List<(int stringIndex, int fret)> FindPositions(
        PitchClass target,
        int maxFrets = 22)
    {
        var results = new List<(int, int)>();

        for (int s = 0; s < 6; s++)
        {
            for (int f = 0; f <= maxFrets; f++)
            {
                if (FretboardPitchMap.GetPitchClass(s, f) == target)
                    results.Add((s, f));
            }
        }

        return results;
    }
}
