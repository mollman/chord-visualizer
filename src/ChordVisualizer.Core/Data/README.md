# Curated Guitar Chord Data

`chords-db-guitar.json` is the generated guitar catalog from [tombatossals/chords-db](https://github.com/tombatossals/chords-db), pinned to commit `df06fa7b425cf5fd29485ff6591236b3557e3fac` (upstream version 0.6.0, retrieved 2026-10-08).

The snapshot is intentionally checked in. The application has no runtime dependency on Node.js or an external chord service. To update it manually, select an upstream commit, replace this file with that commit's `lib/guitar.json`, update the commit and date above, and run the curated provider tests.

The upstream `barres` values identify frets. The provider combines them with finger assignments to create the string ranges required by this application's model; verify ambiguous shapes against upstream diagrams when updating the snapshot.