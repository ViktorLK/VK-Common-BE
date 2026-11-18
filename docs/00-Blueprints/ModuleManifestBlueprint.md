# Module Manifest Blueprint

`src/BuildingBlocks/{Module}/.prompts/module-manifest.md` の標準テンプレートおよび規約です。
L1 `manifest_spec` に準拠し、`vk_be_manifest_lint`（[McpTools.Manifest.cs](/src/Tools/McpServer/Internal/McpTools.Manifest.cs)）で機械検証されます。

## テンプレート

```markdown
<manifest requires="{L3 rule IDs only}" allowed="{modules / packages}" forbidden="{modules / SDKs}">

# VK.Blocks {Module} Manifest

{One sentence: owns X; delegates Y.}

## 1. Ownership
- **Owns**: {OWN-* row IDs}
- **Delegates**: {OWN-* → Module}

## 2. Boundaries
- **Non-goals**: {Deliberate exclusions} (reconsider when {condition}).
- {Only non-obvious exclusions or counterintuitive boundaries; never restate Delegates or allowed-enforced reverse-dependency bans.}

## 3. Public Surface
- {≤ 5 lines, principles only, no method signatures (BASE-12).}

## 4. Defaults & Overrides
- {≤ 3 lines; fallback implementations & override mechanism; "None" is valid.}

## 5. Invariants
### {Topic}
- **{PFX}-{NN}** MUST / never ...  (reason: ... — only when counterintuitive)

## 6. Registration & Diagnostics
- {Optional; reference L3 rules instead of restating.}

</manifest>
```

## 規約（Lint 検証対象）

1. **属性 (`<manifest>`)**:
   - `requires`: L3 ルール ID のみ（`CS.xx`, `AP.xx`, `BB.xx`, `OR.xx`, `DL.xx`, `PS.xx`）。L0–L2（`BASE-*`, `ARCH-*`, `FS-*`）は常駐のため記載しない。
   - `allowed` / `forbidden`: カンマ区切り。**`forbidden` が `allowed` より優先**され、ワイルドカードは**末尾 `*` の前方一致のみ**許可（例: `Qdrant.*`。中間の `*` は不可）。`.csproj` の `ProjectReference` / `PackageReference` と照合される。抽象パッケージでは過度に広い `Microsoft.Extensions.*` を避け、実参照を明記する。
2. **セクション順序と上限**:
   - `1. Ownership` → `2. Boundaries` → `3. Public Surface`（最大 5 行）→ `4. Defaults & Overrides`（最大 3 行）→ `5. Invariants`（最大 15 ルール）→ `6. Registration & Diagnostics`（任意）。
   - 15 ルールを超える場合は詳細を `/docs/03-Architecture/{Domain}/` に逃がしてリンクする。
3. **Ownership 参照**:
   - `Owns` / `Delegates` は該当機能 Group の Ownership Map（AI 系は [ownership-map.md](/docs/03-Architecture/AI/ownership-map.md)）の `OWN-*` 行 ID を参照する。
   - Ownership Map は原則として**複数モジュールが協調する機能 Group**（例: `AI.*`、`Vector*`、`Resilience` + `Web.Resilience`）ごとに `/docs/03-Architecture/{Group}/ownership-map.md` として作成し、他 Group の `ownership-map.md`（特に `AI/ownership-map.md`）へ混在させない。単一モジュールで完結する Group は原則不要（`vk_be_manifest_lint` 実行時は該当 Group の `mapPath` を明示指定する）。
4. **Invariant ID**:
   - Ownership Map の `Invariant Prefix Registry` に登録された 3 文字接頭辞 `{PFX}-{NN}` を使用し、番号は不変とする（廃止時は `[Retired]` を付けて残す）。

## 執筆ルール（原則）

1. **責務境界と不変条件のみを記述する**:
   - 本文には「自分が何を所有し、何をしてはならず、どの不変条件を守るか」だけを書く。内部実装・アルゴリズム・手順の詳細は書かない（必要なら `/docs/03-Architecture/` に分離する）。
2. **委譲と Non-goals を分離する**:
   - Ownership Map 上で他モジュールに委譲済みの責務は `## 1. Ownership` の `Delegates` に集約し、本文で散文的に繰り返さない。`## 2. Boundaries` には `Non-goals`（意図的にやらないこと＋再検討条件）と誤解されやすい境界のみを書き、`allowed` で強制済みの逆方向依存禁止は書かない。
3. **原則優先・識別子の記述禁止（BASE-12）**:
   - メソッドシグネチャ、クラス名、インターフェース名、属性名、設定プロパティ名、エラー定数名は原則書かず、能力と意味論の原則で記述する（リファクタリング時にツールで検知できない陳腐化を防ぐため）。
4. **コード実装済みの事実のみを書く**:
   - モデルや機能の制約を書く前に現行コードを確認し、未実装の構想（例: 存在しないフィールドやライフサイクル項目）を既定事実として書かない。
5. **ベースライン DNA を反復しない**:
   - [industrial-dna.md](/src/BuildingBlocks/.prompts/industrial-dna.md) で定義済みの全 BuildingBlocks 共通原則（`TimeProvider`, `TryAdd`, Provided 層での置換, `[VKBlockDiagnostics]` 等）は再掲しない。`4. Defaults & Overrides` は固有の機構（例: keyed DI）のみ書くか `None` とし、`6. Registration & Diagnostics` は固有契約がない限り省略する。
