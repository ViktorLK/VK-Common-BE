# ADR-023: Agent Framework Boundaries, Guardrails, and MEAI Decoupling

- **Status**: Accepted
- **Date**: 2026-11-15
- **Area**: AI / Agent Runtime Boundaries, Safety Guardrails & Workflow Architecture
- **Related ADRs**: [ADR-019 (MEAI Migration)](/docs/04-ArchitectureDecisionRecords/AI/adr-019-migration-from-semantic-kernel-to-meai-and-maf.md), [ADR-020 (Pure Isolation of Ports)](/docs/04-ArchitectureDecisionRecords/AI/adr-020-pure-isolation-of-vk-blocks-ai-core-ports.md), [ADR-021 (Deterministic MEAI Middleware Pipeline)](/docs/04-ArchitectureDecisionRecords/AI/adr-021-deterministic-meai-middleware-pipeline.md), [ADR-022 (Compile-Time AIFunction Generation and Multi-Agent Workflows)](/docs/04-ArchitectureDecisionRecords/AI/adr-022-compile-time-aifunction-and-agent-workflows.md)

---

## Context

`VK.Blocks.AI.AgentFramework`・・icrosoft Agent Framework / MAF 螳溯｣・ヱ繝・こ繝ｼ繧ｸ・峨・繝ｪ繝輔ぃ繧ｯ繧ｿ繝ｪ繝ｳ繧ｰ縺ｫ蜈育ｫ九■縲∵里蟄倥・螳溯｣・♀繧医・繝代ャ繧ｱ繝ｼ繧ｸ蠅・阜縺ｫ縺ｯ莉･荳九・讒矩逧・ｪｲ鬘後′遒ｺ隱阪＆繧後※縺・◆・・
1. **荳贋ｽ阪・繝斐い螻､縺ｨ縺ｮ邨仙粋驕主､・(`ARCH-01`, `ARCH-02`, `ARCH-04`)**:
   `AI.AgentFramework` 縺・`VK.Blocks.AI.MEAI` 縺ｸ縺ｮ繝励Ο繧ｸ繧ｧ繧ｯ繝亥盾辣ｧ繧堤峩謗･菫晄戟縺励～VKAIMEAIBlock` 縺ｫ萓晏ｭ倥＠縺ｦ縺・◆縲ゅ∪縺溘～IVKAgentThreadStore` 繧・`AgentThreadStoreExtensions` 遲峨・邏皮ｲ句･醍ｴ・・諡｡蠑ｵ縺・`AI.AgentFramework` 蜀・↓鄂ｮ縺九ｌ縺ｦ縺翫ｊ縲∝･醍ｴ・□縺代ｒ蜿ら・縺励◆縺・･ｭ蜍吶さ繝ｼ繝峨↓繧・MAF 萓晏ｭ倥′莨晄眺縺励※縺・◆縲・2. **繝・・繝ｫ螳溯｡後ヱ繧､繝励Λ繧､繝ｳ縺ｮ驥崎､・*:
   `IVKAtomicTool` 縺ｮ繝帙Ρ繧､繝医Μ繧ｹ繝磯∈蛻･繝ｻ繝輔ぅ繝ｫ繧ｿ螳溯｡後・邨先棡蛻・ｊ隧ｰ繧√Ο繧ｸ繝・け縺・`AI.MEAI` 縺ｨ `AI.AgentFramework` 縺ｧ蛟句挨縺ｫ螳溯｣・＆繧悟ｾ励ｋ讒矩縺ｨ縺ｪ縺｣縺ｦ縺翫ｊ縲∵価隱榊宛蠕｡・・IVKAtomicToolApprovalHandler`・峨ｒ蜷ｫ繧√◆荳雋ｫ縺励◆螳牙・繧ｲ繝ｼ繝医′蜈ｱ譛峨＆繧後※縺・↑縺九▲縺溘・3. **LLM 荳ｻ蟆主ｮ溯｡後↓縺翫￠繧九ワ繝ｼ繝牙｢・阜・医ぎ繝ｼ繝峨Ξ繝ｼ繝ｫ・峨・谺螯・*:
   蝗ｺ螳壹ヱ繧､繝励Λ繧､繝ｳ・・AI.Cortex` 遲会ｼ峨→逡ｰ縺ｪ繧翫√お繝ｼ繧ｸ繧ｧ繝ｳ繝医・ LLM 閾ｪ霄ｫ縺梧ｬ｡縺ｮ繧ｹ繝・ャ繝励ｄ繝・・繝ｫ蜻ｼ縺ｳ蜃ｺ縺励ｒ豎ｺ螳壹☆繧九◆繧√～MaxSteps`繝ｻ邏ｯ遨阪ヨ繝ｼ繧ｯ繝ｳ莠育ｮ暦ｼ・MaxTotalTokens`・峨・邱上ヤ繝ｼ繝ｫ蜻ｼ蜃ｺ荳企剞・・MaxToolCalls`・峨・蜷御ｸ繝・・繝ｫ騾｣邯壼他蜃ｺ縺ｮ繝ｫ繝ｼ繝玲､懃衍繝ｻ繝・・繝ｫ蜃ｺ蜉幃聞縺ｮ蛻・ｊ隧ｰ繧√・螳溯｡後ち繧､繝繧｢繧ｦ繝医→縺・▲縺滓ｱｺ螳夊ｫ也噪荳企剞縺悟ｿ・医〒縺ゅ▲縺溘・4. **證鈴ｻ吶ヵ繧ｩ繝ｼ繝ｫ繝舌ャ繧ｯ縺ｫ繧医ｋ `AP.06` 驕募渚繝ｪ繧ｹ繧ｯ縺翫ｈ縺ｳ讖溯・縺ｮ譛ｪ謨ｴ蛯・*:
   `LLMBased` 繧ｰ繝ｫ繝ｼ繝鈴∈螳壹〒繝｢繝・Ν蠢懃ｭ斐′譖匁乂縺ｾ縺溘・荳肴ｭ｣縺ｪ蝣ｴ蜷医↓證鈴ｻ咏噪縺ｫ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ縺吶ｋ謖吝虚縺ｮ遖∵ｭ｢縲｀AF `Handoff` 蜊碑ｪｿ縲∵ｱｺ螳夊ｫ也噪 `WorkflowBuilder` + 繝√ぉ繝・け繝昴う繝ｳ繝亥ｾｩ蜈・√・繝ｭ繝ｳ繝励ヨ/險俶・繧ｷ繧ｹ繝・Β・・Psyche`/`Engram`・峨ｒ逶ｴ謗･蜿ら・縺励↑縺・僑蠑ｵ繝輔ャ繧ｯ縲√↑繧峨・縺ｫ `ActivitySource`/`Meter` 險郁｣・′荳崎ｶｳ縺励※縺・◆縲・
