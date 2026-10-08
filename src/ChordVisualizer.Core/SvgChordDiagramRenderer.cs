using System.Globalization;
using System.Xml.Linq;

namespace ChordVisualizer.Core;

public static class SvgChordDiagramRenderer
{
    private static readonly XNamespace SvgNamespace = "http://www.w3.org/2000/svg";

    public static string Render(ChordDiagramLayout layout)
    {
        var svg = new XElement(SvgNamespace + "svg",
            new XAttribute("viewBox", $"0 0 {Number(layout.Width)} {Number(layout.Height)}"),
            new XAttribute("width", Number(layout.Width)),
            new XAttribute("height", Number(layout.Height)),
            new XAttribute("role", "img"),
            new XAttribute("aria-label", layout.AccessibleLabel),
            new XAttribute("style", "display:block;width:100%;height:auto"),
            new XElement(SvgNamespace + "title", layout.AccessibleLabel));

        foreach (var line in layout.Lines)
        {
            svg.Add(new XElement(SvgNamespace + "line",
                new XAttribute("x1", Number(line.X1)),
                new XAttribute("y1", Number(line.Y1)),
                new XAttribute("x2", Number(line.X2)),
                new XAttribute("y2", Number(line.Y2)),
                new XAttribute("stroke", line.Kind == ChordDiagramLineKind.Nut ? "#263d38" : "#99a8a2"),
                new XAttribute("stroke-width", line.Kind switch
                {
                    ChordDiagramLineKind.Nut => "5",
                    ChordDiagramLineKind.String => "1.7",
                    _ => "1.3"
                }),
                new XAttribute("stroke-linecap", "round")));
        }

        if (layout.FretLabel is not null)
        {
            svg.Add(CreateText(layout.FretLabelX, layout.FretLabelY, layout.FretLabel, "#465852", 11, "start", "600"));
        }

        foreach (var barre in layout.Barres)
        {
            svg.Add(new XElement(SvgNamespace + "rect",
                new XAttribute("x", Number(barre.X)),
                new XAttribute("y", Number(barre.Y)),
                new XAttribute("width", Number(barre.Width)),
                new XAttribute("height", Number(barre.Height)),
                new XAttribute("rx", Number(barre.Height / 2)),
                new XAttribute("fill", "#167267")));

            if (!string.IsNullOrEmpty(barre.Label))
            {
                svg.Add(CreateText(
                    barre.X + barre.Width / 2,
                    barre.Y + barre.Height / 2 + 4,
                    barre.Label,
                    "#ffffff",
                    12,
                    "middle",
                    "700"));
            }
        }

        foreach (var marker in layout.Markers)
        {
            if (marker.Kind == ChordDiagramMarkerKind.Open)
            {
                svg.Add(new XElement(SvgNamespace + "circle",
                    new XAttribute("cx", Number(marker.X)),
                    new XAttribute("cy", Number(marker.Y)),
                    new XAttribute("r", Number(marker.Radius)),
                    new XAttribute("fill", "#ffffff"),
                    new XAttribute("stroke", "#263d38"),
                    new XAttribute("stroke-width", "2")));
            }
            else if (marker.Kind == ChordDiagramMarkerKind.Muted)
            {
                svg.Add(CreateText(marker.X, marker.Y + 6, "x", "#263d38", 17, "middle", "500"));
            }
            else
            {
                if (!marker.IsOnBarre)
                {
                    svg.Add(new XElement(SvgNamespace + "circle",
                        new XAttribute("cx", Number(marker.X)),
                        new XAttribute("cy", Number(marker.Y)),
                        new XAttribute("r", Number(marker.Radius)),
                        new XAttribute("fill", "#167267")));
                }

                if (!string.IsNullOrEmpty(marker.Label))
                {
                    svg.Add(CreateText(
                        marker.X,
                        marker.Y + 4,
                        marker.Label,
                        "#ffffff",
                        marker.Label.Length > 2 ? 8 : 11,
                        "middle",
                        "700"));
                }
            }
        }

        return svg.ToString(SaveOptions.DisableFormatting);
    }

    public static string Render(ChordVoicing voicing, bool showNoteNames = false)
    {
        return Render(ChordDiagramLayoutBuilder.Build(voicing, showNoteNames));
    }

    private static XElement CreateText(double x, double y, string text, string fill, int fontSize, string anchor, string weight)
    {
        return new XElement(SvgNamespace + "text",
            new XAttribute("x", Number(x)),
            new XAttribute("y", Number(y)),
            new XAttribute("fill", fill),
            new XAttribute("font-family", "system-ui, sans-serif"),
            new XAttribute("font-size", fontSize),
            new XAttribute("font-weight", weight),
            new XAttribute("text-anchor", anchor),
            text);
    }

    private static string Number(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}