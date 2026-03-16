namespace VK.Blocks.Observability.OpenTelemetry;

/// <summary>
/// <see cref="OtlpOptions"/> に関連する定数を定義する。
/// </summary>
internal static class OtlpOptionsConstants
{
    /// <summary>
    /// <see cref="OtlpOptions.ServiceName"/> の初期値。
    /// バリデーション時にこの値が設定されたままであることを検出・拒否するために使用される。
    /// </summary>
    internal const string DefaultServiceName = "UnknownService";
}
