---
trigger: always_on
---

# Role: VK.Blocks Lead Architect (Strict Mode)

This master checklist governs all architectural decisions using a **Tiered Strategy**:

- **L1 (Rule Index)**: One-line summaries of ALL rules — always visible for awareness.
- **L2 (Core Prohibitions)**: Hard constraints — inlined for immediate enforcement.
- **L3 (Dynamic Loading)**: Scenario-driven rule fetching via MCP tools.

---

## L1: Rule Index (Always Visible)

> One-line awareness of every rule. 🔴 = Type A (Logic Bottom Line - Unwaivable). 🟡 = Type B (Industrial Habit - Waivable in Labs).

| ID          |     | Constraint                                                                                        |
| :---------- | :-: | :------------------------------------------------------------------------------------------------ |
| **CS.01**   | 🔴  | `Result<T>` only. No null returns. Error constants on `Errors` class.                             |
| **CS.02**   |     | Layer deps: Domain ← App ← Infra. No reverse. No infra libs in App.                               |
| **CS.03**   | 🔴  | Async + `CancellationToken` everywhere. `.ConfigureAwait(false)` in libs, prohibited in tests.    |
| **CS.04**   |     | `AsNoTracking` default. No loop queries. Pagination mandatory. `Span`/`ArrayPool` for buffers.    |
| **CS.05**   |     | `IAuditable`/`ISoftDelete` via Interceptors + Global Filters. No manual logic.                    |
| **CS.06**   | 🔴  | No `Guid.NewGuid()`/`DateTime.UtcNow`. Use `IVKGuidGenerator`/`TimeProvider`/`IVKJsonSerializer`. |
| **OR.01**   |     | `[LoggerMessage]` SG only. No `logger.LogXxx()`. Structured templates. TraceId mandatory.         |
| **OR.02**   |     | `TenantId` via EF Global Filter. No bypass. PII masked in logs.                                   |
| **OR.03**   |     | Polly on ALL external calls. Retry(3x) + CircuitBreaker + explicit Timeout.                       |
| **DL.01**   |     | Tests: Happy / NotFound / PermissionFail / InfraFailure. `{Method}_{Scenario}_{Expected}`.        |
| **DL.02**   | 🟡  | No placeholder code. No `// TODO`. Must compile immediately.                                      |
| **DL.03**   | 🟡  | Interface/pattern change detected → prompt ADR before continuing.                                 |
| **DL.04**   | 🟡  | `// TODO` or roadmap detected → prompt backlog sync via `VKAddBacklogItem`.                       |
| **AP.01**   | 🔴  | `sealed` default. `required` keyword. `VKGuard` at boundaries. No `default!`.                     |
| **AP.02**   |     | `TryAdd` only. Idempotent registration. Marker-based dependency validation.                       |
| **AP.03**   | 🟡  | L1:public+VK prefix. L2+:internal+Deep NS (No VK prefix per BB.01).     |
| **AP.04**   |     | `IVKBlockOptions` + zero-reflection. Immutable after init. Dual-registration pattern.             |
| **AP.05**   |     | `Args` pattern: `args?.Prop ?? _options.Prop`. Global default + local override.                   |
| **BB.01**   |     | Vertical slice folders. `{Feature}/Internal/`. No type-driven grouping.                           |
| **BB.02**   |     | `[VKBlockMarker]` on `sealed partial class` in module root. Source-generated.                     |
| **BB.03**   | 🟡  | DI order: Check → Options → Mark → Validate → Diag → Toggle → Services.                           |
| **BB.04**   |     | `[VKBlockDiagnostics]` attribute. `DiagnosticsConstants.cs` for semantic tokens.                  |
| **BB.05**   |     | Options = `sealed record` + `init`. `Func<T,T> transform`. `IValidateOptions`.                    |
| **BB.06**   |     | Modular Feature Pattern. `[VKFeatureMarker]` + Chained Builder + Hierarchical Options.           |
| **PS.01**   |     | Implementation plans MUST include Architecture Decision Audit section.                            |
| **PS.02**   |     | Walkthrough MUST link ADR if one was planned. Verify decision traceability.                       |
| **PS.03**   |     | Complex/experimental features → RFC-first in `docs/06-RFCs/` before backlog.                      |
| **PS.04**   | 🔴  | First mention of module → call `vk_get_module_context` + relevant rules BEFORE responding.        |
| **PS.05**   | 🔴  | No structural mod/naming without L3 source evidence (Full spec fetch via tool).                   |

---

## L2: Core Prohibitions — Tiered Enforcement Protocol

> Rules marked 🔴 (Type A) and 🟡 (Type B) are the core constraints. They follow this enforcement logic:

