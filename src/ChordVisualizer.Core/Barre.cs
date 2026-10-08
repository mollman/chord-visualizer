namespace ChordVisualizer.Core;

public sealed record Barre
{
    public int Fret { get; }
    public int Finger { get; }
    public int StartStringIndex { get; }
    public int EndStringIndex { get; }

    public Barre(int fret, int finger, int startStringIndex, int endStringIndex)
    {
        if (fret < 1)
            throw new ArgumentOutOfRangeException(nameof(fret), "A barre must be played above the nut.");
        if (finger < 1)
            throw new ArgumentOutOfRangeException(nameof(finger), "A barre must use a fretting-hand finger.");
        if (startStringIndex < 0 || endStringIndex > 5 || startStringIndex >= endStringIndex)
            throw new ArgumentOutOfRangeException(nameof(startStringIndex), "A barre must cover at least two valid string indexes.");

        Fret = fret;
        Finger = finger;
        StartStringIndex = startStringIndex;
        EndStringIndex = endStringIndex;
    }
}