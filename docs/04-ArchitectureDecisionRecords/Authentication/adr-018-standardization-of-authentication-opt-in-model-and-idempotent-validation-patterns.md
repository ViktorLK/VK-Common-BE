# ADR 018: Standardization of Authentication Opt-in Model and Idempotent Validation Patterns

- **Date**: 2026-04-11
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authentication

## 2. Context (背景)

VK.Blocks.Authentication モジュールは、これまで戦略（JWT, API Key, OAuth）ごとにデフォルトの有効状態が統一されておらず、一部の機能が「デフォルトで有効」になっていました。また、設定値のバリデーションロジックが許容的であったため、未構成の環境で予期しない起動エラーが発生したり、逆に必須設定が漏れた状態で実行されるリスクがありました。

さらに、`IValidateOptions<T>` を用いたバリデータの登録において、標準の `TryAddSingleton` を使用すると、ライブラリ内部で登録済みのバリデータ（DataAnnotations 等）と競合し、カスタムバリデータが静かに無視される（Idempotency Issues）という技術的課題が露呈しました。

## 3. Problem Statement (問題定義)

1.  **非一貫な有効状態**: 機能によって `Enabled` のデフォルト値が異なり、開発者が意図しない副作用を招く可能性がありました。
2.  **不完全な Fail-Fast**: 機能が有効であっても設定不備を許容してしまい、実行時の `InvalidOperationException` までエラーが遅延するケースがありました。
3.  **DI 登録の競合**: `AddVKBlockOptions` 内部でバリデータが自動登録されるため、開発者が追加するバリデータを `TryAddSingleton` で登録すると、後続のバリデータがスキップされていました。

## 4. Decision (決定事項)

堅牢性と予測可能性を最大化するため、以下の設計変更を決定しました。

### 4.1 明示的オプトイン (Explicit Opt-in) モデルの採用
ルートの `VKAuthenticationOptions` およびすべてのサブ機能（JWT, API Key, OAuth）の `Enabled` プロパティのデフォルト値を `false` に変更します。これにより、ライブラリを参照しただけでは副作用が発生せず、利用者が意図的に有効化するまで静かな状態を保ちます。

### 4.2 厳格な Fail-Fast バリデーションの強制
機能が有効化された(`Enabled = true`)場合は、`IValidateOptions<T>` を通じて必須プロパティ（Issuer, SecretKey 等）を厳格にチェックします。不備がある場合はアプリケーションの起動プロセスを停止させます。

### 4.3 べき等なバリデータ登録パターンの標準化
複数のバリデータを安全に追加できるよう、`TryAddEnumerable` パターンを標準化しました。また、可読性向上のために `IServiceCollection` に対する `TryAddEnumerableSingleton<TService, TImplementation>` 拡張メソッドを導入しました。

```csharp
// 登録の標準パターン
var options = services.AddVKBlockOptions<MyOptions>(section);

// 独自のバリデータは必ず TryAddEnumerable 経由で登録する
services.TryAddEnumerableSingleton<IValidateOptions<MyOptions>, MyCustomValidator>();
```

## 5. Alternatives Considered (代替案の検討)

### Option 1: デフォルトですべてを有効化 (Permissive by Default)
- **Approach**: 構成なしでも動作するようにデフォルト値を埋める。
- **Rejected Reason**: セキュリティ上のリスク（弱いデフォルト鍵の使用など）や、無駄なリソース消費を招くため却下。

### Option 2: 起動時ではなく実行時にエラーを出す (Lazy Validation)
- **Approach**: 認証ハンドラーの初期化時に設定をチェックする。
- **Rejected Reason**: 設定ミスが本番環境の実行時まで発覚しないリスクがあり、Fail-Fast 原則に反するため却下。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- **予測可能性の向上**: 起動エラーが発生した場合、必ず「意図的に有効化した機能の設定不備」であることが明確になります。
- **DI 構成の堅牢性**: バリデータの重複登録やスキップの問題が解消されました。
- **ドキュメントの自己完結性**: ソースコード内の XML コメントと ADR により、登録ルールが明確化されました。

### Negative
- **導入時のステップ増加**: 開発者は `appsettings.json` で明示的に `Enabled: true` を設定する手間が増えます。

### Mitigation
- **README とエラーメッセージの充実**: 「Enabled スイッチが必要であること」を README の冒頭および例外メッセージで明示し、開発者のオンボーディングを支援します。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Idempotent Register**: `AddVKBlockOptions` は `services.Any()` を用いて二重登録を防止しており、拡張モジュール（OIDC 等）から安全に再呼び出し可能です。
- **Fail-Fast Compliance**: 起動時のバリデーションにより、不完全な暗号化鍵やアルゴリズムの使用を未然に防ぎます。

**Last Updated**: 2026-04-11
