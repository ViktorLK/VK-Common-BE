using Microsoft.AspNetCore.Authorization;

namespace VK.Lab.LayeredArchitecture.Authorization
{
    /// <summary>
    /// APIキー認証はGETリクエスト�Eみ許可する要件
    /// </summary>
    public class ApiKeyReadOnlyRequirement : IAuthorizationRequirement
    {
        // マ�Eカー要件�E�追加のプロパティは不要E��E
    }
}