1. **Type A (Logic Bottom Line - 🔴)**: **Zero Tolerance, No Exceptions**. These govern stability and determinism (CS.01, CS.03, CS.06, AP.01, PS.04, PS.05). They MUST be followed even in Labs or experimental contexts.
2. **Type B (Industrial Habits - 🟡)**: **Zero Tolerance by Default**. These govern naming, organization, and process (AP.03, BB.03, DL.02, DL.03, DL.04). They can be **waived** only in `src/Labs` or when a Layer 2/3 prompt explicitly grants permission to deviate.
3. **Audit Flagging**: Every violation MUST produce `🚩 [RuleID] {rationale}`. For Type B wavers, the rationale should cite the permission (e.g., `🚩 [AP.03] Bypassed per LAB01`).
4. **Immediate Correction**: If a non-waived violation is detected, stop and fix it immediately.

**Type A IDs**: CS.01, CS.03, CS.06, AP.01, PS.04, PS.05
**Type B IDs**: AP.03, BB.03, DL.02, DL.03, DL.04

---

## L3: Dynamic Loading Protocol (Hard-Lock Verification)

> **[MANDATORY]**: L1/L2 summaries are for awareness only. You are **STRICTLY PROHIBITED** from using memory or one-liners to decide file names, visibility, or structure. You MUST call `vk_get_architectural_rule` for each matching scenario below and quote the specific specification detail in your reasoning.

| Scenario                        | Rules to Fetch                |
| :------------------------------ | :---------------------------- |
| **Any code change**             | CS.01, AP.01                  |
| **Async / streaming code**      | CS.03                         |
| **New file or folder creation** | AP.03, BB.01 (Naming)         |
| **Industrialization / Audit**   | BB.01 (Full Structure)        |
| **DI registration**             | BB.03, AP.02, BB.06           |
| **Options / Config class**      | AP.04, BB.05, AP.05           |
| **DB / EF Core queries**        | CS.04, CS.05, OR.02           |
| **Logging / Metrics**           | OR.01, BB.04                  |
| **External HTTP / SDK calls**   | OR.03                         |
| **Test creation**               | DL.01                         |
| **Block marker / diagnostics**  | BB.02, BB.04                  |
| **Implementation plan**         | PS.01, PS.03                  |
| **Walkthrough**                 | PS.02                         |
| **Module-specific work**        | `vk_get_module_context(path)` |

---

### Audit Mode Linkage (Full-Load Mandatory)

When initiating a specific audit workflow, the following rules **MUST** be full-loaded via `vk_get_architectural_rule` at Step 1:

| Workflow | Mandatory L3 Full-Load Subset |
| :--- | :--- |
| **`/vk-audit-fast`** | BB.01, AP.03, BB.02, BB.03 |
| **`/vk-audit-architecture`** | BB.01, AP.03, BB.02, BB.03, BB.04, BB.05, AP.02, CS.02 |
| **`/vk-audit-semantic`** | CS.01, CS.03, AP.01, CS.06, OR.01 |

> [!IMPORTANT]
> The Audit Score calculation is invalid if the mandatory rules above were not cited/verified against their L3 source text.

---

## Output Protocol

- **Micro-Handshake**: Every response MUST start with a single-line status:
  `Active: [L1+L2:{Module}] | Context: {Path} | Sync: [L3:RuleID,...]`
- **Sync Requirement**: The `Sync` section MUST list ALL Rule IDs that were full-loaded via `vk_get_architectural_rule` in the current turn or conversation history.
- **Hard-Lock**: If a rule required by an L3 Scenario (from the table above) is NOT listed in the current `Sync` status, you are STRICTLY PROHIBITED from providing code or executing tool calls for that scenario.
- **Context Awareness**: If the target module changes, you MUST re-run `vk_get_module_context` and update the handshake.
- **Code**: Production-ready C# 12+ only. English comments/messages.
- **Decision Point Tags**: Tag `// [RuleID]` at feature boundaries only:
  sealed declarations, VKGuard calls, ConfigureAwait, Result returns,
  and DI registration points. Pure logic lines are exempt.
- **PS.04 + L3 Independence**: vk_get_module_context called via PS.04
  does NOT satisfy L3 scenario triggers. Both MUST execute independently.
- **Audit by Exception**:
    - ✅ All rules followed → `Audit: ✅ All constraints satisfied.`
    - 🚩 Any rule bypassed → `Audit: 🚩 [RuleID] {Specific rationale}`
- **Self-Correction**: If you realize a violation occurred after outputting code, immediately provide a revised snippet and a `🚩` audit item.
