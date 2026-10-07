namespace ChordVisualizer.Core.Theory;

public static class IntervalParser
{
    private static readonly Dictionary<string, int> QualityOffsets = new()
    {
        { "P", 0 },  // perfect
        { "M", 0 },  // major
        { "m", -1 }, // minor
        { "A", +1 }, // augmented
        { "d", -1 }  // diminished
    };

    private static readonly Dictionary<int, int> PerfectBase = new()
    {
        { 1, 0 },  // unison
        { 4, 5 },  // fourth
        { 5, 7 },  // fifth
        { 8, 12 }, // octave
    };

    private static readonly Dictionary<int, int> MajorBase = new()
    {
        { 2, 2 },
        { 3, 4 },
        { 6, 9 },
        { 7, 11 },
        { 9, 14 },
        { 10, 16 },
        { 11, 17 },
        { 13, 21 }
    };

    public static Interval Parse(string name)
    {
        // Example: "3M", "7m", "9M"
        int number = int.Parse(new string(name.TakeWhile(char.IsDigit).ToArray()));
        string quality = new string(name.SkipWhile(char.IsDigit).ToArray());

        int baseSemitones =
            PerfectBase.TryGetValue(number, out var p)
                ? p
                : MajorBase[number];

        int offset = QualityOffsets[quality];

        return new Interval(name, baseSemitones + offset);
    }
}
