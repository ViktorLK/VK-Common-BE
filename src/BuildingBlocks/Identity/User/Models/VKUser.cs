using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain aggregate root for a User within the identity bounded context.
/// Follows AP.01, CS.01, CS.05.
/// </summary>
public sealed class VKUser : VKAggregateRoot<VKUserId>
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the unique email address.
    /// </summary>
    public VKEmail Email { get; private set; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Gets the contact phone number.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Gets the avatar image URL.
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Gets the external IdP identity identifier (e.g. Auth0, Google sub).
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Gets the user account status.
    /// </summary>
    public VKUserStatus Status { get; private set; }

    /// <summary>
    /// Gets a value indicating whether email is confirmed.
    /// </summary>
    public bool IsEmailConfirmed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether phone number is confirmed.
    /// </summary>
    public bool IsPhoneNumberConfirmed { get; private set; }

    /// <summary>
    /// Gets the last login timestamp.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; private set; }

    /// <summary>
    /// Gets the user settings.
    /// </summary>
    public VKUserSettings Settings { get; private set; }

    /// <summary>
    /// Gets the timestamp when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the user was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKUser(
        VKUserId id,
        VKEmail email,
        string? displayName,
        string? phoneNumber,
        string? avatarUrl,
        string? externalId,
        VKUserStatus status,
        bool isEmailConfirmed,
        bool isPhoneNumberConfirmed,
        DateTimeOffset? lastLoginAt,
        VKUserSettings? settings,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) : base(id)
    {
        Email = email;
        DisplayName = displayName;
        PhoneNumber = phoneNumber;
        AvatarUrl = avatarUrl;
        ExternalId = externalId;
        Status = status;
        IsEmailConfirmed = isEmailConfirmed;
        IsPhoneNumberConfirmed = isPhoneNumberConfirmed;
        LastLoginAt = lastLoginAt;
        Settings = settings ?? VKUserSettings.Default;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new user aggregate root.
    /// </summary>
    public static VKResult<VKUser> Create(
        VKUserId id,
        VKEmail email,
        DateTimeOffset now,
        string? displayName = null,
        string? phoneNumber = null,
        string? avatarUrl = null,
        string? externalId = null,
        bool requireEmailVerification = false,
        VKUserSettings? settings = null)
    {
        VKGuard.NotDefault(id);
        VKGuard.NotNull(email);

        var initialStatus = requireEmailVerification
            ? VKUserStatus.PendingVerification
            : VKUserStatus.Active;

        var user = new VKUser(
            id: id,
            email: email,
            displayName: displayName,
            phoneNumber: phoneNumber,
            avatarUrl: avatarUrl,
            externalId: externalId,
            status: initialStatus,
            isEmailConfirmed: !requireEmailVerification,
            isPhoneNumberConfirmed: false,
            lastLoginAt: null,
            settings: settings ?? VKUserSettings.Default,
            createdAt: now,
            updatedAt: now);

        user.RaiseDomainEvent(new VKUserCreatedEvent(id, email, now));
        return VKResult.Success(user);
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKUser Rehydrate(
        VKUserId id,
        VKEmail email,
        string? displayName,
        string? phoneNumber,
        string? avatarUrl,
        string? externalId,
        VKUserStatus status,
        bool isEmailConfirmed,
        bool isPhoneNumberConfirmed,
        DateTimeOffset? lastLoginAt,
        VKUserSettings settings,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        return new VKUser(
            id,
            email,
            displayName,
            phoneNumber,
            avatarUrl,
            externalId,
            status,
            isEmailConfirmed,
            isPhoneNumberConfirmed,
            lastLoginAt,
            settings,
            createdAt,
            updatedAt ?? createdAt);
    }

    // =========================================================================
    // Domain Invariants & Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    public VKResult UpdateProfile(
        string? displayName,
        string? phoneNumber,
        string? avatarUrl,
        DateTimeOffset now)
    {
        if (Status == VKUserStatus.Disabled)
        {
            return VKResult.Failure(VKUserErrors.UserDisabled);
        }

        DisplayName = displayName;
        PhoneNumber = phoneNumber;
        AvatarUrl = avatarUrl;
        UpdatedAt = now;

        RaiseDomainEvent(new VKUserProfileUpdatedEvent(Id, displayName, phoneNumber, avatarUrl, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Updates user localization and system preferences.
    /// </summary>
    public VKResult UpdateSettings(VKUserSettings settings, DateTimeOffset now)
    {
        VKGuard.NotNull(settings);

        if (Status == VKUserStatus.Disabled)
        {
            return VKResult.Failure(VKUserErrors.UserDisabled);
        }

        Settings = settings;
        UpdatedAt = now;

        RaiseDomainEvent(new VKUserSettingsUpdatedEvent(Id, settings, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Confirms email address verification.
    /// </summary>
    public VKResult ConfirmEmail(DateTimeOffset now)
    {
        var nextStatus = Status == VKUserStatus.PendingVerification
            ? VKUserStatus.Active
            : Status;

        IsEmailConfirmed = true;
        Status = nextStatus;
        UpdatedAt = now;

        RaiseDomainEvent(new VKUserEmailConfirmedEvent(Id, Email, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Records user login activity.
    /// </summary>
    public void RecordLogin(DateTimeOffset now)
    {
        LastLoginAt = now;
        UpdatedAt = now;

        RaiseDomainEvent(new VKUserLoggedInEvent(Id, now));
    }

    /// <summary>
    /// Permanently disables the user account.
    /// </summary>
    public VKResult Disable(DateTimeOffset now)
    {
        if (Status == VKUserStatus.Disabled)
        {
            return VKResult.Failure(VKUserErrors.UserDisabled);
        }

        Status = VKUserStatus.Disabled;
        UpdatedAt = now;

        RaiseDomainEvent(new VKUserDisabledEvent(Id, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Re-activates a disabled user account.
    /// </summary>
    public VKResult Activate(DateTimeOffset now)
    {
        if (Status == VKUserStatus.Active)
        {
            return VKResult.Success();
        }

        Status = VKUserStatus.Active;
        UpdatedAt = now;

        RaiseDomainEvent(new VKUserActivatedEvent(Id, now));
        return VKResult.Success();
    }
}