---

## Options Considered

1. **Option 1: `AI.AgentFramework -> AI.MEAI` 逶ｴ謗･萓晏ｭ倥→螂醍ｴ・・蜷悟ｱ・ｒ邯ｭ謖√＠縺溘∪縺ｾ讖溯・霑ｽ蜉**
   - Pros: `IChatClient` 隗｣豎ｺ繧・・繝・ヱ繝ｼ螳溯｣・ｒ `AI.MEAI` 縺九ｉ逶ｴ謗･豬∫畑縺ｧ縺阪ｋ縲・   - Cons: 讌ｭ蜍吶い繝励Μ繧・ｸｭ髢薙Λ繧､繝悶Λ繝ｪ縺後お繝ｼ繧ｸ繧ｧ繝ｳ繝亥･醍ｴ・ｒ蜿ら・縺吶ｋ縺溘・縺ｫ MAF 縺梧耳遘ｻ逧・↓豺ｷ蜈･縺吶ｋ縲ゅヤ繝ｼ繝ｫ繝輔ぅ繝ｫ繧ｿ縺翫ｈ縺ｳ謇ｿ隱阪ヱ繧､繝励Λ繧､繝ｳ縺ｮ莠碁㍾螳溯｣・Μ繧ｹ繧ｯ縺梧ｮ九ｊ縲∝腰荳讖溯・閧･螟ｧ蛹厄ｼ・Runtime/` 蜊倅ｸ繝輔か繝ｫ繝・峨↓繧医▲縺ｦ讖溯・蜊倅ｽ阪・繧ｫ繧ｹ繧ｱ繝ｼ繝臥┌蜉ｹ蛹厄ｼ・BB.06`・峨ｂ讖溯・縺励↑縺・・
