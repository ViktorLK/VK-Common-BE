# ADR 001: Incoming Guardrails and Preprocessing via AI Afferent Block

- **Date**: 2026-06-10
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Afferent

## 1. Context (背景)

LLM を呼び出す前段階（Before Pipeline）において、入力された信号（オーディオデータ、画像、テキストなど）に対する初期処理が必要となる。これには、音声データのテキスト変換（Transcription）、巨大テキストの分割（Text Splitting）、トークン制限やレートリミットの事前確認（Rate Limit）、および悪意ある入力や不適切なコンテンツの防御（Guardrails / Safety Checks）が含まれる。これらの「入力前処理・防衛」ロジックが、プロンプト組装エンジンである `AI.Psyche` の中に密結合して記述されると、各機能の単一責任原則が崩れ、システム設計が肥大化する。

## 2. Problem Statement (問題定义)

入力前処理とプロンプト構築が同一モジュールに混在することには、以下の課題がある：
1. **単一責任原則 (SRP) の違反**: `AI.Psyche` は提示文の集約・ソート・組装（Weaving）に専念すべきであり、音声の文字起こしや入力コンテンツの有害性検知といった「入力信号そのものの前処理」まで引き受けると、モジュールの関心が肥大化し保守が不可能になる。
2. **安全・控流のタイミング不足**: トークンレート制限やガードレール判定は、プロンプトを組み立てる**前**の生入力の段階で即時評価し、アウトであれば即座に上位にエラーを返して後続の重い組装処理をスキップすべきだが、境界が曖昧だとそれが難しい。
3. **入力前処理の再利用性の欠如**: 音声文字起こしやガードレール処理を別のパイプライン（例: 直接 LLM を叩く単純なチャットフロー等）で再利用したい際、Psyche の巨大なプロンプトコンテキスト構造に依存しなければならず、結合度が高すぎる。

## 3. Decision (決定事項)

入力信号の受容、前処理、および安全性検証の関心を独立して管理するため、**「AI.Afferent (求心性入力信号処理)」ビルディングブロックの新設**を決定する。

1. **`AI.Afferent` モジュールの創設**:
   - 生の入力（求心性信号）を受け取り、LLM 送信に適した形式にサニタイズ・変換・制限する処理のみを担当する。
2. **特性ベースのステージ設計**:
   - `IVKPsycheBeforePipelineStage` を実装した以下のステージを `AI.Afferent` 内に登録する：
     - `AfferentAudioPipelineStage`: 音声入力の文字起こし（`IVKAudioTranscriber`）。
     - `AfferentTextPipelineStage` & `AfferentDocumentPipelineStage`: テキスト分割（`IVKTextSplitter`）。
     - `AfferentGuardrailsPipelineStage`: 入力コンテンツの検知と有害性防御（`IVKGuardrail`）。
     - `AfferentRateLimitPipelineStage` & `AfferentTokenicsPipelineStage`: トークン控流。
3. **Psyche パイプラインへのステージフック登録**:
   - DI 登録時（`AIAfferentBlockRegistration.cs`）において、これらのステージを `IVKBeforePipelineStage<T>` （具現として `IVKPsycheBeforePipelineStage`）として DI コンテナに自動追加登録する。これにより、Psyche のコアコードを変更することなく、Afferent 側の入力サニタイズステージが自動的に実行チェーンの最前列に挿入される。

### `AI.Afferent` の構成と登録ロジック

```csharp
namespace VK.Blocks.AI.Afferent.Common.DependencyInjection.Internal;

internal static class AIAfferentBlockRegistration
{
    internal static IVKAIAfferentBuilder Register(IServiceCollection services, IConfiguration configuration, ...)
    {
        // 1~6. 重複登録チェック、Options検証、マーク等の共通DI処理 (BB.03)

        // 7. コアサービスおよびパイプラインステージの自動フック登録
        services.TryAddScoped<IVKAudioTranscriber, DefaultAudioTranscriber>();
        services.TryAddScoped<IVKTextSplitter, DefaultTextSplitter>();
        services.TryAddScoped<IVKGuardrail, DefaultGuardrail>();

        // PsycheのBEFOREステージとして自動注入
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, AfferentGuardrailsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, AfferentTextPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, AfferentAudioPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, AfferentRateLimitPipelineStage>());

        return builder;
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Include Preprocessing in AI.Psyche under "Preprocessors/" directory
- **Approach**: 新モジュールを作らず、`AI.Psyche` の中にフォルダを切って実装する。
- **Rejected Reason**: Psyche 自体のプロジェクトサイズがさらに肥大化し、音声依存（Speech SDK）などの重い外部ライブラリを Psyche 自体が参照せざるを得なくなり、単純なプロンプト構築を求めるシステムにとってライブラリサイズが重くなりすぎるため。

### Option 2: Run Preprocessors at Controller Layer
- **Approach**: Web アプリの API コントローラーやアプリケーションハンドラーで、手動で文字起こしやガードレールを呼び出ししてから Psyche に投げる。
- **Rejected Reason**: 開発者がガードレールの呼び出しを忘れるなどの人為的ミスが発生しやすく、セキュリティ水準の一律適用が不可能になるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **強固なセキュリティ境界**: 悪意ある入力や超巨大な音声データなどは、Psyche のプロンプト構築段階に入る前に `AfferentGuardrailsPipelineStage` で即座にエラーとして弾かれるため、サーバー全体の安全性が担保される。
- **優れた疎結合性**: 音声文字起こしなどの前処理は、Psyche の存在を知らなくても単体で DI から解決して実行できる。

### Negative
- **起動時のモジュール間依存関係**: `AI.Afferent` は `IVKPsycheBeforePipelineStage` を提供するため、必然的に `AI.Psyche` モジュールが事前にロードされている必要がある（Prerequisite 依存の発生）。

### Mitigation
- `VKAIAfferentBlock` の `Dependencies` マーカー定義において、明確に `typeof(VKAIPsycheBlock)` を指定し、起動時に依存関係がソースジェネレーターによって検証されるように保護する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Guardrail Integration**: `AfferentGuardrailsPipelineStage` は、`IVKGuardrail`（Azure Content Safety やローカルの有害表現フィルター）をループ実行し、入力テキストに問題がある場合は `GuardrailsErrors.InputViolatedSafetyRules` を含む `VKResult.Failure` で直ちにパイプライン全体を緊急中断（Abort）させる。

## 7. Status
✅ Accepted
