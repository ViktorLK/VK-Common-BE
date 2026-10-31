---
name: vk-generate-pr-summary
description: Analyze git diff and generate a professional Pull Request summary in Markdown aligned with .github/PULL_REQUEST_TEMPLATE.md.
---

## Goal

Read the uncommitted changes (or changes between specific branches) using `git diff` and generate a highly professional, well-structured Pull Request description in Business IT Japanese (ビジネスIT日本語) highlighting alignment with VK.Blocks architecture, fully adhering to `.github/PULL_REQUEST_TEMPLATE.md`.

## Steps

1. **Read Changes via Tool**:
    - Before writing anything, use your OS command tool to execute `git diff` (or `git diff --cached` if files are staged) in the project workspace to understand exactly what was changed.
    - If the user provides a branch name, compare it against `main` or `develop`.

2. **Analyze the Diff**:
    - Understand the _purpose_ behind the code modifications.
    - Actively cross-reference the changes with `l3-vk-be.md` rules (`CS.01`, `CS.03`, `CS.06`, `OR.01`, `AP.01`, `AP.03`, `BB.01-08`). Did they introduce `Result<T>`? Did they propagate `CancellationToken`? Did they use `TimeProvider`?

3. **Generate Markdown Summary**:
    - Align with the standard repository template in `.github/PULL_REQUEST_TEMPLATE.md`.
    - Create the PR Summary with the following structure:

    ```markdown
    ## 🎯 目的 (Goal / Context)

    [Why this PR exists. What problem does it solve? Link to Backlog Item / RFC / Issue]
    - Ref: docs/07-Backlogs/{ModuleName}/{NNN}-{slug}.md

    ## 🛠️ 変更概要 (Summary of Changes)

    - [File/Component 1]: [Brief explanation of what changed]
    - [File/Component 2]: [Brief explanation of what changed]

    ## 🛡️ アーキテクチャ・品質チェック (Architectural Compliance)

    - [x] **Result Pattern (`CS.01`, `BASE-05`)**: [Did we apply Result<T> properly?]
    - [x] **Contract & Boundaries (`BASE-01`, `AP.03`, `ARCH-07`)**: [Are public surfaces minimal with VK prefix?]
    - [x] **Async Hygiene (`CS.03`)**: [Is CancellationToken propagated with ConfigureAwait(false)?]
    - [x] **Deterministic Dependencies (`CS.06`, `BASE-02`)**: [Are TimeProvider and IVKGuidGenerator used?]
    - [x] **Structured Logging (`OR.01`)**: [Are [LoggerMessage] source generators used?]

    ## ⚠️ 契約変更・ADR アラート (Contract & Architecture Alert)

    - [ ] 公開契約（Public API / DTO / Enum / Options / DB Schema）の変更有無:
      - [ ] 変更なし (No Breaking Change)
      - [ ] 変更あり ➔ `docs/04-ArchitectureDecisionRecords/{Module}/` に ADR を作成・更新済み (`DL.03`, `PS.01`)

    ## ✅ Definition of Done (DoD) - 完了条件チェックリスト

    - [ ] **Build & Warning Free (`DL.02`, `BASE-03`)**: コンパイル時警告ゼロでビルドが通る (`dotnet build`)
    - [ ] **Test Coverage (`DL.01`, `BASE-07`)**: 4経路（Happy / NotFound / Permission / Infra）のテストを網羅
    - [ ] **No Naked TODO (`BASE-07`)**: 未解決の裸の `// TODO` が存在しない（条件付き `TODO[condition]` または Backlog 起票 `DL.04`）
    - [ ] **Documentation (`BASE-12`)**: 必要なドキュメント（README / Manifest / DataDictionary）が同期されている
    ```

4. **Output Format**:
    - Do NOT save this to a file.
    - Print the generated Markdown directly into the chat inside a `markdown ` block so the user can easily copy and paste it into GitHub / Azure DevOps.