2. **Option 2: `VK.Blocks.AI` 縺ｸ縺ｮ邏皮ｲ句･醍ｴ・寔邏・+ DI 邨檎罰 `IChatClient` 逍守ｵ仙粋 + 3繧ｹ繝ｩ繧､繧ｹ蛻・牡縺ｨ繝上・繝峨・繧ｬ繝ｼ繝峨Ξ繝ｼ繝ｫ邨ｱ蜷・(謗｡逕ｨ)**
   - Pros:
     - 縺吶∋縺ｦ縺ｮ繧ｨ繝ｼ繧ｸ繧ｧ繝ｳ繝亥･醍ｴ・・繧､繝吶Φ繝医Δ繝・Ν繝ｻ繧ｬ繝ｼ繝峨Ξ繝ｼ繝ｫ險ｭ螳壹・`IVKAgentThreadStore` / `InMemoryAgentThreadStore`繝ｻ`IVKAgentWorkflowRunner` / `NoOpAgentWorkflowRunner`繝ｻ蜈ｱ譛・`VKAtomicToolPipeline` 繧・`VK.Blocks.AI` 縺ｸ髮・ｴ・・     - `AI.AgentFramework` 縺九ｉ `VK.Blocks.AI.MEAI` 縺ｸ縺ｮ繝励Ο繧ｸ繧ｧ繧ｯ繝亥盾辣ｧ繧貞ｮ悟・縺ｫ謦､蜴ｻ縺励．I 荳翫・辟｡繧ｭ繝ｼ / `ModelProfile` 繧ｭ繝ｼ莉倥″ `IChatClient` 螂醍ｴ・・縺ｿ縺ｧ隗｣豎ｺ縲・     - `[VKFeature]` 縺ｫ繧医ｊ `SingleAgent/`繝ｻ`MultiAgent/`繝ｻ`AgentWorkflow/` 縺ｮ 3 縺､縺ｮ蝙ら峩繧ｹ繝ｩ繧､繧ｹ縺ｸ蛻・牡縲・     - `DelegatingChatClient` + `IVKAtomicToolFilter` 縺ｫ繧医ｋ繝上・繝峨・繧ｬ繝ｼ繝峨Ξ繝ｼ繝ｫ縺ｨ蜈ｱ譛峨ヤ繝ｼ繝ｫ謇ｿ隱阪ヱ繧､繝励Λ繧､繝ｳ縲√＆繧峨↓ `IVKAgentContextProvider` 諡｡蠑ｵ繝昴う繝ｳ繝医ｒ遒ｺ遶九・   - Cons:
     - `AI.AgentFramework` 蜀・↓譛蟆城剞縺ｮ MAF/MEAI 逕ｨ `AgentChatMessageMapper`・育ｴ・70 陦鯉ｼ峨′蠢・ｦ√→縺ｪ繧翫～AI.MEAI` 縺ｮ繝ｪ繝・メ縺ｪ繝槭Ν繝√Δ繝ｼ繝繝ｫ螟画鋤・育判蜒・髻ｳ螢ｰ/謗ｨ隲悶ヱ繝ｼ繝亥､画鋤・峨→縺ｯ蠖ｹ蜑ｲ縺ｫ蠢懊§縺ｦ繧ｹ繧ｳ繝ｼ繝励ｒ髯仙ｮ壹☆繧玖ｨｭ險亥愛譁ｭ繧剃ｼｴ縺・・
---

## Decision & Rationale

**Option 2 繧呈治逕ｨ縺吶ｋ縲・*

1. **Pure Contracts & Co-located Defaults in `VK.Blocks.AI` (`BASE-01`, `ARCH-05`)**:
   - `IVKAgent`縲～IVKAgentFactory`縲～IVKAgentGroup`縲～IVKAgentThreadStore`・・ `InMemoryAgentThreadStore`・峨～IVKAgentWorkflowRunner`・・ `NoOpAgentWorkflowRunner`・峨～IVKAgentContextProvider`縲～IVKAtomicToolApprovalHandler`縲～VKAgentDefinition`縲～VKAgentGuardrailOptions`縲～VKAgentStreamEvent`縲～VKAgentRunResult`縲～VKAgentWorkflowDefinition`縲～VKAgentErrors` 繧・`VK.Blocks.AI/Agents` 縺ｫ驟咲ｽｮ縺吶ｋ縲・   - `IVKAtomicToolFilter` 繝代う繝励Λ繧､繝ｳ縲√・繝ｯ繧､繝医Μ繧ｹ繝磯∈蛻･縲√ヤ繝ｼ繝ｫ蜃ｺ蜉帛・繧願ｩｰ繧√∵撫諢溘ヤ繝ｼ繝ｫ遒ｺ隱搾ｼ・VKToolApprovalMode`・峨ｒ `VKAtomicToolPipeline` 縺ｫ荳譛ｬ蛹悶＠縲～AI.MEAI`・・AtomicToolAdapter`・峨→ `AI.AgentFramework`・・AgentAtomicToolBridge`・峨・蜿梧婿縺悟酔荳繝代う繝励Λ繧､繝ｳ繧貞ｮ溯｡後☆繧九・
2. **Decoupling from `VK.Blocks.AI.MEAI` (`ARCH-01`, `ARCH-02`)**:
   - `VK.Blocks.AI.AgentFramework` 縺ｯ `VK.Blocks.AI.MEAI` 繧貞盾辣ｧ縺帙★縲√・繧ｹ繝茨ｼ・omposition Root・峨′逋ｻ骭ｲ縺励◆譌｢螳壹∪縺溘・蜷榊燕莉倥″・・ModelProfile`・峨・ `IChatClient` 繧・`IServiceProvider` / `IKeyedServiceProvider` 邨檎罰縺ｧ隗｣豎ｺ縺吶ｋ縲・   - 繧ｯ繝ｩ繧､繧｢繝ｳ繝医′蟄伜惠縺励↑縺・ｴ蜷医ｄ譛ｪ逋ｻ骭ｲ繝励Ο繝輔ぃ繧､繝ｫ謖・ｮ壽凾縺ｯ繧ｵ繧､繝ｬ繝ｳ繝医↓繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ縺帙★縲∵・遉ｺ逧・↑ `VKResult.Failure`・・VKAIAgentFrameworkErrors.ChatClientNotRegistered` / `ProfileNotFound`・峨ｒ霑斐☆・・AP.06`・峨・
