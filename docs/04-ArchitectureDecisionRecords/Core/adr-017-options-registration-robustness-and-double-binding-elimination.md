# ADR 017: Options Registration Robustness and Double Binding Elimination

## 1. Meta Data

- **ADR Number & Title**: ADR 017: Options Registration Robustness and Double Binding Elimination
- **Date**: 2026-05-19
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: Core DependencyInjection / Config Binding

---

## 2. Context (背景)

VK.Blocks における不変性（`sealed record` + `init`）および関数型流式変換（ADR-016 で導入された `Func<TOptions, TOptions>` 和 `with` 式）を支える底座（Infrastructure）である `AddVKBlockOptions<TOptions>` において、以下の 2 つの深刻な設計上の問題が発見された。

1. **二重バインディング（Double Binding）と不要な DI 記述子の蓄積**
   `AddVKBlockOptions` 内のステップ 3 において、`.Bind(targetConfig)` を重ねて呼び出していた。しかし、VK.Blocks はカスタム `IOptionsFactory<T>` である `VKBlockOptionsFactory` を登録してすでに完全バインド・変換済みの Singleton インスタンスを直接返却しているため、この `.Bind` は完全に重複しており、DI コンテナに不要な `IConfigureOptions` を登録するオーバーヘッドとなっていた。また、デフォルトの `OptionsFactory` が再導入された場合に意図しない「二重バインディング」を引き起こす潜在リスクがあった。

2. **`TryAddSingleton` の DI 登録順序依存の脆弱性**
   カスタムファクトリの登録に `TryAddSingleton<IOptionsFactory<TOptions>>` を使用していた。しかし、`TryAdd` の特性上、もし外部の拡張メソッドや他の箇所で先に `AddOptions<TOptions>()` が呼び出されていた場合、.NET 規定の `OptionsFactory<TOptions>` が既に登録されているため、我々のカスタムファクトリが**静黙的に無視（サイレントスキップ）**されてしまう脆弱な構造になっていた。

---

## 3. Problem Statement (問題定義)

- **バインドの冗長性と DI 汚染**: `Bind(targetConfig)` により、実行されない `ConfigureNamedOptions` が登録され、メモリおよび DI 解決に不要なオーバーヘッドが発生する。また、コードを読む开发者に対して二重バインディングの懸念を与える（コードの可読性・明瞭性の欠如）。
- **起動時 Fail-Fast の脆弱性**: `TryAdd` の順序依存性により、カスタムファクトリの登録がスキップされた場合、事前バインド済みのインスタンス（不変レコード）が IOptions 経由で取得できなくなり、最悪の場合、バリデーション（`ValidateOnStart`）や不変変換（`transform`）が適用されないまま規定の `OptionsFactory` によって空のインスタンスが生成されてしまう。

---

## 4. Decision (決定事項)

1. **`.Bind()` の徹底排除**
   ステップ 3 の `services.AddOptions<TOptions>()` 呼び出しから冗長な `.Bind(targetConfig)` を削除する。DataAnnotations などのバリデーションルール（`IValidateOptions<T>`）は、カスタムファクトリが返却する事前バインド済みのインスタンスに対して実行されるため、`.Bind()` を削除しても Fail-Fast 性能は 100% 維持される。
2. **`services.Replace` による DI 登録順序依存の完全解消**
   カスタムファクトリの登録を `TryAddSingleton` から `services.Replace(ServiceDescriptor.Singleton<IOptionsFactory<TOptions>>(...))` に変更する。これにより、DI 登録の時序（Timing）に関わらず、必ず我々のカスタムファクトリが優先して登録・オーバーライドされることを保証する（`Replace` は登録がない場合は新規追加、ある場合は上書きするため常に安全）。
3. **並行処理と非アトミックな二重 Replace に対する警告コメントの追加**
   幂等性チェック（再バインド・再 Transform）における 2 つの `services.Replace` 操作がスレッドセーフでないため、スレッドセーフティの警告コメントを追加し、すべての Options 流式変換はスタートアップ（Configure フェーズ）の単一スレッド上で同期的に実行されなければならないことを明文化する。

---

## 5. Alternatives Considered (代替案の検討)

### Option 1: 規定の `OptionsFactory` をそのまま使い、`IConfigureOptions` ですべての変換を行う
- **Approach**: レコードの不変性をあきらめ、可変（mutable）なクラスに戻すことで、規定のバインドフローに乗せる。
- **Rejected Reason**: 不変（immutable）設計である `sealed record` と `with` 式（ADR-016）の決定を根底から覆すことになり、副作用の排除やスレッド安全性の観点から却下。

### Option 2: `TryAdd` の順序を厳格にドキュメント化して開発者に遵守させる
- **Approach**: 「`AddVKBlockOptions` は必ず他のどの Options 操作よりも先に呼ばなければならない」というルールを課す。
- **Rejected Reason**: 開発者のヒューマンエラーを交通ルールで解決しようとするアプローチであり、フレームワークの頑健性（Robustness）として極めて脆弱であるため却下。

---

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- 二重バインディングの概念的混乱を完全に解消し、DI 登録オーバーヘッドを削減。
- 順序依存のない、堅牢で決定論的な DI 構成を確立。
- `ValidateOnStart` とカスタムファクトリの協調が完全に保証される。

### Negative
- 幂等性の上書き時における 2 つ of `Replace` 操作が非アトミックである点。

### Mitigation
- コードベースに `NOTE (CONCURRENCY & THREAD-SAFETY WARNING)` を明記し、Configure フェーズ以外でのマルチスレッドによる動的再構成を禁止する。

---

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **DI 設計の堅牢性**: `Replace` の使用により、他サービスによる意図しない DI キャプチャや順序の狂いを完全に防ぐ。
- **安全性**: 起動時の Fail-Fast（`ValidateOnStart`）が順序依存なく確実に実行されるため、不完全な構成（空の接続文字列など）でアプリケーションが起動し、実行時にデータ漏洩や予期せぬクラッシュを引き起こすセキュリティリスクをゼロに抑え込む。
