using Microsoft.AspNetCore.Authorization;

namespace VK.Lab.CleanArchitecture.Authorization
{
    /// <summary>
    /// APIキー認証はGETリクエストのみ許可する要件
    /// </summary>
    public class ApiKeyReadOnlyRequirement : IAuthorizationRequirement
    {
        // マーカー要件（追加のプロパティは不要）
    }
}
