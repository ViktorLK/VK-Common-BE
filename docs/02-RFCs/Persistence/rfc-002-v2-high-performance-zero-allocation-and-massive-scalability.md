# RFC-002: V2 High-Performance Zero-Allocation & Massive Scalability

- **Status**: ✅ Approved (Implemented Evolution Wave 2)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-02-25
- **Target Module(s)**: `VK.Blocks.Persistence.EFCore`
- **Related ADRs**:
  - [EFCore ADR-002: Static Generic Caching for Zero-Overhead Metadata](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-002-static-generic-caching.md)
  - [EFCore ADR-003: Cursor-Based Pagination with Bidirectional Scrolling](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-003-cursor-pagination.md)
  - [EFCore ADR-004: Expression Compilation Caching for High-Performance Cursor Pagination](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-004-expression-caching.md)
  - [EFCore ADR-005: IAsyncEnumerable for Memory-Efficient Data Streaming](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-005-async-enumerable-streaming.md)
  - [EFCore ADR-008: MethodInfo Caching for Bulk Operations](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-008-methodinfo-caching.md)
  - [EFCore ADR-009: Cursor Serializer Abstraction (`ICursorSerializer`)](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-009-cursor-serializer-abstraction.md)
  - [EFCore ADR-012: EF Core Bulk Optimization and Adapter Pattern for .NET 10](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-012-efcore-bulk-update-refactoring.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第2波着手前の課題 (Wave 2 Pain Points)**:
  - **Deep Pagination の深刻なレイテンシ劣化 (`EFCore ADR-003`)**:
    従来の Offset / Skip-Take ページネーションでは、ページが深くなるにつれて DB が数万〜数十万件のレコードをスキャンして破棄する必要があり、10,000 ページ目での応答時間が 5,000ms を超過していた。
  - **リフレクションと式木コンパイルの CPU 浪費 (`EFCore ADR-002`, `ADR-004`, `ADR-008`)**:
    エンティティが `IAuditable` や `ISoftDelete` を実装しているかの判定、および動的カーソル式木のコンパイル（`Expression.Compile()`）がリクエストごとに走り、CLR ヒープの無駄な消費と GC 圧迫を引き起こしていた。
  - **大規模データ取得時の OOM（メモリ枯渇）リスク (`EFCore ADR-005`)**:
    数万〜数十万行の一括エクスポートやバッチ処理で、全レコードをメモリ上の `List<T>` にバッファリングしていたため、ギガバイト単位のメモリを一時占有していた。
  - **カーソルトークンの改ざん脆弱性 (`EFCore ADR-009`)**:
    カーソル文字列が単なる Base64 エンコードであった場合、クライアントによる改ざんや情報漏洩のリスクがあった。
  - **.NET 10 Bulk 操作の破壊的変更 (`EFCore ADR-012`)**:
    EF Core のバージョンアップに伴い、`ExecuteUpdateAsync` の内部式ツリーが `UpdateSettersBuilder` へシフトしたことによる拡張性のボトルネック。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - 全社プロダクション環境で数百万件規模のトランザクションデータを扱うサービスが増加し、API 応答の $p99$ レイテンシの極限短縮とメモリ使用量の劇的削減が必須となったため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **マイクロ秒からナノ秒への極限キャッシュ (`EFCore ADR-002`, `ADR-008`)**:
    C# の静的ジェネリック型（`EfCoreTypeCache<TEntity>`）を活用し、型メタデータ判定を 62 倍（50ns → <1ns）、`MethodInfo` 取得を 100 倍（50μs → <1μs）高速化する。
  - **定数時間カーソルページネーション (`EFCore ADR-003`, `ADR-004`)**:
    シーク法（Seek Method）と式木比較キャッシュ（`ExpressionEqualityComparer`）により、10,000 ページ目でも 4ms で即時応答（1,250 倍向上）。
  - **ストリーミングによる 95% メモリ削減 (`EFCore ADR-005`)**:
    `IAsyncEnumerable<T>` を採用し、DB から 1 行ずつ読み出して即時消費（パイプライン化）することで、数百万件でも 50MB 以下のメモリフットプリントを維持。
  - **改ざん防止カーソルセキュリティ (`EFCore ADR-009`)**:
    HMAC-SHA256 署名を伴う `SecureCursorSerializer` を標準提供。
  - **.NET 10 Bulk 最適化 (`EFCore ADR-012`)**:
    `EfCorePropertySetterAdapter` により、Clean Architecture を損なわずに EF Core ネイティブの更新ビルダーへ委譲。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **分散 Outbox やマルチプロバイダ分離**: 第 3 波の工業化パイプラインで解決する。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **高速磁気浮上鉄道と極限計量ライン (The Maglev Express Line & Zero-Friction Telemetry)**
>
> 従来のデータアクセスは、各駅停車で乗客全員の身元確認をいちから書類審査（リフレクション）し、途中の駅（Offset）をすべて通過しないと終点に辿り着けない低速普通列車でした。
> 第 2 波の改革により、線路を「高速磁気浮上線（Seek カーソル）」に切り替え、途中の駅を一切踏み越えずに目的の座標へ 4ms で直行できるようにしました。
> また、車両の重量検査（型メタデータ判定）は、静的ジェネリックという「永久磁石センサー」によって摩擦ゼロ（<1ns）で瞬時に判定。乗客の移動も、駅舎に全員を集めてから出発するのではなく、動く歩道（`IAsyncEnumerable` ストリーミング）によって滞留メモリを 95% 削減しました。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **Magnetic Sensor (摩擦ゼロ磁気センサー)** | `EfCoreTypeCache<T>`, `EfCoreMethodInfoCache` | `Internal/` | CLR 静的ジェネリックを活用した $0$ ns メタデータ即時参照 |
| **Maglev Direct Track (リニア直行便)** | `CursorPagedResult<T>`, `IVKEntityReadRepository` | Public Protocol | インデックスシークによる定数時間（$O(1)$）ページネーション |
| **Formula Memory (走行ルート記録装置)** | `EfCoreExpressionCache`, `ExpressionEqualityComparer` | `Internal/` | 式木コンパイル結果の再利用による CPU スパイク抑止 |
| **Moving Walkway (動く歩道)** | `IAsyncEnumerable<T>` (Streaming) | Public Protocol | 全件バッファリングを排した定常メモリフットプリントでの処理 |
| **Sealed Cargo Ticket (封印付きチケット)** | `SecureCursorSerializer` | `Internal/` | HMAC-SHA256 署名によるカーソルの耐改ざん性保証 |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 静的ジェネリックメタデータキャッシュ (`EfCoreTypeCache<TEntity>`)

```csharp
// [EFCore ADR-002] CLR 型初期化セマンティクスによるゼロオーバーヘッド
internal static class EfCoreTypeCache<TEntity>
{
    public static readonly bool IsAuditable = typeof(IVKAuditable).IsAssignableFrom(typeof(TEntity));
    public static readonly bool IsSoftDelete = typeof(IVKSoftDelete).IsAssignableFrom(typeof(TEntity));
    public static readonly string EntityName = typeof(TEntity).Name;
}
```

### 4.2 双方向カーソルページネーション契約

```csharp
// [EFCore ADR-003] [EFCore ADR-009] カーソルシリアライザー抽象
public interface ICursorSerializer
{
    string Serialize(object[] keyValues);
    object[] Deserialize(string cursor);
}

// レスポンス契約 (FS-08 準拠)
public sealed record VKCursorPagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public string? NextCursor { get; init; }
    public string? PreviousCursor { get; init; }
    public bool HasNextPage { get; init; }
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **メモリ消費 vs 式木キャッシュ**:
  - `ExpressionEqualityComparer` によるキャッシュキー保持に伴うメモリフットプリント。
  - *Mitigation*: 式木キャッシュは LRU または弱参照により一定件数で上限（Bouded Cache）を管理。
- **カーソルソートのインデックス要求**:
  - カーソルシークではソート列（複合キー）に対する物理インデックスが必須。
  - *Mitigation*: 開発者ガイドラインにて、カーソル対象カラムへの複合インデックス作成を義務付け（`CS.08`）。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

- [x] **Phase 1 (Micro-Optimizations)**: `EfCoreTypeCache` および `EfCoreMethodInfoCache` の全社展開。
- [x] **Phase 2 (Cursor Pagination Engine)**: 双方向シークロジック、式木コンパイルキャッシュ、および `SecureCursorSerializer` の実装。
- [x] **Phase 3 (Bulk & Streaming Modernization)**: `IAsyncEnumerable` ストリーミング API の公開と .NET 10 `EfCorePropertySetterAdapter` の導入。

