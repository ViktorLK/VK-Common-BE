# ADR 009: Standardization of Building Block Naming and Structural Conventions

## 1. Meta Data

- **Date**: 2026-04-21
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks 全体のモジュール構造と命名規則の統一

## 2. Context (背景)

VK.Blocks エコシステムにおいて、公開 API と内部実装の命名規則（`VK` プレフィックスの有無）および配置階層（第1層 vs 第2層以下）が混在しており、以下の問題が生じていた。

1. **名称衝突**: フレームワーク標準クラス（例: `AuthenticationBuilder`）と内部実装クラス名が衝突し、完全修飾名（FQNs）の使用を余儀なくされる。
2. **直感性の欠如**: インテリセンスにおいて、モジュール提供の型と外部ライブラリの型が混在し、発見性が低下する。
3. **カプセル化の不備**: `internal` な型が意図しないフォルダに配置され、公開 API との境界が曖昧になる。

これらを解消するため、全モジュールで一貫した「工業級（Industrial-grade）」の構造標準を確立する。

## 3. Problem Statement (問題定義)

現状の「悪い見本」：
```csharp
// src/BuildingBlocks/Authentication/Common/Extensions/ClaimsPrincipalExtensions.cs
// 1. 公開クラスなのに第3層にあり、VKプレフィックスがないため他と衝突しやすい
// 2. 名前空間が深すぎて利用者が using を複数書く必要がある
public static class ClaimsPrincipalExtensions { ... }

// src/BuildingBlocks/Authentication/DependencyInjection/Internal/VKAuthenticationBuilder.cs
// 3. 内部クラスなのに VK プレフィックスがあり、内部的なノイズになる
internal sealed class VKAuthenticationBuilder : IVKAuthenticationBuilder { ... }
```

## 4. Decision (決定事項)

全ての VK.Blocks モジュールに対し、以下の命名および構造規約を強制する。

### 4.1. 公開 API (Public API Surface)
- **配置**: モジュールルート直下の **第1層フォルダ**（例: `Abstractions/`, `Common/`, `Jwt/`）に配置する。
- **名前空間**: モジュールの **ルート名前空間**（例: `VK.Blocks.Authentication`）を使用する。
- **命名規則**: 原則として **`VK` プレフィックスを必須**とする（インターフェースは `IVK`）。
    - *例外*: `Attribute` サフィックスを持つ型（例: `JwtAuthorizeAttribute`）および `Block` サフィックスを持つ Marker（例: `AuthenticationBlock`）にはプレフィックスを付けず、記述性を優先する。

### 4.2. カプセル化された内部実装 (Encapsulated Internals)
- **配置**: 必ず **`Internal/` サブフォルダ**（第2層以下）に配置する。
- **名前空間**: 物理フォルダ階層に一致した名前空間（例: `VK.Blocks.Authentication.Jwt.Internal`）を強制する。
- **命名規則**: **`VK` プレフィックスを禁止**する。
    - 名前空間で内部実装であることが明示されているため、型名は簡潔にする。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 名前空間のみで区別する**
    - Approach: 全ての型からプレフィックスを排除し、名前空間だけで公開/非公開を分ける。
    - Rejected Reason: 同一モジュール内の `internal` クラスと外部ライブラリのクラスが名前衝突を起こすリスク（例: `AuthenticationBuilder`）が解消されない。
- **Option 2: 全ての型に VK を付ける**
    - Approach: 内部実装も含めすべてにプレフィックスを付与。
    - Rejected Reason: 内部コードにおける冗長性が増し、可読性が低下する。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - インテリセンスで `VK.` と入力するだけで、そのモジュールの主要型が直感的に発見できる。
    - 内部実装が `Internal` サブ名前空間に完全に分離され、API の透明性が向上する。
    - フレームワーク標準型との衝突が物理的に回避される。
- **Negative**:
    - 既存モジュールの破壊的変更（リファクタリング）が必要。
- **Mitigation**:
    - IDE のグローバル置換と `dotnet format` を活用し、移行コストを最小化する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **One File, One Type**: 第14規則に基づき、ファイル名と型名を一致させる。
- **Security**: 内部実装が `Internal` に隠蔽されることで、開発者が誤って内部 API を直接利用するリスクを低減し、セキュアな拡張ポイントのみを外部へ公開する。
