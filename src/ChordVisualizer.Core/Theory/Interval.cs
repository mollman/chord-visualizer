namespace ChordVisualizer.Core.Theory;

public sealed class Interval
{
    public string Name { get; }
    public int Semitones { get; }

    public Interval(string name, int semitones)
    {
        Name = name;
        Semitones = semitones;
    }

    public override string ToString() => $"{Name} ({Semitones})";
}
