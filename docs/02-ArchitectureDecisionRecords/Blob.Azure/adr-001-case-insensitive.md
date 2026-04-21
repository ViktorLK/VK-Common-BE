# ADR 001: Implementation of Case-Insensitive Naming Conflict Prevention

- **Date**: 2026-03-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Blob.Azure Implementation Optimization

## 2. Context (背景)

Azure Blob Storage は、ファイル名（Blob Name）の大小文字を区別（Case-Sensitive）する仕様です。例えば、`Report.pdf` と `report.pdf` は同一コンテナ内に共存可能です。しかし、エンタープライズアプリケーションの要件や Windows などのファイルシステムに慣れたユーザーにとっては、これが予期しない同名ファイルの混在や、検索時の混乱を招く原因となります。

## 3. Problem Statement (問題定義)

Azure のデフォルト動作に依存すると、以下の問題が発生します：
1.  **ユーザーの混乱**: 大小文字が異なるだけの同名ファイルが存在することによる視認性の低下。
2.  **不整合**: アプリケーションの一部（DB 等）が大小文字を区別しない場合、ストレージとの間でデータ不整合が起きる可能性がある。
3.  **上書きリスクの誤認**: 開発者が「同名チェック」を実装する際、大小文字の区別を失念し、意図せずマルチエントリを許容してしまうリスク。

## 4. Decision (決定事項)

`VK.Blocks.Blob.Azure` における書き込み操作（Upload, CreateDirectory）の前に、**大小文字を無視した（Case-Insensitive）既存チェック**を明示的に行うことを決定しました。

1.  **実装**: 操作対象のパスに対し、コンテナ内の既存 Blob を `StringComparison.OrdinalIgnoreCase` でスキャンします。
2.  **エラー処理**: 衝突が検出された場合、独自の `BlobErrors.NameConflict` を返却し、操作を中断します。

```csharp
// 実装イメージ
await foreach (var blob in containerClient.GetBlobsAsync(prefix: blobName, cancellationToken: cancellationToken))
{
    if (string.Equals(blob.Name, blobName, StringComparison.OrdinalIgnoreCase))
    {
        return Result.Failure(BlobErrors.NameConflict);
    }
}
```

## 5. Alternatives Considered (代替案の検討)

### Option 1: Azure のデフォルト動作 (Case-Sensitive) をそのまま利用
*   **Approach**: チェックを行わず、Azure の仕様に任せる。
*   **Rejected Reason**: ビジネス要件として「同名ファイルは 1 つ」というルールを徹底するため、デフォルトでは不十分と判断。

### Option 2: すべてのファイル名を小文字に強制変換 (ToLower)
*   **Approach**: 保存時にパスを強制的に小文字化する。
*   **Rejected Reason**: ユーザーが意図して付けた大小文字の表示（キャメルケース等）が失われるため、体験を損損ねる。

## 6. Consequences & Mitigation (結果と緩和策)

*   **Positive**: ファイルシステムの一貫性が保たれ、ユーザーにとって直感的で安全な操作が保証される。
*   **Negative**: 書き込み前に `GetBlobsAsync` によるリストアップが必要なため、若干のレイテンシと API 呼び出しコストが発生する。
*   **Mitigation**: プレフィックス検索を用いることでスキャン範囲を最小化し、パフォーマンスへの影響を抑える。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

このチェックはライブラリ内部で強制されるため、呼び出し側が意識することなくエンタープライズ基準の整合性が保たれます。
