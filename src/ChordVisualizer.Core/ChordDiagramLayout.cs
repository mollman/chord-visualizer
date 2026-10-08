namespace ChordVisualizer.Core;

public enum ChordDiagramLineKind
{
    String,
    Fret,
    Nut
}

public enum ChordDiagramMarkerKind
{
    Muted,
    Open,
    Fretted
}

public sealed record ChordDiagramLine(
    double X1,
    double Y1,
    double X2,
    double Y2,
    ChordDiagramLineKind Kind);

public sealed record ChordDiagramMarker(
    int StringIndex,
    double X,
    double Y,
    double Radius,
    ChordDiagramMarkerKind Kind,
    string? Label,
    bool IsOnBarre);

public sealed record ChordDiagramBarre(
    int StartStringIndex,
    int EndStringIndex,
    double X,
    double Y,
    double Width,
    double Height,
    string Label);

public sealed class ChordDiagramLayout
{
    public double Width { get; }
    public double Height { get; }
    public string AccessibleLabel { get; }
    public string? FretLabel { get; }
    public double FretLabelX { get; }
    public double FretLabelY { get; }
    public IReadOnlyList<ChordDiagramLine> Lines { get; }
    public IReadOnlyList<ChordDiagramBarre> Barres { get; }
    public IReadOnlyList<ChordDiagramMarker> Markers { get; }

    internal ChordDiagramLayout(
        double width,
        double height,
        string accessibleLabel,
        string? fretLabel,
        double fretLabelX,
        double fretLabelY,
        IEnumerable<ChordDiagramLine> lines,
        IEnumerable<ChordDiagramBarre> barres,
        IEnumerable<ChordDiagramMarker> markers)
    {
        Width = width;
        Height = height;
        AccessibleLabel = accessibleLabel;
        FretLabel = fretLabel;
        FretLabelX = fretLabelX;
        FretLabelY = fretLabelY;
        Lines = lines.ToList();
        Barres = barres.ToList();
        Markers = markers.ToList();
    }
}

public static class ChordDiagramLayoutBuilder
{
    private const int StringCount = 6;
    private const double Left = 28;
    private const double Right = 16;
    private const double GridTop = 48;
    private const double Bottom = 16;
    private const double StringSpacing = 36;
    private const double FretSpacing = 36;
    private const double MarkerRadius = 11;

    public static ChordDiagramLayout Build(ChordVoicing voicing, bool showNoteNames = false)
    {
        var fretSpaceCount = Math.Max(1, voicing.DisplayFretSpan + 1);
        var firstDisplayedFret = Math.Max(1, voicing.DisplayStartFret);
        var width = Left + (StringCount - 1) * StringSpacing + Right;
        var height = GridTop + fretSpaceCount * FretSpacing + Bottom;
        var lines = new List<ChordDiagramLine>();

        for (var stringIndex = 0; stringIndex < StringCount; stringIndex++)
        {
            var x = Left + stringIndex * StringSpacing;
            lines.Add(new ChordDiagramLine(x, GridTop, x, GridTop + fretSpaceCount * FretSpacing, ChordDiagramLineKind.String));
        }

        for (var fretLine = 0; fretLine <= fretSpaceCount; fretLine++)
        {
            var y = GridTop + fretLine * FretSpacing;
            var kind = fretLine == 0 && voicing.DisplayStartFret <= 1
                ? ChordDiagramLineKind.Nut
                : ChordDiagramLineKind.Fret;
            lines.Add(new ChordDiagramLine(Left, y, Left + (StringCount - 1) * StringSpacing, y, kind));
        }

        var barres = voicing.Barres
            .Select(barre =>
            {
                var x1 = Left + barre.StartStringIndex * StringSpacing;
                var x2 = Left + barre.EndStringIndex * StringSpacing;
                var fretIndex = barre.Fret - firstDisplayedFret;
                var centerY = GridTop + (fretIndex + 0.5) * FretSpacing;
                return new ChordDiagramBarre(
                    barre.StartStringIndex,
                    barre.EndStringIndex,
                    x1 - MarkerRadius,
                    centerY - MarkerRadius,
                    x2 - x1 + MarkerRadius * 2,
                    MarkerRadius * 2,
                    showNoteNames ? string.Empty : barre.Finger.ToString());
            })
            .ToList();
        var barreStrings = voicing.Barres
            .SelectMany(barre => Enumerable.Range(barre.StartStringIndex, barre.EndStringIndex - barre.StartStringIndex + 1)
                .Select(stringIndex => (barre.Fret, stringIndex)))
            .ToHashSet();
        var markers = new List<ChordDiagramMarker>();

        foreach (var position in voicing.Positions.OrderBy(position => position.StringIndex))
        {
            if (position.State == StringPlayState.Fretted && position.Fret.HasValue)
            {
                var fretIndex = position.Fret.Value - firstDisplayedFret;
                if (fretIndex < 0 || fretIndex >= fretSpaceCount)
                    continue;

                var isOnBarre = barreStrings.Contains((position.Fret.Value, position.StringIndex));
                if (isOnBarre && !showNoteNames)
                    continue;

                markers.Add(new ChordDiagramMarker(
                    position.StringIndex,
                    Left + position.StringIndex * StringSpacing,
                    GridTop + (fretIndex + 0.5) * FretSpacing,
                    MarkerRadius,
                    ChordDiagramMarkerKind.Fretted,
                    showNoteNames
                        ? GetPitchName(position.StringIndex, position.Fret.Value)
                        : position.Finger?.ToString(),
                    isOnBarre));
                continue;
            }

            if (position.State is StringPlayState.Open or StringPlayState.Muted)
            {
                markers.Add(new ChordDiagramMarker(
                    position.StringIndex,
                    Left + position.StringIndex * StringSpacing,
                    GridTop - 18,
                    7,
                    position.State == StringPlayState.Open ? ChordDiagramMarkerKind.Open : ChordDiagramMarkerKind.Muted,
                    null,
                    false));
            }
        }

        return new ChordDiagramLayout(
            width,
            height,
            $"{voicing.Chord.Text}, {voicing.Label}",
            voicing.DisplayStartFret > 1 ? $"{voicing.DisplayStartFret}fr" : null,
            3,
            GridTop + 14,
            lines,
            barres,
            markers);
    }

    private static string GetPitchName(int stringIndex, int fret)
    {
        return Voicings.FretboardPitchMap.GetPitchClass(stringIndex, fret)
            .ToString()
            .Replace("s", "#", StringComparison.Ordinal);
    }
}