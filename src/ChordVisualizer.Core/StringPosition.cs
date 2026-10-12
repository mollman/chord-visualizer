using System;

namespace AppliedHyperkinetics.ChordVisualizer.Core;

public sealed class StringPosition
{
    public int StringIndex { get; }
    public StringPlayState State { get; }
    public int? Fret { get; }
    public int? Finger { get; }

    public StringPosition(int stringIndex, StringPlayState state, int? fret, int? finger)
    {
        StringIndex = stringIndex;
        State = state;
        Fret = fret;
        Finger = finger;
    }
}