3. **Three `[VKFeature]` Vertical Slices (`BB.01`, `BB.06`)**:
   - **`SingleAgent/`**: `DefaultAgentFactory`・亥ｮ｣險逧・`VKAgentDefinition` 縺九ｉ縺ｮ逕滓・縺ｨ譌｢螳・蛟句挨蠑墓焚蜷域・縲√し繝悶お繝ｼ繧ｸ繧ｧ繝ｳ繝医・ `AsTool` 蛹厄ｼ峨～DefaultClientAgent`・磯壼ｸｸ繝ｻ繧ｹ繝医Μ繝ｼ繝溘Φ繧ｰ荳｡蟇ｾ蠢懊∝｢怜・繧､繝吶Φ繝亥・蜉帙∽ｸ譎ゅせ繝ｬ繝・ラ繧｢繝繝励ち繝ｼ縺ｨ縺励※縺ｮ `IVKAgentThreadStore`縲∝ｱ･豁ｴ莉ｶ謨ｰ/繝医・繧ｯ繝ｳ謨ｰ繝医Μ繝溘Φ繧ｰ縲～IVKAgentContextProvider` 蜻ｼ縺ｳ蜃ｺ縺暦ｼ峨・   - **`MultiAgent/`**: `DefaultAgentGroupRunner`・・Sequential`縲～Concurrent`縲～RoundRobin`縲～LLMBased`縲～Handoff`・峨ＡLLMBased` 縺ｮ荳肴ｭ｣蠢懃ｭ斐・蜴溷援螟ｱ謨励→縺励～AllowFallbackToRoundRobin = true` 縺ｮ譏守､ｺ繧ｪ繝励ヨ繧､繝ｳ譎ゅ・縺ｿ莉｣譖ｿ驕ｸ謚槭ｒ險ｱ蜿ｯ縺吶ｋ・・AP.06`・峨・   - **`AgentWorkflow/`**: `DefaultAgentWorkflowRunner` 縺ｫ繧医ｋ MAF `WorkflowBuilder` + `CheckpointManager` 繧ｰ繝ｩ繝募ｮ溯｡後♀繧医・繝√ぉ繝・け繝昴う繝ｳ繝医°繧峨・蜀埼幕縲・
