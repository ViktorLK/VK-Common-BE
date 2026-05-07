# Task: Systematize Architectural Rule Numbers (Logical Aliasing)
**ID**: CORE-007
**Status**: 🔵 Low | #Debt
**Target**: `.agents/rules/`
**Ref**: N/A

## 📝 Description
現行の連番（CS.01〜26）を、ドメインベースの論理番号（CS.01, OR.02, AP.03等）へと体系化する。
既存の膨大な監査レポート、バックログ、ソースコードコメントとの互換性を維持するため、以下のステップで実施する：
1. ルール番号の論理マッピング定義（CS=Core Standards, OR=Observability, AP=Architecture Patterns 等）。
2. ルール定義ファイル（.agents/rules/*.md）のヘッダーに論理番号を併記（例: 'CS.01'）。
3. 今後の新規 ADR および監査レポートでの論理番号の使用開始。
4. 既存コード内コメントの段階的なリファクタリング。

## ✅ DoD (Definition of Done)
- [ ] Systematize Architectural Rule Numbers (Logical Aliasing)
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests
