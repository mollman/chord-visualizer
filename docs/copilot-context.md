ChordVisualizer — Project Context for GitHub Copilot
This document captures the architectural decisions, scaffolding, models, and roadmap for the ChordVisualizer application. Copilot Chat should use this file as context for future development.

🎯 Project Goal
Build a cross‑platform guitar chord visualization application that:

Accepts chord names (e.g., Dm, Fmaj7, C9, C/D)

Displays chord voicing shapes across the entire fretboard

Renders shapes visually (similar to standard chord diagrams)

Prints chord shapes in ASCII/text format for debugging or export

Supports multiple platforms:

Web (initial target)

Windows

iOS (optional)

Uses C# as the primary language

Uses VS Code as the development environment

Hosts the repo on GitHub

🧱 Architecture Overview
1. Core Library — ChordVisualizer.Core
A .NET 8 class library containing:

Chord theory models

Voicing models

Fretboard geometry

Rendering logic (ASCII + later SVG)

Voicing providers (JSON-backed initially)

This library is platform‑agnostic and reused by all front‑ends.

2. Web UI — ChordVisualizer.Web
A Blazor WebAssembly app that:

Accepts chord input

Queries the core library for voicings

Renders diagrams (ASCII now, SVG later)

Can be hosted on GitHub Pages

3. Future Shells
Windows: .NET MAUI or WPF

iOS: .NET MAUI
Both reuse ChordVisualizer.Core.

📁 Solution Structure
Code
ChordVisualizer/
│
├── ChordVisualizer.sln
│
├── ChordVisualizer.Core/
│   └── ChordVisualizer.Core.csproj
│
└── ChordVisualizer.Web/
    └── ChordVisualizer.Web.csproj
