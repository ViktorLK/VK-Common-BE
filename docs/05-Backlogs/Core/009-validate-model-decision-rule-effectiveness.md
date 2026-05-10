# Core: Validate `.agents/rules` Effectiveness (Model Decision Pattern)

## 📌 Context
Validate the effectiveness of architectural rules defined in `.agents/rules` (specifically the `model_decision` trigger in `04-architecture-patterns.md` / `AP.05`) as they apply to model selection and configuration within the VK.Blocks ecosystem. This ensures that the "Industrial DNA" is correctly enforced by AI agents and verifiable via automated tests.

## 🛠 Tasks
- [ ] **Rule Validation (AP.05)**: Develop unit tests to verify that the "Global Default + Local Override" (Args Pattern) correctly handles model-specific parameters as defined in `.agents/rules/04-architecture-patterns.md`.
- [ ] **Trigger Effectiveness**: Verify that the AI agent correctly activates the `model_decision` context when encountering model selection logic, as specified in the rule metadata.
- [ ] **API Boundary Test**: Verify that the pattern prevents implementation-specific types from leaking into Level 1 (Public API) namespaces, per `AP.03`.
- [ ] **README Alignment**: Update `Core` and `AI` module READMEs to include a "Governance" section linking to `.agents/rules/` for transparency and compliance.
- [ ] **Result Pattern Audit**: Ensure all model-related failures use the `Result<T>` pattern with consistent error types, per `CS.01`.
- [ ] **Checklist Compliance**: Perform a self-audit using `.agents/rules/vk-blocks-checklist.md` to ensure zero-tolerance rules (Type A) are satisfied.

## 🎯 Success Criteria
- 100% test coverage for the `Args` merging logic (`args?.Property ?? _options.Property`).
- Zero 'Type A' rule violations in reference implementations.
- Module READMEs explicitly reference and link to the architectural standards in `.agents/rules/`.

## 🔗 Metadata
- **Module**: Core
- **Priority**: High
- **Target**: .agents/rules, VK.Core.DecisionPatterns
- **Reference**: AP.03, AP.05, CS.01, BB.03, PS.04
