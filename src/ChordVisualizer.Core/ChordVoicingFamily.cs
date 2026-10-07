namespace ChordVisualizer.Core;

public sealed class ChordVoicingFamily
{
    public ChordVoicing BestVoicing { get; }
    public IReadOnlyList<ChordVoicing> Alternatives { get; }

    public ChordVoicingFamily(ChordVoicing bestVoicing, IEnumerable<ChordVoicing> alternatives)
    {
        BestVoicing = bestVoicing;
        Alternatives = alternatives.ToList();
    }
}