🖥️ Windows Shell Commands (PowerShell/CMD)
Create solution
Code
dotnet new sln -n ChordVisualizer
Core library
Code
dotnet new classlib -n ChordVisualizer.Core
dotnet sln ChordVisualizer.sln add ChordVisualizer.Core\ChordVisualizer.Core.csproj
Blazor WebAssembly app
Code
dotnet new blazorwasm -n ChordVisualizer.Web
dotnet sln ChordVisualizer.sln add ChordVisualizer.Web\ChordVisualizer.Web.csproj
Add reference
Code
dotnet add ChordVisualizer.Web\ChordVisualizer.Web.csproj reference ChordVisualizer.Core\ChordVisualizer.Core.csproj
GitHub setup
Code
git init
git add .
git commit -m "Initial ChordVisualizer scaffolding"
git branch -M main
git remote add origin https://github.com/<your-username>/ChordVisualizer.git
git push -u origin main
🎼 Core Domain Model (C#)
Pitch classes
csharp
public enum PitchClass { C, Cs, D, Ds, E, F, Fs, G, Gs, A, As, B }
Chord qualities
csharp
public enum ChordQuality
{
    Major, Minor, Dominant7, Major7, Minor7, Add9, Sus4
}
Chord symbol
csharp
public sealed class ChordSymbol
{
    public PitchClass Root { get; }
    public ChordQuality Quality { get; }
    public PitchClass? BassOverride { get; }
    public string Text { get; }

    public ChordSymbol(PitchClass root, ChordQuality quality, PitchClass? bassOverride, string text)
    {
        Root = root;
        Quality = quality;
        BassOverride = bassOverride;
        Text = text;
    }
}
Fretboard + voicing representation
csharp
public enum StringPlayState { Muted, Open, Fretted }

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

public sealed class ChordVoicing
{
    public ChordSymbol Chord { get; }
    public string Label { get; }
    public IReadOnlyList<StringPosition> Positions { get; }
    public int DisplayStartFret { get; }
    public int DisplayFretSpan { get; }

    public ChordVoicing(
        ChordSymbol chord,
        string label,
        IEnumerable<StringPosition> positions,
        int displayStartFret,
        int displayFretSpan)
    {
        Chord = chord;
        Label = label;
        Positions = positions.ToList();
        DisplayStartFret = displayStartFret;
        DisplayFretSpan = displayFretSpan;
    }
}
Voicing provider interface
csharp
public interface IChordVoicingProvider
{
    IReadOnlyList<ChordVoicing> GetVoicings(ChordSymbol chord);
}
📦 JSON Voicing Data
A simple JSON schema for chord shapes:

json
{
  "chord": "C/D",
  "label": "Open D11",
  "displayStartFret": 0,
  "displayFretSpan": 4,
  "positions": [
    { "stringIndex": 0, "state": "Muted", "fret": null, "finger": null },
    { "stringIndex": 1, "state": "Muted", "fret": null, "finger": null },
    { "stringIndex": 2, "state": "Fretted", "fret": 0, "finger": null },
    { "stringIndex": 3, "state": "Fretted", "fret": 0, "finger": null },
    { "stringIndex": 4, "state": "Fretted", "fret": 0, "finger": null },
    { "stringIndex": 5, "state": "Fretted", "fret": 1, "finger": 1 }
  ]
}
🖨️ ASCII Diagram Renderer
csharp
public static class ChordDiagramRenderer
{
    public static string Render(ChordVoicing voicing)
    {
        int strings = 6;
        int frets = voicing.DisplayFretSpan;

        var grid = new char[frets + 1, strings];

        for (int f = 0; f <= frets; f++)
            for (int s = 0; s < strings; s++)
                grid[f, s] = '|';

        foreach (var pos in voicing.Positions)
        {
            if (pos.State == StringPlayState.Muted)
                grid[0, pos.StringIndex] = 'x';
            else if (pos.State == StringPlayState.Open)
                grid[0, pos.StringIndex] = 'o';
            else if (pos.State == StringPlayState.Fretted && pos.Fret.HasValue)
            {
                int fretRow = pos.Fret.Value - voicing.DisplayStartFret;
                if (fretRow >= 1 && fretRow <= frets)
                    grid[fretRow, pos.StringIndex] =
                        pos.Finger.HasValue ? pos.Finger.Value.ToString()[0] : '●';
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{voicing.Chord.Text} ({voicing.Label})");

        for (int f = 0; f <= frets; f++)
        {
            for (int s = 0; s < strings; s++)
                sb.Append(grid[f, s]).Append(' ');
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
🌐 Blazor Web Wiring
csharp
builder.Services.AddSingleton<IChordVoicingProvider>(sp =>
{
    var stream = File.OpenRead("Data/chord-voicings.json");
    return new JsonChordVoicingProvider(stream);
});
Simple UI component:

razor
<input @bind="ChordText" placeholder="Enter chord (e.g. C/D)" />
<button @onclick="Search">Show Voicings</button>

@if (Voicings?.Count > 0)
{
    foreach (var v in Voicings)
    {
        <h4>@v.Chord.Text (@v.Label)</h4>
        <pre>@ChordDiagramRenderer.Render(v)</pre>
    }
}
📚 Open‑Source References for Voicing Data
These projects can be used as inspiration or data sources:

chordplayer (JS) — large voicing catalog

fretboard-chord-visualizer (JS)

dadabe (.NET harmonic engine)

None provide a ready‑made C# voicing dictionary, so the plan is:

Seed your own JSON dataset

Optionally port data from JS projects

Later add algorithmic voicing generation

🛣️ Roadmap
Phase 1 — Foundation
Core library models

JSON voicing provider

Blazor Web UI

ASCII renderer

Basic chord parser

Phase 2 — Visualization
SVG fretboard renderer

Finger dots, muted/open indicators

Barre detection

Multiple voicing layouts

Phase 3 — Expansion
More chord types

More voicings

Sound clips (optional)

Import/export formats

Phase 4 — Cross‑Platform
.NET MAUI Windows app

.NET MAUI iOS app

Shared UI components

🧭 How Copilot Should Use This File
Copilot Chat should:

Treat this file as persistent architectural context

Use the naming conventions exactly as written

Generate code that fits the models and structure above

Follow the roadmap when suggesting next steps

Avoid reinventing the architecture unless asked

Reference this file when implementing new features

Implement each class and interface in a separate compilation unit