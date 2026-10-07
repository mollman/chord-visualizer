namespace ChordVisualizer.Core.Theory;

public sealed class ChordType
{
    public string Name { get; }
    public string Alias { get; }
    public IReadOnlyList<string> IntervalNames { get; }
    public IReadOnlyList<Interval> Intervals { get; }

    public ChordType(string name, string alias, IEnumerable<string> intervalNames)
    {
        Name = name;
        Alias = alias;
        IntervalNames = intervalNames.ToList();
        Intervals = IntervalNames.Select(IntervalParser.Parse).ToList();
    }

    public override string ToString() => $"{Name} ({Alias})";
}
