using System.Xml.Linq;
using ChordVisualizer.Core;

namespace ChordVisualizer.Tests;

public class ChordDiagramLayoutTests
{
    [Fact]
    public void Build_RepresentsBarreAsOneSpanInsteadOfRepeatedMarkers()
    {
        var layout = ChordDiagramLayoutBuilder.Build(GetBarreVoicing());

        var barre = Assert.Single(layout.Barres);
        Assert.Equal(1, barre.StartStringIndex);
        Assert.Equal(3, barre.EndStringIndex);
        Assert.Equal("1", barre.Label);
        Assert.DoesNotContain(layout.Markers, marker => marker.IsOnBarre);
    }

    [Fact]
    public void Build_WhenShowingNoteNames_LabelsEachNoteUnderBarre()
    {
        var layout = ChordDiagramLayoutBuilder.Build(GetBarreVoicing(), showNoteNames: true);

        Assert.Equal(3, layout.Markers.Count(marker => marker.IsOnBarre));
        Assert.All(layout.Markers.Where(marker => marker.IsOnBarre), marker => Assert.False(string.IsNullOrEmpty(marker.Label)));
        Assert.Equal(string.Empty, Assert.Single(layout.Barres).Label);
    }

    [Fact]
    public void Build_UsesDisplayedFretForBarreCoordinates()
    {
        var layout = ChordDiagramLayoutBuilder.Build(GetBarreVoicing());

        Assert.Equal(66, layout.Barres[0].Y + layout.Barres[0].Height / 2);
        Assert.Equal("3fr", layout.FretLabel);
    }

    [Fact]
    public void Render_ProducesValidSvgAndEscapesAccessibleLabels()
    {
        var voicing = GetBarreVoicing(chordText: "F & G", label: "Shape <one>");
        var markup = SvgChordDiagramRenderer.Render(voicing);
        XNamespace svgNamespace = "http://www.w3.org/2000/svg";
        var svg = XElement.Parse(markup);

        Assert.Equal("F & G, Shape <one>", (string?)svg.Attribute("aria-label"));
        Assert.Equal("F & G, Shape <one>", svg.Element(svgNamespace + "title")?.Value);
        Assert.Single(svg.Elements(svgNamespace + "rect"));
    }

    [Fact]
    public void ChordVoicing_RejectsBarreThatDoesNotMatchPositions()
    {
        var positions = GetBarreVoicing().Positions;

        Assert.Throws<ArgumentException>(() => new ChordVoicing(
            ChordSymbol.Parse("F"),
            "Invalid barre",
            positions,
            3,
            4,
            new[] { new Barre(4, 1, 1, 3) }));
    }

    [Fact]
    public void ChordVoicing_AllowsHigherFrettedNotesUnderFullBarre()
    {
        var chord = ChordSymbol.Parse("F");
        var positions = new[]
        {
            new StringPosition(0, StringPlayState.Fretted, 1, 1),
            new StringPosition(1, StringPlayState.Fretted, 3, 3),
            new StringPosition(2, StringPlayState.Fretted, 3, 4),
            new StringPosition(3, StringPlayState.Fretted, 2, 2),
            new StringPosition(4, StringPlayState.Fretted, 1, 1),
            new StringPosition(5, StringPlayState.Fretted, 1, 1)
        };

        var voicing = new ChordVoicing(
            chord,
            "Full F barre",
            positions,
            1,
            4,
            new[] { new Barre(1, 1, 0, 5) });

        Assert.Single(voicing.Barres);
        Assert.Equal(3, ChordDiagramLayoutBuilder.Build(voicing).Markers.Count);
    }

    private static ChordVoicing GetBarreVoicing(string chordText = "F", string label = "Barre shape")
    {
        var chord = new ChordSymbol(PitchClass.F, ChordQuality.Major, null, chordText);
        return new ChordVoicing(
            chord,
            label,
            new[]
            {
                new StringPosition(0, StringPlayState.Muted, null, null),
                new StringPosition(1, StringPlayState.Fretted, 3, 1),
                new StringPosition(2, StringPlayState.Fretted, 3, 1),
                new StringPosition(3, StringPlayState.Fretted, 3, 1),
                new StringPosition(4, StringPlayState.Fretted, 5, 3),
                new StringPosition(5, StringPlayState.Muted, null, null)
            },
            3,
            4,
            new[] { new Barre(3, 1, 1, 3) });
    }
}