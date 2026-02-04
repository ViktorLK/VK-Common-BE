using Microsoft.AspNetCore.Authorization;

namespace VK_Common_BE.Authorization
{
    /// <summary>
    /// APIキー認証はGETリクエストのみ許可する要件
    /// </summary>
    public class ApiKeyReadOnlyRequirement : IAuthorizationRequirement
    {
        // マーカー要件（追加のプロパティは不要）
    }
}
