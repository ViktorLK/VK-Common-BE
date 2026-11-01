# ✅ Definition of Done (Global)

> 手動管理ドキュメント（GENERATED ではない）。全 PBI に適用される共通の完了基準。各 PBI 固有の DoD はこれに **追加** されるものであり、緩和はできない。

## 1. Code (category: task / story / debt)

- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る（`verifyBuild: true`）
- [ ] 振る舞いの変更にはテストが追加・更新され、全テストが通る（`BASE-07`, `DL.01`）
- [ ] Type A ルール違反を新たに持ち込んでいない（`CS.01`, `CS.06`, `OR.01`, `AP.01` など）
- [ ] 裸の `// TODO` が無い。残した負債は PBI 化済み（`DL.02`, `DL.04`）
- [ ] Commit メッセージに `[#NNN]` が含まれ、**main にマージ済み**

## 2. Contract / Docs

- [ ] Public API・パターン・DI 順序を変更した場合、ADR を作成済み（`DL.03`）
- [ ] Public API を変更した場合、モジュールの README / Architecture ドキュメントを更新済み
- [ ] 破壊的変更の場合、CHANGELOG に記載済み（`FS-09`）

## 3. Spike / Doc-only

- [ ] 結論を PBI 本文または ADR / RFC に記録済み
- [ ] 後続タスクがあれば PBI 化済み（`allowNoCommits: true` の理由を note に記載）

## 運用ルール

- `completeDod: true` は、上記項目を **実際に確認した後** にのみ使用する。確認を省略するための一括チェックとして使わない。
- 満たせない項目がある場合は Done にせず、`doing` のまま残すか、残作業を新規 PBI に分割する。

