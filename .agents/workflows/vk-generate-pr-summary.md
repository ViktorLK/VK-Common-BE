---
description: Analyze git diff and generate a professional Pull Request summary in Markdown.
---

## Goal

Read the uncommitted changes (or changes between specific branches) using `git diff` and generate a highly professional, well-structured Pull Request description in Business IT Japanese (ビジネスIT日本語) highlighting alignment with VK.Blocks architecture.

## Steps

1. **Read Changes via Tool**:
    - Before writing anything, use your OS command tool to execute `git diff` (or `git diff --cached` if files are staged) in the project workspace to understand exactly what was changed.
    - If the user provides a branch name, compare it against `main` or `develop`.

2. **Analyze the Diff**:
    - Understand the _purpose_ behind the code modifications.
    - Actively cross-reference the changes with the `vk-blocks-checklist.md` rules. Did they introduce `Result<T>`? Did they add `Polly` policies? Did they add Interceptors?

3. **Generate Markdown Summary**:
    - Create the PR Summary with the following structure:

    ```markdown
    ## 🎯 目的 (Goal)

    [Why this PR exists. What problem does it solve?]

    ## 🛠️ 技術的変更点 (Technical Details)

    - [File/Component 1]: [Brief explanation of what changed]
    - [File/Component 2]: [Brief explanation of what changed]

    ## 🛡️ アーキテクチャ準拠 (Architectural Alignment)

    - ✅ **Result Pattern (Rule 1)**: [Did we apply Result<T> properly?]
    - ✅ **Observability (Rule 6)**: [Did we add proper tracing/structured logs?]
    - (Include only the Rules that are relevant to this PR)

    ## 🧪 確認事項 (Testing Done)

    - [ ] Unit tests generated and passed
    - [ ] Local build verified
    ```

4. **Output Format**:
    - Do NOT save this to a file.
    - Print the generated Markdown directly into the chat inside a `markdown ` block so the user can easily copy and paste it into GitHub / Azure DevOps.
