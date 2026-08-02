
using System.Diagnostics;

namespace Organization.Shared.Identity;

/// <summary>
/// Static user info for Blazor clients
/// </summary>
public static class StaticUserInfoBlazor
{
    
    /// <summary>
    /// Key used for local storage and user settings
    /// </summary>
    public const string UserLocalStorageKey = "UserInfo";

    /// <summary>
    /// Key used for local storage user name used for passkey login
    /// </summary>
    public const string PasskeyLoginEmailStorageKey = "PasskeyLoginEmail";

}
