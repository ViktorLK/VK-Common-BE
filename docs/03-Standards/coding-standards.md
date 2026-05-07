# Coding Standards (開発標準)

本プロジェクトで採用しているコーディング規約とアーキテクチャ标准（VK.Blocks Industrial DNA）のインデックスです。

## 📜 核心標準 (Detailed Standards)

各領域の深い技術標準については、以下の個別ドキュメントを参照してください。

1.  **[Standard 01: Result Pattern & Error Handling](./01-result-pattern.md)**
    - 統一されたエラー処理、Result<T>、エラーコード階層の定義。
2.  **[Standard 02: Observability & Resiliency](./02-observability-resiliency.md)**
    - LoggerMessage、OpenTelemetry、Polly レジリエンスポリシーの適用。
3.  **[Standard 03: Dependency Injection & Configuration](./03-dependency-injection.md)**
    - 冪等な登録シーケンス、Marker Pattern、Options ライフサイクル。
4.  **[Standard 04: Structural Organization & Naming](./04-structural-organization.md)**
    - 垂直スライス、可见性境界（Level 1/2）、VK プレフィックス、Args Pattern。
5.  **[Standard 05: Development Lifecycle](./05-development-lifecycle.md)**
    - テストプロトコル、ADR (アーキテクチャ意思決定)、RFC (提案要求)、Backlog 管理。

---

## 🛠️ 基本スタイル (Base Style)

- **C# Version**: C# 12.0/13.0 最新機能を積極的に採用する。
- **Namespace**: File-scoped namespace を使用する。
- **Nullable**: 全プロジェクトで `Enabled`。
- **Async**: `ConfigureAwait(false)` の徹底（ライブラリのみ）、`ValueTask` の活用。
- **Sealed**: すべてのクラスはデフォルトで `sealed`。
- **Guard**: 境界チェックには `VKGuard` を必須とする。
