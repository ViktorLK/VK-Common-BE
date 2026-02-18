# ADR 009: Cursor Serializer Abstraction (`ICursorSerializer`)

**Date**: 2026-02-18
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: [EFCore Persistence Layer — Cursor Serialization Strategy]
**Supersedes**: ADR-003 Future Considerations §1 (Cursor Serialization Abstraction)

---

## Context (背景)

### ADR-003 との関係

[ADR-003](./adr-003-cursor-pagination.md) はカーソルページネーションの**アルゴリズム**（WHERE 句の動的生成・双方向スクロール）を決定した。その Future Considerations §1 において、以下の問題が予告されていた：

> 「将来的に `ICursorSerializer` を導入し、バージョン情報を埋め込む」

本 ADR はその予告を**正式な設計決定として記録**するものである。

---

### Problem Statement (問題定義)

ADR-003 採用時点の実装では、カーソルのシリアライズ・デシリアライズが `EfCoreReadRepository` 内に**静的メソッドとして直接埋め込まれていた**。

```csharp
// 旧実装（ADR-003 時点）
private static string EncodeCursor<TCursor>(TCursor cursor)
{
    var json = JsonSerializer.Serialize(cursor);
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
}
```

この設計には以下の問題があった：

#### 1. テスタビリティの欠如

シリアライズロジックが Repository に直接埋め込まれているため、カーソルのエンコード/デコード動作を**単体テストで独立して検証できない**。

#### 2. セキュリティリスク

Base64 + JSON のみのエンコードは**改ざん防止機能を持たない**。悪意あるクライアントがカーソルを手動で書き換え、任意の WHERE 条件を注入できる可能性がある。

#### 3. 差し替え不可能性 (OCP 違反)

シリアライズ戦略を変更（例: HMAC 署名の追加・バージョン管理の追加）するには、Repository クラス自体を修正する必要があり、**Open/Closed Principle に違反**する。

#### 4. スキーマ変更への脆弱性

カーソルトークンにバージョン情報が含まれないため、エンティティのプロパティ型が変更された場合（例: `decimal` → `double`）、既存のカーソルが**サイレントにデシリアライズ失敗**する。

---

## Decision (決定事項)

**`ICursorSerializer` インターフェースを導入し、Strategy パターンで実装を差し替え可能にする。**

### インターフェース設計

```csharp
// VK.Blocks.Persistence.Abstractions.Pagination
public interface ICursorSerializer
{
    /// <summary>カーソル値をトークン文字列にシリアライズする。</summary>
    string Serialize<T>(T value);

    /// <summary>トークン文字列をカーソル値にデシリアライズする。無効なトークンは default を返す。</summary>
    T? Deserialize<T>(string? token);
}
```

### 実装戦略の二段階構成

| 実装クラス               | 用途             | セキュリティ                             | 依存                                |
| ------------------------ | ---------------- | ---------------------------------------- | ----------------------------------- |
| `SimpleCursorSerializer` | 開発・テスト環境 | なし（Base64 のみ）                      | なし                                |
| `SecureCursorSerializer` | 本番環境         | HMAC-SHA256 署名 + バージョン + 有効期限 | `IOptions<CursorSerializerOptions>` |

### DI 登録戦略

```csharp
// デフォルト: SimpleCursorSerializer（TryAdd で上書き可能）
services.TryAddSingleton<ICursorSerializer, SimpleCursorSerializer>();

// 本番環境: AddSecureCursorSerializer() を呼び出して上書き
services.AddSecureCursorSerializer(opts =>
{
    opts.SigningKey = configuration["CursorSerializer:SigningKey"];
    opts.DefaultExpiry = TimeSpan.FromHours(1);
});
```

### Repository への注入

```csharp
// EfCoreReadRepository はインターフェース経由でのみ依存
public EfCoreReadRepository(
    DbContext context,
    ILogger logger,
    ICursorSerializer cursorSerializer)  // ← 具象型に依存しない
```

---

## Alternatives Considered (検討した代替案)

### ❌ Option 1: 静的メソッドのまま維持

**Approach**: `EncodeCursor` / `DecodeCursor` を Repository の private static メソッドとして継続。

