---
name: vk-generate-readme
description: Generate an evidence-based Japanese README.md for a single publishable module using the ReadMeGenerator.md spec.
---

## Goal

Generate an accurate, verifiable Japanese README.md for one module (NuGet package), following `references/ReadMeGenerator.md`.
Every claim in the README must be traceable to the module's files. Unknowns are marked as TODO, never guessed.

## Steps

1. **Identify the Target Module**
    - Determine which module to generate the README for.
    - If the target is unclear, ask: `"Which module would you like me to generate a README for?"`
    - If the module is an internal slice and not a publishable package (no `PackageId` / not packable), ask the user whether to proceed.

2. **Load Rules**
    - Read `references/ReadMeGenerator.md` (or `.agents/skills/vk-generate-readme/references/ReadMeGenerator.md`).
    - Fill the Project Context placeholders: `{{MODULE}}` = module name, `{{MODULE_PATH}}` = module directory path.
    - Treat the spec as authoritative for structure, tone, and the evidence rules.

3. **Collect Facts in Layers** (do not read every source file blindly)
    - Read in this order and stop reading a layer once the facts for it are clear:
        1. Existing manifest / design docs / ADRs related to the module
        2. `csproj`: `PackageId`, `TargetFramework`, `PackageReference`, `ProjectReference`
        3. Public API surface: Block entry class, DI extension methods, public interfaces and types
        4. Options classes: properties, defaults, validation, `SectionName`
        5. Diagnostics constants and Roslyn diagnostic definitions (if any)
        6. Test project and samples (to confirm real usage)
        7. Implementation files, only to verify a specific claim
    - Also read the repository root `LICENSE`, any CI workflow file, and the existing README (if present).

4. **Build the Evidence Table** (internal, not part of the output)
    - Map each planned claim to a file or type: `claim → evidence`.
    - Drop any claim without evidence, or mark it as `<!-- TODO: 確認が必要 — ... -->`.
    - Record source conflicts as TODO items.
    - Decide which conditional sections to include or omit (設定 / 設計判断 / 診断・可観測性 / 今後の展望).

5. **Generate the README**
    - Write the Japanese README (です・ます体) following the sections and rules in `ReadMeGenerator.md`.
    - Code and code comments in English; type, package, and API names kept in original English.
    - Include a Mermaid diagram only when the module has a non-trivial flow or layered structure.

6. **Self-check**
    - Run through the Self-check list in `ReadMeGenerator.md`.
    - Verify that every type, method, and Options property used in code samples exists in the source.
    - Verify that no technology appears that the module does not actually use.
    - Fix issues before saving.

7. **Save and Report**
    - If `README.md` does not exist in the module root, save it as `README.md`.
    - If it already exists, do **not** overwrite: save to `README.generated.md` and tell the user to review the differences.
    - Report in this format:
        - ✅ README saved to `[path]`
        - TODO count: `[n]` (list each in one line)
        - Unverified or conflicting items: `[list, or "none"]`
        - Sections omitted for lack of facts: `[list, or "none"]`
