using System.Reflection;
using System.Text.Json;

namespace ChordVisualizer.Core;

public sealed class ChordsDbChordVoicingProvider : IChordVoicingProvider, IDisposable
{
    private readonly JsonDocument _database;

    public ChordsDbChordVoicingProvider()
    {
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("ChordVisualizer.Core.Data.chords-db-guitar.json")
            ?? throw new InvalidOperationException("The curated guitar chord database resource is missing.");
        _database = JsonDocument.Parse(stream);
    }

    public IReadOnlyList<ChordVoicing> GetVoicings(ChordSymbol chord)
    {
        if (!_database.RootElement.TryGetProperty("chords", out var chordRoots))
            return Array.Empty<ChordVoicing>();

        foreach (var root in chordRoots.EnumerateObject())
        {
            if (!TryParseRoot(root.Name, out var rootPitch) || rootPitch != chord.Root)
                continue;

            foreach (var chordRecord in root.Value.EnumerateArray())
            {
                if (!chordRecord.TryGetProperty("suffix", out var suffix) ||
                    !string.Equals(suffix.GetString(), chord.Suffix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return ReadPositions(chord, chordRecord);
            }
        }

        return Array.Empty<ChordVoicing>();
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    private static IReadOnlyList<ChordVoicing> ReadPositions(ChordSymbol chord, JsonElement chordRecord)
    {
        if (!chordRecord.TryGetProperty("positions", out var positions))
            return Array.Empty<ChordVoicing>();

        var voicings = new List<ChordVoicing>();
        var positionNumber = 1;
        foreach (var position in positions.EnumerateArray())
        {
            var baseFret = position.GetProperty("baseFret").GetInt32();
            var fretValues = position.GetProperty("frets").EnumerateArray().Select(value => value.GetInt32()).ToArray();
            var fingerValues = position.TryGetProperty("fingers", out var fingers)
                ? fingers.EnumerateArray().Select(value => value.GetInt32()).ToArray()
                : Array.Empty<int>();

            if (fretValues.Length != 6)
                continue;

            var stringPositions = new StringPosition[6];
            for (var stringIndex = 0; stringIndex < stringPositions.Length; stringIndex++)
            {
                var fretValue = fretValues[stringIndex];
                int? finger = stringIndex < fingerValues.Length && fingerValues[stringIndex] > 0
                    ? fingerValues[stringIndex]
                    : null;

                stringPositions[stringIndex] = fretValue switch
                {
                    < 0 => new StringPosition(stringIndex, StringPlayState.Muted, null, null),
                    0 => new StringPosition(stringIndex, StringPlayState.Open, 0, null),
                    _ => new StringPosition(stringIndex, StringPlayState.Fretted, baseFret + fretValue - 1, finger)
                };
            }

            var barres = ReadBarres(position, stringPositions, baseFret);
            voicings.Add(new ChordVoicing(
                chord,
                $"Position {positionNumber++}",
                stringPositions,
                baseFret,
                4,
                barres));
        }

        return voicings;
    }

    private static IReadOnlyList<Barre> ReadBarres(JsonElement position, IReadOnlyList<StringPosition> positions, int baseFret)
    {
        if (!position.TryGetProperty("barres", out var barreFrets) || barreFrets.ValueKind != JsonValueKind.Array)
            return Array.Empty<Barre>();

        var barres = new List<Barre>();
        foreach (var relativeFret in barreFrets.EnumerateArray().Select(value => value.GetInt32()))
        {
            var absoluteFret = baseFret + relativeFret - 1;
            var fingerGroups = positions
                .Where(stringPosition => stringPosition.State == StringPlayState.Fretted &&
                                         stringPosition.Fret == absoluteFret &&
                                         stringPosition.Finger.HasValue)
                .GroupBy(stringPosition => stringPosition.Finger!.Value)
                .Where(group => group.Count() > 1);

            foreach (var group in fingerGroups)
            {
                var stringIndexes = group.Select(stringPosition => stringPosition.StringIndex).ToArray();
                var startStringIndex = stringIndexes.Min();
                var endStringIndex = stringIndexes.Max();
                var coveredPositions = positions
                    .Where(stringPosition => stringPosition.StringIndex >= startStringIndex &&
                                             stringPosition.StringIndex <= endStringIndex)
                    .ToArray();

                if (coveredPositions.Length != endStringIndex - startStringIndex + 1 ||
                    coveredPositions.Any(stringPosition => stringPosition.State != StringPlayState.Fretted ||
                                                           stringPosition.Fret < absoluteFret ||
                                                           (stringPosition.Fret == absoluteFret && stringPosition.Finger.HasValue &&
                                                            stringPosition.Finger != group.Key)))
                {
                    continue;
                }

                barres.Add(new Barre(absoluteFret, group.Key, startStringIndex, endStringIndex));
            }
        }

        return barres;
    }

    private static bool TryParseRoot(string value, out PitchClass root)
    {
        try
        {
            var normalized = value.Replace("sharp", "#", StringComparison.OrdinalIgnoreCase);
            root = ChordSymbol.ParsePitchClass(normalized);
            return true;
        }
        catch (FormatException)
        {
            root = default;
            return false;
        }
    }
}