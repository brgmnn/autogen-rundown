# Repository Guidelines

## Project Structure & Module Organization
- `AutogenRundown/src`: C# plugin source (net6.0).
- `AutogenRundownTests`: MSTest unit tests.
- `GameData`: JSON assets copied into build output.
- `build/`: Post-build output (DLL + manifest/assets).
- `deps/`, `AutogenRundown/interop`, `AutogenRundown/plugins`: local DLL references (BepInEx, interop, Thunderstore plugins).
- Solution: `AutogenRundown.sln`.

## Build, Test & Local Run
- Build: `dotnet build AutogenRundown.sln`
  - Copies assets to `build/`. When `Debug` MSBuild property is not set to `false` on Windows, the build also deploys to r2modman’s BepInEx profile: `%AppData%\r2modmanPlus-local\GTFO\profiles\modding\BepInEx\plugins\the_tavern-AutogenRundown`.
- Disable debug deploy: `dotnet build /p:Debug=false`
- Tests: `dotnet test AutogenRundownTests`
- Optional coverage: `dotnet test /p:CollectCoverage=true`

## Coding Style & Naming Conventions
- Indentation: C# uses 4 spaces; JSON/MD/SH/YML and project files use 2. Enforced via `.editorconfig`.
- Usings: place outside namespace; unused usings are treated as errors (`IDE0005`).
- C#: Nullable enabled, implicit usings on; prefer PascalCase for types, camelCase for locals/params, `_camelCase` for private fields.
- Keep system usings first; groupings need not be separated.

## Testing Guidelines
- Framework: MSTest (`Microsoft.NET.Test.Sdk`, `MSTest.TestFramework`).
- Conventions: test classes end with `Tests` (e.g., `GeneratorTests.cs`), methods annotated with `[TestMethod]`.
- Scope: prefer fast, deterministic unit tests around `AutogenRundown/src` logic.
- Run locally with `dotnet test`; add coverage when changing core generation logic.

## Commit & Pull Request Guidelines
- Commits: short, imperative, present tense (e.g., "Fix generator cluster spawn").
- Group related changes; keep diffs focused. Reference issues where applicable.
- PRs: include summary, rationale, test results, and screenshots/logs if behavior changes. Note any data changes under `GameData/`.

## Packaging & Tips
- Thunderstore metadata: `manifest.json`, `thunderstore.toml`; post-build copies README/CHANGELOG into `build/`.
- Ensure `deps/` contains BepInEx pack; do not commit game files.
- For local in-game testing, use the default build (debug deploy enabled) and launch GTFO via your mod manager profile.
