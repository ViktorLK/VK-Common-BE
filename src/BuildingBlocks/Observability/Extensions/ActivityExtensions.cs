using System.Diagnostics;
using VK.Blocks.Core.Results;
using VK.Blocks.Observability.Conventions;

namespace VK.Blocks.Observability.Extensions;

/// <summary>
/// <see cref="Activity"/> に対する VK.Blocks 拡張メソッド群。
/// <para>
/// ビジネス操作の結果 (<see cref="IResult"/>) を OpenTelemetry スパンへ自動マッピングする
/// ブリッジを提供する。
/// </para>
/// </summary>
public static class ActivityExtensions
{
    #region Result Recording

    /// <summary>
    /// <see cref="IResult"/> の内容を現在のスパンに記録する。
    /// </summary>
    /// <param name="activity">記録対象の <see cref="Activity"/>。<c>null</c> の場合はノーオペレーション。</param>
    /// <param name="result">記録する操作結果。</param>
    /// <remarks>
    /// <para><strong>成功時:</strong> <c>vblocks.result.success=true</c> タグを追加し、ステータスは <see cref="ActivityStatusCode.Unset"/> のまま。</para>
    /// <para><strong>失敗時:</strong></para>
    /// <list type="bullet">
    ///   <item><description><c>vblocks.result.success=false</c></description></item>
    ///   <item><description><c>vblocks.result.code</c> — エラーコード</description></item>
    ///   <item><description><c>vblocks.result.message</c> — エラー説明</description></item>
    ///   <item><description><c>vblocks.error.type</c> — <see cref="ErrorType"/> の文字列表現</description></item>
    ///   <item><description>ステータス <see cref="ActivityStatusCode.Error"/></description></item>
    ///   <item><description><see cref="ActivityEvent"/> (<c>"result.failure"</c>) の記録</description></item>
    /// </list>
    /// <para>
    /// <see cref="DiagnosticConfig.ActivitySource"/> にリスナーが存在しない場合、
    /// タグの追加をスキップしてメモリアロケーションを最小化する。
    /// </para>
    /// </remarks>
    public static void RecordResult(this Activity? activity, IResult result)
    {
        // null ガード — リスナーなし時のゼロアロケーション保証
        if (activity is null)
        {
            return;
        }

        if (result.IsSuccess)
        {
            // 成功: 最小限のタグのみ追加（HasListeners チェック不要 — activity は既に生成済み）
            activity.SetTag(FieldNames.ResultSuccess, true);
        }
        else
        {
            // 失敗: エラー情報をスパンへ完全マッピング
            var firstError = result.FirstError;

            activity.SetTag(FieldNames.ResultSuccess, false);
            activity.SetTag(FieldNames.ResultCode, firstError.Code);
            activity.SetTag(FieldNames.ResultMessage, firstError.Description);
            activity.SetTag(FieldNames.ErrorType, firstError.Type.ToString());

            // スパンステータスを Error に設定
            activity.SetStatus(ActivityStatusCode.Error, firstError.Code);

            // 失敗イベントをタグ付きで記録
            var eventTags = new ActivityTagsCollection
            {
                { FieldNames.ResultCode, firstError.Code },
                { FieldNames.ResultMessage, firstError.Description },
                { FieldNames.ErrorType, firstError.Type.ToString() }
            };

            activity.AddEvent(new ActivityEvent(
                FieldNames.ResultFailure,
                tags: eventTags));
        }
    }

    #endregion

    #region Context Tag Helpers

    /// <summary>
    /// テナントIDタグ (<c>vblocks.tenant.id</c>) を設定する。
    /// </summary>
    /// <param name="activity">対象の <see cref="Activity"/>。<c>null</c> の場合はノーオペレーション。</param>
    /// <param name="tenantId">テナントID。<c>null</c> または空文字の場合はタグを追加しない。</param>
    public static void SetTenantId(this Activity? activity, string? tenantId)
    {
        if (activity is null || string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        activity.SetTag(FieldNames.TenantId, tenantId);
    }

    /// <summary>
    /// ユーザーIDタグ (<c>vblocks.user.id</c>) を設定する。
    /// </summary>
    /// <param name="activity">対象の <see cref="Activity"/>。<c>null</c> の場合はノーオペレーション。</param>
    /// <param name="userId">ユーザーID。<c>null</c> または空文字の場合はタグを追加しない。</param>
    public static void SetUserId(this Activity? activity, string? userId)
    {
        if (activity is null || string.IsNullOrEmpty(userId))
        {
            return;
        }

        activity.SetTag(FieldNames.UserId, userId);
    }

    #endregion
}
