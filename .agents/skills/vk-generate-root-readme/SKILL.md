---
name: vk-generate-root-readme
description: Generate an evidence-based Japanese root README.md for a multi-package repository using the RootReadMeGenerator.md spec.
---

## Goal

Generate an accurate, navigation-oriented Japanese README.md at the repository root of a multi-package .NET repository, following `references/RootReadMeGenerator.md`.
Facts (package list, dependencies, target frameworks) come from `csproj` files. Narrative content (principles, non-goals) comes from existing documents only. Unknowns are marked as TODO, never guessed.

This workflow should run **after** package-level READMEs have been generated with `/vk-generate-readme`.

## Steps

1. **Confirm the Repository and Check Prerequisites**
    - Identify the repository root. If unclear, ask: `"Which repository root would you like me to generate a README for?"`
    - List all packable projects (exclude test, sample, benchmark, and tooling projects).
    - Check which packable projects already have a `README.md` in their directory.
    - If many packages lack a README, tell the user and suggest running `/vk-generate-readme` for them first. Continue only if the user confirms; missing ones will appear as TODO rows.

2. **Load Rules**
    - Read `references/RootReadMeGenerator.md` (or `.agents/skills/vk-generate-root-readme/references/RootReadMeGenerator.md`).
    - Fill the Project Context placeholder: `{{REPO_ROOT}}` = repository root path.
    - Treat the spec as authoritative for structure, tone, and evidence rules.

3. **Enumerate Packages and Build the Dependency Graph** (from `csproj` only)
    - For each packable project, collect: `PackageId`, `TargetFramework`, `Description`, and `ProjectReference` entries pointing to other packable projects.
    - Group packages by domain (derive grouping from the package naming or directory structure; if ambiguous, record a TODO instead of inventing groups).
    - Do not take dependency or version facts from README text.

4. **Collect Narrative and Supporting Facts**
    - Read, if present: manifest / design docs / ADRs / `docs/`, the existing root README, `CONTRIBUTING.md`, `SECURITY.md`, `CHANGELOG.md`, `LICENSE`, CI workflow files, and any roadmap or backlog document.
    - From each package README, read only the introduction (「はじめに」) to get the one-line purpose; do not copy details.
    - Count the `TODO:` markers in each package README for the final report. Do not carry them into the root README.

5. **Determine Optional Columns and Sections**
    - **Stability column**: include only if an explicit source exists (documentation, metadata, or a list the user provides). If none is found, omit the column and mention it in the report.
    - Decide for each conditional section (設計原則 / パッケージの選び方 / クイックスタート / 今後の展望) whether enough evidence exists. Omit the section if not.

6. **Build the Evidence Table** (internal, not part of the output)
    - Map each planned claim to a file: `claim → evidence`.
    - Drop claims without evidence, or mark them as `<!-- TODO: 確認が必要 — ... -->`.
    - Record conflicts (e.g. a package README that disagrees with its `csproj`) as TODO items.

7. **Generate the README**
    - Write the Japanese README (です・ます体) following the sections and rules in `RootReadMeGenerator.md`.
    - Build the Mermaid architecture diagram only from existing `ProjectReference` edges; group by `subgraph` when there are many packages.
    - Link each package row to its README; leave a TODO where the README is missing.

8. **Self-check**
    - Run through the Self-check list in `RootReadMeGenerator.md`.
    - Verify that the package table matches the enumerated packable projects exactly (no missing, no extra).
    - Verify that every Mermaid edge exists as a `ProjectReference`.
    - Verify that every link points to an existing file.
    - Fix issues before saving.

9. **Save and Report**
    - If `README.md` does not exist at the repository root, save it as `README.md`.
    - If it already exists, do **not** overwrite: save to `README.generated.md` and tell the user to review the differences.
    - Report in this format:
        - ✅ README saved to `[path]`
        - Packages listed: `[n]`; packages without a README: `[list, or "none"]`
        - TODO count in the root README: `[n]` (list each in one line)
        - Total TODOs remaining in package READMEs: `[n]`
        - Conflicts found between sources: `[list, or "none"]`
        - Sections or columns omitted for lack of facts: `[list, or "none"]`

## Maintenance Note

The package table and architecture diagram go stale when packages are added, removed, or re-referenced. Re-run this workflow after such changes, and review the diff against the existing README before replacing it.
