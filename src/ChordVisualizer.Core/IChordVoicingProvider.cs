using System;

namespace ChordVisualizer.Core;

public interface IChordVoicingProvider
{
    IReadOnlyList<ChordVoicing> GetVoicings(ChordSymbol chord);
}
