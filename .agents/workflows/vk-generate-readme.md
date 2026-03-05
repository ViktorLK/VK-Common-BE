---
description: Generate a professional Japanese README.md for a specified module using the ReadMeGenerator.md prompt.
---

## Goal

Generate a professional, portfolio-quality Japanese README.md for a specified project module, following the guidelines in `prompts/CodeReview/ReadMeGenerator.md`.

## Steps

1. **Identify the Target Module**:
    - Determine which module or project to generate the README for.
    - If the target is unclear, ask the user: `"Which module would you like me to generate a README for?"`

2. **Load Rules**:
    - Read the `prompts/CodeReview/ReadMeGenerator.md` file to understand the README structure and tone requirements.

3. **Analyze the Module**:
    - Read all source files in the target module to understand its purpose, architecture, dependencies, and key features.
    - Identify the tech stack used (frameworks, libraries, patterns).

4. **Generate the README**:
    - Write a professional Japanese README following the required sections:
        - **Title & Badges**: Project title with .NET, License, Build Status badge placeholders.
        - **Introduction (はじめに)**: Brief description of the module's purpose.
        - **Architecture (アーキテクチャ)**: Design principles, patterns, and architectural styles used.
        - **Key Features (主な機能)**: Detailed feature list with technical terminology.
        - **Tech Stack (採用技術)**: All core technologies used.
        - **Getting Started (開始方法)**: How to clone and run.
        - **Future Roadmap (今後の展望)**: Planned features (optional).
    - Tone: Professional, concise, confident — suitable for a resume or LinkedIn showcase.

5. **Save and Report**:
    - Save the README.md to the root of the target module directory.
    - Report: ✅ README.md saved to `[path]`.
