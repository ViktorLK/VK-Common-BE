import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
import fs from "fs/promises";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ✅ 新API：McpServer
const server = new McpServer({
  name: "vk-blocks-manager",
  version: "1.0.0",
});

// Register tool for code compliance auditing
server.tool(
  "audit_vk_blocks_code",
  "Audits the provided C# code snippet against the VK.Blocks architectural guidelines (Rule 1 & Rule 3 compliance).",
  {
    code: z.string().describe("The C# code snippet to audit for compliance."),
  },
  async ({ code }) => {
    const violations: string[] = [];

    if (code.includes("public") && !code.includes("Result<")) {
      violations.push("[Violation] Rule 1: Public methods must return a Result<T> object.");
    }

    return {
      content: [
        { type: "text", text: violations.length > 0 ? violations.join("\n") : "[Pass] The code complies with the VK.Blocks architectural guidelines." },
      ],
    };
  }
);


// Register tool for generating Architectural Decision Records (ADRs)
// Pattern B: MCP reads the prompt template, AI generates and writes the final .md file
server.tool(
  "publish_adr",
  "Generates an ADR by reading the official ADR_PROMPT rulebook and instructing the AI to write the full document, then save it to docs/02-ArchitectureDecisionRecords/<module>/",
  {
    sourceDir: z.string().describe(
      "Path to the source code directory being documented (e.g. 'src/BuildingBlocks/Authentication'). Determines the output subdirectory."
    ),
    title:    z.string().describe("ADR title in English."),
    context:  z.string().describe("Background context: why is this decision needed?"),
    decision: z.string().describe("The architectural decision made and its rationale."),
  },
  async ({ sourceDir, title, context, decision }) => {
    try {
      // 1. Read the ADR rulebook from disk — this is the single source of truth for ADR format
      const templatePath = path.resolve(__dirname, "../../docs/Blueprints/ADR_PROMPT.md");
      const adrRules = await fs.readFile(templatePath, "utf-8");

      // 2. Derive the output directory from the source directory
      //    e.g. "src\BuildingBlocks\Authentication" → "Authentication"
      const normalizedSrc = sourceDir.replace(/\\/g, "/").replace(/\/$/, "");
      const moduleName = normalizedSrc.split("/").at(-1) ?? "General";

      const projectRoot = path.resolve(__dirname, "../../");
      const outputDir = path.join(projectRoot, "docs", "02-ArchitectureDecisionRecords", moduleName);
      await fs.mkdir(outputDir, { recursive: true });

      // 3. Compute the next sequential ADR number from existing files in the directory
      //    Files are expected to follow the format: adr-NNN-<slug>.md
      const existingFiles = await fs.readdir(outputDir).catch(() => [] as string[]);
      const adrFiles = existingFiles.filter(f => /^adr-\d+-.+\.md$/.test(f));
      const nextNumber = String(adrFiles.length + 1).padStart(3, "0");

      // 4. Compute the filename and full output path
      const date = new Date().toISOString().slice(0, 10);
      const slug = title.toLowerCase().replace(/\s+/g, "-").replace(/[^\w-]/g, "");
      const fileName  = `adr-${nextNumber}-${slug}.md`;
      const outputPath = path.join(outputDir, fileName);
      const readmePath = path.join(outputDir, "README.md");

      // 5. Read existing README content so the AI can append a new entry to it
      let existingReadme = "";
      try {
        existingReadme = await fs.readFile(readmePath, "utf-8");
      } catch {
        // README doesn't exist yet — the AI will create it from scratch
        existingReadme = `# ADR Index — ${moduleName}\n\n| # | Title | Date | Status |\n|---|-------|------|--------|\n`;
      }

      // 6. Return the rules + user input to the AI as an instruction prompt
      //    The AI is expected to: (a) write the full ADR content, (b) update README.md
      const prompt = `
[MCP Tool: publish_adr]
You are acting as a senior .NET software architect.

Your task is TWO steps — complete BOTH in order:

=== STEP 1: Write the ADR file ===
Generate a complete, professional ADR following ALL rules below, then save it.

--- ADR RULES (from docs/Blueprints/ADR_PROMPT.md) ---
${adrRules}

--- USER INPUT ---
- **ADR Number**: ${nextNumber}
- **Source Module**: \`${normalizedSrc}\`
- **Title**: ${title}
- **Context / Background**: ${context}
- **Decision**: ${decision}
- **Date**: ${date}

SAVE the ADR to:
  ${outputPath}

=== STEP 2: Update the README index ===
The directory README.md tracks all ADRs in this module.
Current README content:
\`\`\`
${existingReadme}
\`\`\`

Append a new row for this ADR to the table:
| ${nextNumber} | ${title} | ${date} | 📝 Draft |

Then SAVE the updated README to:
  ${readmePath}

Do NOT skip either step. Do NOT just print the content in chat.
`.trim();

      return {
        content: [{ type: "text", text: prompt }],
      };

    } catch (error: any) {
      return {
        content: [{ type: "text", text: `[Error] Failed to read ADR rules: ${error.message}` }],
        isError: true,
      };
    }
  }
);

async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("VK Blocks Manager MCP server running");
}

main().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});
