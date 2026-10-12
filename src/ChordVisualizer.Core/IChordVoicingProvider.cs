using System;

namespace AppliedHyperkinetics.ChordVisualizer.Core;

public interface IChordVoicingProvider
{
    IReadOnlyList<ChordVoicing> GetVoicings(ChordSymbol chord);
}
