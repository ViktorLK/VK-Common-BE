# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.AI.Afferent モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Input Signal Preprocessing & Security (求心性入力前処理とセキュリティ)

#### [ADR-001: Incoming Guardrails and Preprocessing via AI Afferent Block](./adr-001-incoming-guardrails-and-preprocessing-via-ai-afferent-block.md)

**Status**: ✅ Accepted  
**概要**: 音声の文字起こし、テキスト分割、および有害コンテンツの検知といった「入力信号に対する前処理と防衛ガードレール」を独立してカプセル化する `AI.Afferent` ビルディングブロックの新設。  
**キーワード**: Input Guardrails, Transcription, AI.Afferent, Safety Checks

---

## 🎯 ADR の読み方ガイド

### 安全・防衛設計の理解用
1. **ADR-001**: 悪意ある入力や不適切なコンテンツが LLM に到達する前に、どのように `AI.Afferent` を介して遮断されているかを理解するために読んでください。

**Last Updated**: 2026-06-10  
**Total ADRs**: 1
