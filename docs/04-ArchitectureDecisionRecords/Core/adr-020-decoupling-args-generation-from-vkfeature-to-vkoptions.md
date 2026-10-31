# ADR-CORE-020: Decoupling Args Generation from VKFeature to VKOptions

## Status
Accepted

## Context
以前のアーキテクチャでは、`[VKFeature]` 属性に `ArgsGenerationMode` および `ArgsBaseType` プロパティが配置されていました（例：`[VKFeature(typeof(VKAIBlock), OptionsType = typeof(VKChatOptions), ArgsGenerationMode = VKArgsGenerationMode.Implicit)]`）。
しかし、この設計には以下の問題が存在していました：
1. **関心の分離（SoC）の破綻**: `Feature` は DI サービス登録スライス（DI Composition Hook）を定義する責務を持ちます。一方、`Args` 生成はリクエストスコープの動的パラメータ上書き（AP.05）であり、`Options` 自体のデータ契約（Contract）に属します。DI 登録属性がデータモデルのコード生成を制御するという依存関係の逆転が生じていました。
2. **モデル凝集度の低下**: `Options` 側で `[VKRequestOverride]` を宣言しても、実際のコード生成モード（`Implicit`/`Explicit`/`None`）は別のファイルにある `*Feature.cs` を確認しなければ判別できませんでした。
3. **安全でないデフォルト値**: `VKOptionsAttribute` が以前 `ArgsMode = Explicit` かつ `GenerateMerge = true` をデフォルトとしていたため、静的インフラ Options に不用意に属性を付与すると不要なシンボルが生成される懸念がありました。

## Decision
1. **責務の返還**:
   - `[VKFeature]` から `ArgsGenerationMode` の責務を剥奪（`[Obsolete]` 化）。
   - すべての動的パラメータ生成設定を `Options` レコード側の `[VKOptions(ArgsMode = ..., GenerateMerge = ..., ArgsBaseType = ...)]` に集約。
2. **安全なオプトイン（Zero Overhead）**:
   - `[VKOptions]` のデフォルト値を `ArgsMode = None`、`GenerateMerge = false` に改定。明示的に指定しない限り、余計なコード生成を一切行わない。
3. **全棟統一**:
   - 動的 Args を要求する 55 箇所の Feature/Options（`AI.*`、`VectorSearch`、`VectorIngest`、`Authorization`）を新方式に移行。
   - 残りの静的 Options も `[VKOptions]` で統一マークし、契約体系の一貫性を確立。
4. **生成器の堅牢化**:
   - `VKFeatureGenerator` における `optionsTypeSymbol` の読み出し順序の不具合を修正。
   - `VKValidatorGenerator` において、対象プロジェクトが `VK.Blocks.Validation` を参照している場合のみ Validator 実装を出力するガードを追加。

## Consequences
- **Positive**:
  - `Feature` と `Options` の責務がクリーンに分離され、モデルの凝集度が最大化。
  - `Options` クラスを見るだけで、リクエストレベルでのオーバーライド挙動が完结して理解可能（Self-documenting）。
  - ソリューション全体（全 83 プロジェクト）のクリーンビルドを達成。
- **Negative / Trade-offs**:
  - `[VKFeature]` の既存 `ArgsGenerationMode` 引数を移行するための機械的置換作業が発生した（本 ADR 導入により完了）。
