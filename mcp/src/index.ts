import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
import fs from "fs/promises";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const server = new McpServer({
  name: "vk-blocks-manager",
  version: "1.0.0",
});


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
      const templatePath = path.resolve(__dirname, "../../docs/Blueprints/AdrPrompt.md");
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

--- ADR RULES (from docs/Blueprints/AdrPrompt.md) ---
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

// Register tool for listing building blocks
server.tool(
  "list_building_blocks",
  "Lists all available BuildingBlock modules in the VK-Common-BE solution, including their internal project dependencies.",
  {},
  async () => {
    try {
      const blocksDir = path.resolve(__dirname, "../../src/BuildingBlocks");
      const entries = await fs.readdir(blocksDir, { withFileTypes: true });
      const modules = [];

      for (const entry of entries) {
        if (entry.isDirectory()) {
          const moduleName = entry.name;
          const modulePath = path.join(blocksDir, moduleName);
          const files = await fs.readdir(modulePath).catch(() => []);
          const csprojFile = files.find(f => f.endsWith(".csproj"));

          if (csprojFile) {
            let dependencies: string[] = [];
            try {
              const csprojContent = await fs.readFile(path.join(modulePath, csprojFile as string), "utf-8");
              const regex = /<ProjectReference\s+Include="([^"]+)"/g;
              let match;
              while ((match = regex.exec(csprojContent)) !== null) {
                const depPath = match[1];
                const depName = path.basename(depPath as string, ".csproj");
                dependencies.push(depName);
              }
            } catch (err) {
              // Ignore read errors
            }

            modules.push({ module: moduleName, project: csprojFile, dependencies });
          }
        }
      }

      const prompt = `
[MCP Tool: list_building_blocks]
Here are the currently available BuildingBlocks in the solution:
${JSON.stringify(modules, null, 2)}
`.trim();

      return {
        content: [{ type: "text", text: prompt }],
      };

    } catch (error: any) {
      return {
        content: [{ type: "text", text: `[Error] Failed to list BuildingBlocks: ${error.message}` }],
        isError: true,
      };
    }
  }
);

// Register tool for auditing EF Core migrations
server.tool(
  "audit_ef_migrations",
  "Scans EF Core migration files for potentially destructive operations like DROP TABLE or DROP COLUMN.",
  {
    migrationsDir: z.string().describe("Path to the EF Core Migrations directory (e.g. 'src/BuildingBlocks/Identity/Migrations')."),
  },
  async ({ migrationsDir }) => {
    try {
      const absPath = path.resolve(migrationsDir);
      const files = await fs.readdir(absPath);
      const csFiles = files.filter(f => f.endsWith(".cs") && !f.endsWith(".Designer.cs") && !f.endsWith("ModelSnapshot.cs"));

      const destructivePatterns = [
        "migrationBuilder.DropTable",
        "migrationBuilder.DropColumn",
        "migrationBuilder.DropForeignKey",
        "migrationBuilder.DropIndex",
        "migrationBuilder.Sql(\\\"DROP",
        "migrationBuilder.Sql(\\\"DELETE",
        "migrationBuilder.Sql(\\\"TRUNCATE"
      ];

      const findings: { file: string, line: number, code: string }[] = [];

      for (const file of csFiles) {
        const filePath = path.join(absPath, file);
        const content = await fs.readFile(filePath, "utf-8");
        const lines = content.split('\\n');

        lines.forEach((line, index) => {
          if (destructivePatterns.some(p => line.includes(p) || line.includes(p.replace("\\\"", "'")))) {
            findings.push({ file, line: index + 1, code: line.trim() });
          }
        });
      }

      const prompt = `
[MCP Tool: audit_ef_migrations]
I have scanned the migrations in \`${migrationsDir}\`.

${findings.length > 0
  ? "CRITICAL WARNING: Found " + findings.length + " potentially destructive operations:\\n" + findings.map(f => "- " + f.file + ":" + f.line + " -> `" + f.code + "`").join('\\n') + "\\n\\nYour task: WARN the user about these destructive changes and ask for explicit confirmation before they apply this migration to production. Explain the impact of each finding."
  : "SUCCESS: No obvious destructive operations (DROP TABLE, DROP COLUMN) were found in the C# migration files. It appears safe."}
`.trim();

      return {
        content: [{ type: "text", text: prompt }],
      };

    } catch (error: any) {
      return {
        content: [{ type: "text", text: `[Error] Failed to audit migrations: ${error.message}` }],
        isError: true,
      };
    }
  }
);

// Register tool for generating Integration Tests from OpenAPI/Swagger JSON
server.tool(
  "generate_api_tests",
  "Parses an OpenAPI (Swagger) JSON file and generates instructions to create integration tests for all endpoints.",
  {
    swaggerJsonPath: z.string().describe("Path to the swagger.json file."),
  },
  async ({ swaggerJsonPath }) => {
    try {
      const absPath = path.resolve(swaggerJsonPath);
      const content = await fs.readFile(absPath, "utf-8");
      const swagger = JSON.parse(content);

      if (!swagger.paths) {
        throw new Error("Invalid OpenAPI JSON: 'paths' object not found.");
      }

      const endpoints: { path: string, method: string, summary: string }[] = [];

      for (const [apiPath, methods] of Object.entries(swagger.paths)) {
        for (const [method, details] of Object.entries(methods as Record<string, any>)) {
          endpoints.push({
            path: apiPath,
            method: method.toUpperCase(),
            summary: details.summary || "No summary"
          });
        }
      }

      const prompt = `
[MCP Tool: generate_api_tests]
I have parsed the OpenAPI specification from \`${swaggerJsonPath}\`.
Found ${endpoints.length} endpoints.

Endpoints:
${endpoints.map(e => "- [" + e.method + "] " + e.path + " (" + e.summary + ")").join('\\n')}

Your task:
Generate C# Integration Tests (using WebApplicationFactory and xUnit) for the endpoints listed above.
Ensure you test:
1. Happy Path (200 OK or 201 Created)
2. Validation Failure (400 Bad Request)
3. Unauthorized/Forbidden (401/403)
`.trim();

      return {
        content: [{ type: "text", text: prompt }],
      };

    } catch (error: any) {
      return {
        content: [{ type: "text", text: `[Error] Failed to parse Swagger file: ${error.message}` }],
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
