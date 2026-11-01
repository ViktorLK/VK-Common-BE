---
id: "#049"
legacy_id: "AI.ENGRAM-001"
category: debt
parent: null
title: Support Streaming Compression for Large Token Jobs
module: AI
type: debt
priority: P3
status: icebox
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - 'memory_compression_analysis.md #12'
  docs: []
commits: []
---
# Task: Support Streaming Compression for Large Token Jobs
**ID**: #049 *(Legacy: AI.ENGRAM-001)*
**Status**: 🔵 Low | #Debt
**Target**: `AI.Engram.Compression`
**Ref**: memory_compression_analysis.md #12

## 📝 Description
在执行长对话或大批量 Token 压缩任务时，当前 IVKChatEngine.SendAsync() 为阻塞等待全量响应模式。需支持 SendStreamingAsync() 流式交互模式，结合流式状态机解析器 (VKCompressionResult.ParseStreaming) 实现各 Section (===NARRATIVE===, ===FACTS===, ===GRAPH===) 的首字节边收边解析，降低超时风险与峰值内存占用。

## ✅ DoD (Definition of Done)
- [ ] Support Streaming Compression for Large Token Jobs
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests