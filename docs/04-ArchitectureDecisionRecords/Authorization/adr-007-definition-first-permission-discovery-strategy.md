# ADR 007: Definition-First Permission Discovery Strategy

**Date**: 2026-04-08  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: VK.Blocks.Authorization - Permission Governance

## 1. Context (背景)

これまでの Authorization モジュールでは、コントローラーやアクションに直接文字列で権限を記述する「受動的発見（Passive Discovery）」モデルを採用していました。しかし、システムが大規模化し、複数のモジュールが統合されるにつれ、以下の課題が顕在化しました：
- **メタデータの欠落**：権限の「表示名（DisplayName）」や「詳細説明（Description）」をコードから同期できない。
- **ガバナンスの欠如**：どの権限がどのモジュールに属しているかが不明確で、ドキュメントとの乖離が発生しやすい。
- **開発効率の低下**：マジックストリング（Magic Strings）への依存により、タイポなどのランタイムエラーが発生しやすく、コンパイル時のチェックが効かない。

## 2. Problem Statement (問題定義)

權限管理における「唯一の事実ソース（Single Source of Truth）」が不在であるため、DB に存在する権限一覧とコードで要求される権限が乖離し、監査性や保守性に重大な影響を及ぼしています。

## 3. Decision (決定事項)

「定義優先（Definition-First）」の権限スキャン戦略を採用し、Source Generator (SG) によるメタデータの自動抽出と、強タイプ属性の自動生成を導入します。

1. **定義の統合**：
   権限は専用の定数クラスに集約し、`[GeneratePermissions]` 特性でマーキングします。
2. **メタデータの付与**：
   標準の `[Display(Name = "...", Description = "...")]` を使用して、ビジネス要件に沿ったメタデータをコード内に直接定義します。
3. **Source Generator の活用**：
   SG はセマンティックモデル（Semantic Model）を用いて定数の解決を行い、以下の成果物を自動生成します：
    - 各権限に対応する強タイプ属性（例：`[RequireIdentityUserReadAttribute]`）。
    - 全量メタデータを保持する `PermissionsCatalog.All` 読み取り専用リスト。
4. **命名規則の強制**：
   生成される属性名の競合を避けるため、後述の ADR-009 に基づく命名規則を適用します。

## 4. Alternatives Considered (代替案の検討)

### Option 1: Manual Reflection Processing (ランタイム・リフレクション)
- **Approach**: 起動時にリフレクションですべての Assembly をスキャンする。
- **Rejected Reason**: 起動パフォーマンスの低下。また、定数値に付随する `[Display]` 特性をランタイムで全量収集するのは非効率。

### Option 2: JSON/YAML Based Definition
- **Approach**: 外部の設定ファイルで権限を一括定義する。
- **Rejected Reason**: コードとの乖離が発生しやすく、インテリセンスの恩恵を受けられない。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive (メリット)
- **コンパイル時の安全性**：マジックストリングの排除により、権限名のタイポがコンパイルエラーとして検出される。
- **自動同期の基盤**：DB への権限マスタ同期が、SG が生成した `PermissionsCatalog.All` を通じて 100% 正確に実行可能。
- **ドキュメントの自動化**：コードがそのまま権限仕様書となる。

### Negative (デメリット)
- **ビルド時間の微増**：Source Generator の実行によるオーバーヘッド。
- **学習コスト**：新しい「定義 -> 生成」のフローを開発者に定着させる必要がある。

---
**Last Updated**: 2026-04-08  
