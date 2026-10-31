# ADR 012: Authorization Library Architecture Normalization

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authorization

## 1. Context (背景)

Authorization ライブラリは、先行して実施されたリファクタリング（ADR-011）により基本的な名前空間の整理が行われていたが、その後に策定された「Building Block Blueprint (BB.01–BB.05)」に対しては、内部構造の階層、マーカーパターンの実装、およびソースジェネレータとの統合において依然として不整合が残っていた。特に、診断用メタデータの提供方法や、内部実装の隠蔽深度において、他の Building Block との統一性が欠如していた。

## 2. Problem Statement (問題定義)

現在の実装には以下の課題があった：
1. **Blueprint 違反**: フォルダ構造が BB.01 に定める「垂直スライス」および `Internal/` フォルダによるカプセル化ルールに完全に適合していなかった。
2. **マーカーパターンの未整備**: `IVKBlockMarker` が正しく実装されておらず、グローバルなテレメトリ収集において手動の文字列定義に依存していた。
3. **カプセル化の不足**: 2段階目以降の深度にあるべき内部クラスがルート近傍に混在し、公開 API サーフェスが不明確であった。

## 3. Decision (決定事項)

Authorization ライブラリを「Building Block Blueprint」に完全準拠させるべく、以下の再編を実施した：

1. **名前空間とフォルダ構造の再編 (AP.03/16)**:
   - 公開 API（Options, Builder, Marker）のみを `VK.Blocks.Authorization` ルートに配置。
   - すべての実装ロジック、ハンドラー、バリデーターを各機能フォルダ配下の `Internal/` サブディレクトリへ移動し、名前空間もそれに同期させた。
2. **IVKBlockMarker の実装 (BB.02)**:
   - `VKAuthorizationBlock` マーカークラスをルートに導入。
   - Source Generator により、`BlockIdentifier` ("VK.Blocks.Authorization") と `BlockName` ("Authorization") を自動生成。
3. **命名規則の完全適用**:
   - 公開されるすべての型に `VK` プレフィックスを強制し、内部型からは一貫して除去した。

### 実装後のディレクトリ構成例：
```
Authorization/
├── VKAuthorizationBlock.cs (Public Marker)
├── DependencyInjection/
│   ├── VKAuthorizationExtensions.cs (Public Wrapper)
│   ├── VKAuthorizationOptions.cs (Public Options)
│   └── Internal/
│       ├── AuthorizationBlockRegistration.cs (Principal Logic)
│       └── ...
└── Roles/
    ├── Internal/
    │   ├── RoleHandler.cs (Internal)
    │   └── RolesRegistration.cs (Internal)
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 現状維持
- **Approach**: ADR-011 の状態を維持する。
- **Rejected Reason**: 他の Building Block（Core, Authentication）と構造が異なるため、開発者が各ブロックごとに個別の構造を理解する必要があり、認知負荷が高い。また、Source Generator による自動化の恩恵を十分に受けられない。

### Option 2: すべてを internal にする
- **Approach**: インターフェースのみを public にし、すべてのクラスを internal に隔離する。
- **Rejected Reason**: 認可ハンドラーは ASP.NET Core の DI システムから解決可能である必要があるが、ライブラリ外部からの高度なカスタマイズ（継承等）を将来的に許容する場合、極端な制限は拡張性を損なう恐れがある。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **利用体験の向上**: ルート名前空間一つでほとんどの機能にアクセス可能になり、開発効率が向上した。
- **堅牢な DI**: 標準的な認可パイプラインへの確実な統合により、ランタイムエラーのリスクが低減した。
- **一貫性**: 他の Building Block と全く同じルールでコードを読み書きできるようになった。

### Negative
- **大規模なファイル移動**: 多くのファイルの物理パスと名前空間が変更されたため、既存の参照プロジェクトで修正が必要になる。

### Mitigation
- ソリューション全体のビルドおよびテストをパスさせることで、移行の正確性を担保した。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **セキュリティ**: この再編により、セキュリティに関連する内部ハンドラー（`TenantAuthorizationHandler` 等）が `Internal/` 空間に適切に隔離され、外部から直接インスタンス化されたり、不正にバイパスされたりするリスクを低減した。
- **診断**: `AuthorizationMetadataProvider` が Source Generated な `BlockIdentifier` を使用するように更新され、すべてのログとトレースにおいて正確なモジュール識別が可能となった。

**Last Updated**: 2026-04-24
**Status**: ✅ Accepted