4. **Hard Runtime Guardrails & Context Extensibility (`ARCH-09`, `ARCH-12`)**:
   - `AgentRunBudgetTracker`縲～AgentStepGuardrailChatClient`縲～AgentToolGuardrailFilter` 繧貞ｮ溯｡悟｢・阜縺ｫ驟咲ｽｮ縺励～MaxSteps`繝ｻ`MaxTotalTokens`繝ｻ`MaxToolCalls`繝ｻ`MaxConsecutiveIdenticalToolCalls`繝ｻ`MaxToolResultLength`繝ｻ`Timeout` 繧貞ｼｷ蛻ｶ縺吶ｋ縲・   - System Prompt 讒狗ｯ会ｼ・AI.Psyche`・峨ｄ險俶・蜿門ｾ暦ｼ・AI.Engram`・峨∈縺ｮ萓晏ｭ倥・謖√◆縺壹～IVKAgentContextProvider` 繧帝壹§縺ｦ Cortex 螻､繧・い繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ螻､縺悟虚逧・↓繧ｳ繝ｳ繝・く繧ｹ繝医ｄ謖・､ｺ繧呈ｳｨ蜈･縺ｧ縺阪ｋ險ｭ險医→縺吶ｋ縲・
5. **Observability (`OR.01`, `BB.04`)**:
   - `[VKBlockDiagnostics]` 縺ｨ `AIAgentFrameworkDiagnostics` 縺ｫ繧医ｊ `ActivitySource`・亥ｮ溯｡後・繧ｹ繝・ャ繝励・繧ｰ繝ｫ繝ｼ繝励・繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ繧ｹ繝代Φ・峨～Meter`・亥ｮ溯｡悟屓謨ｰ繝ｻ謇隕∵凾髢薙・繧ｹ繝・ャ繝玲焚繝ｻ繧ｬ繝ｼ繝峨Ξ繝ｼ繝ｫ驕募渚蝗樊焚・峨√♀繧医・ `[LoggerMessage]` 讒矩蛹悶Ο繧ｰ・・TenantId` / `TraceId` 莨晄眺・峨ｒ謠蝉ｾ帙☆繧九・
