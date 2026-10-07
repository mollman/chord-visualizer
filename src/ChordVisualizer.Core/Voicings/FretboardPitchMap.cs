namespace ChordVisualizer.Core.Voicings;

public static class FretboardPitchMap
{
    // Standard tuning: E2 A2 D3 G3 B3 E4
    private static readonly PitchClass[] OpenStrings =
    {
        PitchClass.E, // string 0 (low E)
        PitchClass.A,
        PitchClass.D,
        PitchClass.G,
        PitchClass.B,
        PitchClass.E  // string 5 (high e)
    };

    public static PitchClass GetPitchClass(int stringIndex, int fret)
    {
        int root = (int)OpenStrings[stringIndex];
        return (PitchClass)((root + fret) % 12);
    }
}