**Rejected Reason**:

- テスト不可能（モック不可）
- セキュリティ強化のたびに Repository を修正する必要がある（OCP 違反）
- 本番環境での HMAC 署名追加が困難

---

### ❌ Option 2: ジェネリック型パラメータで戦略を指定

**Approach**: Repository 自体をジェネリック型パラメータで差し替え可能にする。

```csharp
public class EfCoreReadRepository<TEntity, TCursorSerializer>
    where TCursorSerializer : ICursorSerializer, new()
```

**Rejected Reason**:

- DI コンテナとの統合が複雑になる
- `new()` 制約により、コンストラクタ引数を持つ `SecureCursorSerializer` が使用できない
- 型パラメータが増加し、使用側の記述が煩雑になる

---

### ❌ Option 3: `Func<T, string>` デリゲートを Repository メソッドに渡す

**Approach**: シリアライズ関数を `GetCursorPagedAsync` の引数として渡す。

```csharp
public Task<CursorPagedResult<TEntity>> GetCursorPagedAsync<TCursor>(
    ...,
    Func<TCursor, string>? cursorEncoder = null)
```

**Rejected Reason**:

- 呼び出し側が毎回シリアライズ戦略を意識する必要がある（関心の分離に反する）
- デシリアライズはリポジトリ外（Application Layer）で行う必要があり、責務が分散する
- DI による一元管理ができない

---

### ✅ Option 4: `ICursorSerializer` インターフェース + DI（採用案）

**Advantages**:

- ✅ **DIP 準拠**: Repository は抽象に依存し、具象実装を知らない
- ✅ **OCP 準拠**: 新しいシリアライズ戦略の追加が既存コードを変更せずに可能
- ✅ **テスタビリティ**: `ICursorSerializer` をモック化して Repository を単体テスト可能
- ✅ **段階的セキュリティ強化**: 開発環境は `Simple`、本番環境は `Secure` を DI で差し替え

---

## Consequences (結果)

### Positive (ポジティブな影響)

✅ **テスタビリティの向上**: `ICursorSerializer` をモック化することで、カーソルのエンコード/デコードを Repository テストから分離できる

✅ **セキュリティの段階的強化**: `SecureCursorSerializer` により HMAC-SHA256 署名・スキーマバージョン管理・トークン有効期限を一元管理

✅ **タイミング攻撃対策**: `CryptographicOperations.FixedTimeEquals` による定時間比較で、署名検証のタイミング差分からの情報漏洩を防止

✅ **スキーマ進化への対応**: バージョンフィールド (`v`) により、将来のスキーマ変更時に古いトークンを明示的に拒否できる

✅ **設定の集中管理**: `CursorSerializerOptions` により、署名キーと有効期限を `appsettings.json` + Azure Key Vault で一元管理

### Negative (ネガティブな影響)

⚠️ **本番誤用リスク**: `SimpleCursorSerializer` がデフォルト登録されるため、開発者が `AddSecureCursorSerializer()` を呼び忘れると本番環境で無署名トークンが使用される

⚠️ **署名キーのローテーション非対応**: 現時点の `SecureCursorSerializer` は単一の署名キーのみをサポート。キーローテーション時に既存のカーソルが一斉無効化される

⚠️ **シングルトンライフタイムの制約**: `ICursorSerializer` は `Singleton` で登録されるため、リクエストスコープの情報（例: テナント別の署名キー）を使用できない

### Mitigation (緩和策)

| リスク             | 緩和策                                                                                  | 優先度 |
| ------------------ | --------------------------------------------------------------------------------------- | ------ |
| 本番誤用           | 起動時に `SimpleCursorSerializer` + Production 環境の組み合わせを検出して警告ログを出力 | 高     |
| キーローテーション | 将来的に `SecureCursorSerializer` に複数キーサポート（`PreviousSigningKey`）を追加      | 中     |
| シングルトン制約   | マルチテナント要件が発生した場合は `IOptionsSnapshot<T>` + `Scoped` ライフタイムへ移行  | 低     |

---

## Implementation Details (実装詳細)

### `SecureCursorSerializer` のトークン構造