6. **Scope Exclusions (Out of Scope)**:
   - 繝｢繝・Ν霈ｸ騾・繝励Ο繝舌う繝繝ｼ驕ｩ蠢懶ｼ・AI.MEAI`・峨√・繝ｭ繝ｳ繝励ヨ逕滓・・・AI.Psyche`・峨∬ｨ俶・邂｡逅・ｼ・AI.Engram`・峨∵ｧ矩蛹門・蜉帑ｺ､貂会ｼ・AI.Eidos`・峨∝崋螳壻ｼ夊ｩｱ繝輔Ο繝ｼ・・AI.Cortex`・峨∝刀雉ｪ隧穂ｾ｡・・AI.Probe`・峨∵ｰｸ邯咼B繧ｹ繝ｬ繝・ラ繧ｹ繝医い繧・UI/MCP 繧ｵ繝ｼ繝舌・縺ｮ螳溯｣・・譛ｬ繝代ャ繧ｱ繝ｼ繧ｸ縺ｫ縺ｯ蜷ｫ繧√↑縺・・
---

## Consequences & Trade-offs

- **繝励Λ繧ｹ縺ｮ蠖ｱ髻ｿ**:
  - 讌ｭ蜍吶さ繝ｼ繝峨′ `VK.Blocks.AI` 縺ｮ縺ｿ縺ｧ繧ｨ繝ｼ繧ｸ繧ｧ繝ｳ繝亥･醍ｴ・ｄ繝・せ繝育畑繝輔ぉ繧､繧ｯ/InMemory 繧ｹ繝医い繧貞茜逕ｨ蜿ｯ閭ｽ縺ｨ縺ｪ繧翫｀AF 繧・LLM 繝励Ο繝舌う繝繝ｼ萓晏ｭ倥・豬∝・繧帝・譁ｭ縲・  - 蜊倅ｸ繝ｻ蜊碑ｪｿ繝ｻ繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ縺ｮ 3 繧ｹ繝ｩ繧､繧ｹ迢ｬ遶句喧縺翫ｈ縺ｳ繝上・繝峨・繧ｬ繝ｼ繝峨Ξ繝ｼ繝ｫ縺ｫ繧医ｊ縲√お繝ｼ繧ｸ繧ｧ繝ｳ繝医・辟｡髯舌Ν繝ｼ繝励ｄ繝医・繧ｯ繝ｳ證ｴ襍ｰ繧呈ｱｺ螳夊ｫ也噪縺ｫ髦ｲ豁｢蜿ｯ閭ｽ縲・  - 繝・・繝ｫ謇ｿ隱阪・繝輔ぅ繝ｫ繧ｿ蜃ｦ逅・′ `AI.MEAI` 縺ｨ螳悟・蜈ｱ騾壼喧縺輔ｌ縲∽ｸ雋ｫ縺励◆繧ｻ繧ｭ繝･繝ｪ繝・ぅ蠅・阜繧堤ｶｭ謖√・- **繝槭う繝翫せ縺ｮ蠖ｱ髻ｿ / 繝医Ξ繝ｼ繝峨が繝・*:
  - 繝帙せ繝医い繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ Composition Root 縺ｧ縺ｯ縲√Δ繝・Ν蛻ｩ逕ｨ繧呈怏蜉ｹ縺ｫ縺吶ｋ縺溘ａ `AddVKAIMEAIBlock()`・医∪縺溘・莠呈鋤縺ｮ `IChatClient` 逋ｻ骭ｲ・峨ｒ譏守､ｺ逧・↓菴ｵ逕ｨ縺吶ｋ蠢・ｦ√′縺ゅｋ縲・  - 豌ｸ邯夂噪縺ｪ Human-in-the-loop 繝昴・繧ｺ/繝ｬ繧ｸ繝･繝ｼ繝繧・・謨｣繝√ぉ繝・け繝昴う繝ｳ繝域ｰｸ邯壼喧繧貞茜逕ｨ縺吶ｋ蝣ｴ蜷医・縲√い繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ螻､縺ｾ縺溘・蟆ら畑繧､繝ｳ繝輔Λ繝代ャ繧ｱ繝ｼ繧ｸ縺ｧ繝√ぉ繝・け繝昴う繝ｳ繝井ｿ晏ｭ倥♀繧医・ `IVKAgentThreadStore` 豌ｸ邯壼ｮ溯｣・ｒ謠蝉ｾ帙☆繧句ｿ・ｦ√′縺ゅｋ縲・