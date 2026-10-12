# Deploying under a subpath on Azure Static Web Apps

Chord Visualizer is a standalone Blazor WebAssembly application. Its published
static files can live in a subdirectory of a larger static website, such as
`/ChordVisualizer/`.

The app uses a relative base URL (`./`), so it can load its framework assets
from either the local site root or its deployed subdirectory. Link to it with a
trailing slash:

```html
<a href="/ChordVisualizer/">Chord Visualizer</a>
```

## Build and stage

Publish the app, then copy the contents of its published `wwwroot` directory
into a `ChordVisualizer` directory in the main website's deployment artifact.
For example, from the repository root in PowerShell:

```powershell
dotnet publish src\ChordVisualizer.Web\ChordVisualizer.Web.csproj `
  --configuration Release `
  --output artifacts\chord-visualizer

New-Item -ItemType Directory -Force artifacts\site\ChordVisualizer | Out-Null
Copy-Item artifacts\chord-visualizer\wwwroot\* `
  artifacts\site\ChordVisualizer `
  -Recurse -Force
```

The final artifact should have this shape:

```text
artifacts/site/
  index.html                 # The separately authored site index
  staticwebapp.config.json   # The main site's routing/access configuration
  ChordVisualizer/
    index.html
    _framework/
    css/
    ...
```

Deploy that combined artifact as one Azure Static Web App. A custom domain is
bound to the Static Web App, not to each subdirectory, so the root site and
Chord Visualizer share the same deployment and domain.

The app currently uses only the root client-side route, so the directory
`index.html` is sufficient for normal navigation. If client-side routes are
added later, configure the main site's `staticwebapp.config.json` to fall back
to `/ChordVisualizer/index.html` for app routes without sending the root site's
routes there. Keep that configuration at the deployment artifact root; a
configuration file inside the `ChordVisualizer` subdirectory does not configure
the Static Web App.
