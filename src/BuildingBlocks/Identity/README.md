# VK.Blocks.Identity

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## はじめに (Introduction)

`VK.Blocks.Identity` は、エンタープライズマルチテナント SaaS およびクラウドネイティブアプリケーション向けに設計された、DDD（ドメイン駆動設計）ベースの ID・テナント管理 Building Block です。

従来の ASP.NET Core Identity のような「ドメイン、インフラ、Webプロトコルが混在した設計」から脱却し、純粋なドメイン境界、状態機、不変条件、およびポート（Port）抽象のみをカプセル化しています。

---

## アーキテクチャ (Architecture)

本モジュールは **Vertical Slice Architecture（垂直スライスアーキテクチャ）** に従い、以下の3つの独立したフィーチャースライスで構成されています。

```text
src/BuildingBlocks/Identity/
├── VKIdentityBlock.cs                 # [BB.02] 唯一のモジュールマーカー
├── VKIdentityOptions.cs               # [BB.05] 共通オプション
├── Common/                            # 共通診断・プロトコル
│   ├── Diagnostics/
│   │   ├── VKIdentityDiagnosticsConstants.cs
│   │   └── Internal/IdentityDiagnostics.cs
│   └── Protocols/
│       └── IVKIdentityContext.cs
├── User/                              # ユーザー管理スライス
│   ├── VKUserOptions.cs
│   ├── UserFeature.cs                 # [BB.06] [VKFeature]
│   ├── Contracts/VKUserErrors.cs
│   ├── Models/ (VKUser, VKUserStatus, VKEmail)
│   ├── Events/ (VKUserCreatedEvent, VKUserPasswordChangedEvent)
│   ├── Protocols/ (IVKUserRepository, IVKPasswordValidator, IVKUserClaimsPrincipalFactory)
│   └── Internal/DefaultPasswordValidator.cs
├── Tenant/                            # テナント管理スライス
│   ├── VKTenantOptions.cs
│   ├── TenantFeature.cs               # [BB.06] [VKFeature]
│   ├── Contracts/VKTenantErrors.cs
│   ├── Models/ (VKTenant, VKTenantStatus, VKTenantPlan)
│   ├── Events/ (VKTenantSuspendedEvent)
│   └── Protocols/ (IVKTenantRepository)
└── Membership/                        # テナントメンバーシップスライス
    ├── VKMembershipOptions.cs
    ├── MembershipFeature.cs           # [BB.06] [VKFeature]
    ├── Contracts/VKMembershipErrors.cs
    ├── Models/ (VKTenantMembership, VKTenantRole)
    ├── Events/ (VKUserJoinedTenantEvent)
    └── Protocols/ (IVKTenantMembershipRepository)
```

---

## 主な機能 (Key Features)

1. **純粋な集約ルートと状態機**:
   - `VKUser`: アカウント状態（Active / Locked / Disabled / PendingVerification）、パスワード変更ルール、ロックアウト管理。
   - `VKTenant`: テナント状態（Trial / Active / Suspended / Archived）、プラン・クォータ管理。
   - `VKTenantMembership`: テナントとユーザーの多対多関係、テナント内ロール（Owner / Admin / Member / Guest）。
2. **値オブジェクトと型安全性**:
   - `VKEmail`: RFC準拠のフォーマット検証および小文字正規化を保証。
   - `VKTenantPlan`: 最大ユーザー数・ストレージ容量のクォータカプセル化。
3. **ドメインイベント（Domain Events）**:
   - `VKUserCreatedEvent`, `VKUserPasswordChangedEvent`, `VKTenantSuspendedEvent`, `VKUserJoinedTenantEvent` などの発行。
4. **Source Generator との連携**:
   - `[VKBlockMarker]`, `[VKFeature]`, `[VKBlockDiagnostics]` による DI および診断コードの自動生成。

---

## 開始方法 (Getting Started)

```csharp
// DI 登録
services.AddVKIdentityBlock(builder.Configuration, options =>
{
    options.MultiTenancyEnabled = true;
})
.AddUser(options =>
{
    options.RequiredLength = 8;
    options.RequireDigit = true;
})
.AddTenant()
.AddMembership();
```
