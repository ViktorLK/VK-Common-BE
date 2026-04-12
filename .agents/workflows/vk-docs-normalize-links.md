---
description: Standardize internal documentation links to repository-root-relative paths (/src/ and /docs/) across all MD files safely.
---

## Goal

Normalize all internal links in **all Markdown files (`.md`)** across the entire repository to follow the repository-root-relative convention (`/src/...`, `/docs/...`) and remove local absolute file URIs, while strictly preserving character encoding.

## Steps

1. **Enumerate Targets (Git-Aware)**:
    - Use `git ls-files "*.md"` and `git ls-files -o --exclude-standard "*.md"` to identify all Markdown files.
    - This approach strictly respects `.gitignore` rules and ensures only relevant files are processed.

2. **Calculate Repository Root**:
    - Determine the absolute local path of the current repository root (using the workspace path context) to identify absolute file URIs that need removal.

3. **Safe Link Normalization (File-by-File)**:
    - **CRITICAL**: Do NOT use bulk terminal commands (like PowerShell pipes) that risk character corruption.
    - For each Markdown file:
        - Read the file content using `view_file`.
        - Apply the following replacements in the AI's internal memory:
            - Replace absolute local file URIs (e.g., `file:///path/to/repo/`) with `/`.
            - Standardize relative links that start with `src/` or `docs/` by adding a leading slash (e.g., `[text](/src/file.cs)` -> `[text](/src/file.cs)`).
        - If changes are needed, save the modified content back using the `write_to_file` tool with `Overwrite: true` or `replace_file_content`. These tools are designed to handle character encoding safely.

4. **Verify Encoding and Readability**:
    - After processing a batch of files, perform a `view_file` on at least one file containing multi-byte characters (Japanese) to ensure no mojibake occurred.

5. **Final Cleanup**:
    - Provide a summary of how many files were modified and confirm that no `file:///` links or relative `src/` links remain in the repository.
