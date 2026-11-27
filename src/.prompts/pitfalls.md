# Repo-Wide External Pitfall Memory (Cross-Session Working Notes)

> **Maintenance**: Keep each entry 1–2 lines with a `[YYYY-MM-DD]` date. For module-specific pitfalls, create `/src/BuildingBlocks/{Module}/.prompts/pitfalls.md` on demand (`vk_be_get_module_context` auto-cascades both). Prune obsolete items or promote recurring ones (2+ hits) to Roslyn analyzers or `module-manifest.md` when this file exceeds ~40 entries.

## Tried (Failed Approaches)
- `[2026-11-27] Diagnostic ID VK0001 collision`: Inline `VK0001` in `OAuthProviderMapperGenerator` collided with `MissingSealedModifierAnalyzer` (`VK0001`). Reserve `VK0001–VK0999` for `VK.Tools.Analyzers`, `VK1200–VK1299` for Auth SG (`VK1201`), and register all SG diagnostics in `VKDiagnosticDescriptors.cs`.
- `[2026-11-27] dotnet build -warnaserror on single BuildingBlock`: `VK.Tools.SourceGenerators` has 8 pre-existing `RS1030`/`RS1032`/`RS1037` warnings; build `Analyzers`/`SourceGenerators` first and run `dotnet build <Block>.csproj --no-restore --no-dependencies -warnaserror` to isolate BuildingBlock warnings until SG warnings are cleaned up.

## Never (Hard Pitfalls)
- Never declare a new Roslyn `DiagnosticDescriptor` inline inside a Generator or Analyzer without checking the global ID allocation table in `VKAnalyzerDescriptors.cs` and `VKDiagnosticDescriptors.cs`.
- Never use ALL-CAPS headings (e.g. `## TRIED`) in `.prompts/pitfalls.md` — `McpTools.SplitBodyIntoContext` parses all-caps headings as rule IDs instead of general guidance.

## Decided (Working Conventions)
- `[2026-11-27]` Compile-time Type A rules (`VK0001–VK0006`, `CA1848`, `CA1852`, `CA2007`, `IDE0130`) are enforced via `VK.Tools.Analyzers` + `.editorconfig` (`warning`, hard-blocked by `-warnaserror`); avoid duplicating them as verbose prompt prose.