```
Base64(json_payload) + "." + Base64(hmac_signature)
```

**JSON ペイロード**:

```json
{
  "v": 1,           // スキーマバージョン（将来の互換性管理）
  "d": <cursor>,    // カーソルデータ（型安全な JSON シリアライズ）
  "exp": 1708300800 // Unix タイムスタンプ（null = 無期限）
}
```

### デシリアライズの拒否条件

```
1. トークンが null または空文字列 → default を返す
2. "." セパレータが存在しない → default を返す
3. HMAC 署名が一致しない → default を返す（タイミング攻撃対策）
4. スキーマバージョンが不一致 → default を返す
5. 有効期限切れ → default を返す
6. FormatException / JsonException → default を返す（例外は伝播させない）
```

> **設計方針**: デシリアライズ失敗は「カーソルなし（初回リクエスト）」として扱い、例外を上位層に伝播させない。これにより、改ざんされたトークンや期限切れトークンは自動的に最初のページから再取得される。

### `CursorSerializerOptions` の設定例

```json
// appsettings.Production.json
{
    "CursorSerializer": {
        "SigningKey": "@Microsoft.KeyVault(SecretUri=https://...)",
        "DefaultExpiry": "01:00:00"
    }
}
```

---

## Security Considerations (セキュリティ考察)

### なぜ HMAC-SHA256 か

カーソルはサーバーが生成し、クライアントが次のリクエストで返送する。署名なしの場合、クライアントが以下の攻撃を実行できる：

1. **カーソル改ざん**: Base64 デコード → JSON 編集 → 再エンコードで任意の WHERE 条件を注入
2. **情報漏洩**: カーソルにエンティティの内部フィールド値が含まれる場合、それを読み取れる

HMAC-SHA256 署名により、サーバーが生成したトークンのみが有効であることを保証する。

### なぜ `FixedTimeEquals` か

通常の `==` 比較は最初の不一致バイトで処理を終了するため、比較時間から署名の一致度を推測できる（タイミング攻撃）。`CryptographicOperations.FixedTimeEquals` は常に全バイトを比較するため、この情報漏洩を防止する。

---

## Implementation References (実装参照)

### Core Components

| ファイル                                                                                                                      | 役割                                                     |
| ----------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------- |
| [`ICursorSerializer.cs`](/src/BuildingBlocks/Persistence/Abstractions/Pagination/ICursorSerializer.cs)                        | インターフェース定義                                     |
| [`SimpleCursorSerializer.cs`](/src/BuildingBlocks/Persistence/EFCore/Infrastructure/SimpleCursorSerializer.cs)                | 開発・テスト用実装                                       |
| [`SecureCursorSerializer.cs`](/src/BuildingBlocks/Persistence/EFCore/Infrastructure/SecureCursorSerializer.cs)                | 本番用実装（HMAC-SHA256）                                |
| [`CursorSerializerOptions.cs`](/src/BuildingBlocks/Persistence/EFCore/Options/CursorSerializerOptions.cs)                     | 設定オプション                                           |
| [`ServiceCollectionExtensions.cs`](/src/BuildingBlocks/Persistence/EFCore/DependencyInjection/ServiceCollectionExtensions.cs) | DI 登録 (`AddVKDbContext` / `AddSecureCursorSerializer`) |
| [`EfCoreReadRepository.cs`](/src/BuildingBlocks/Persistence/EFCore/Repositories/EfCoreReadRepository.cs)                      | `ICursorSerializer` の注入・使用                         |

---

## Related Documents (関連ドキュメント)

- 📄 [ADR-003: Cursor-Based Pagination](./adr-003-cursor-pagination.md) — 本 ADR の前提となるカーソルページネーション設計
- 📄 [ADR-002: Static Generic Caching](./adr-002-static-generic-caching.md) — `EfCoreExpressionCache` によるカーソルセレクタのコンパイルキャッシュ
- 📄 [Architecture Audit Report](/docs/04-AuditReports/EFCore/EFCore_Persistence_20260218.md) — `SimpleCursorSerializer` 本番誤用リスクの指摘（⚠️ セキュリティ注意）
