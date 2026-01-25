
using Blazored.LocalStorage;

namespace Organization.Infrastructure.Services;

/// <summary>
/// Provides methods for retrieving and saving user settings in local storage asynchronously.
/// </summary>
/// <remarks>This service acts as a wrapper around an underlying local storage implementation, enabling the
/// application to persist and retrieve user-specific settings. All operations are performed asynchronously to avoid
/// blocking the calling thread.</remarks>
public class PrivateLocalStorageService : IPrivateLocalStorageService
{
    private readonly ILocalStorageService _localStorage;

    /// <summary>
    /// Initializes a new instance of the PrivateLocalStorageService class using the specified local storage service.
    /// </summary>
    /// <param name="localStorage">The local storage service to be used for storing and retrieving data. This parameter cannot be null.</param>
    public PrivateLocalStorageService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    /// <summary>
    /// Asynchronously retrieves the user settings from local storage.
    /// </summary>
    /// <remarks>Ensure that local storage is available and accessible before calling this method. The
    /// returned settings may be null if no user settings have been previously saved.</remarks>
    /// <param name="key">The key under which the user settings are stored.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user settings if they exist;
    /// otherwise, null.</returns>
    public async Task<UserLocalStorage?> GetUserSettingsAsync(string key)
    {
        return await _localStorage.GetItemAsync<UserLocalStorage>(key);
    }

    /// <summary>
    /// Asynchronously saves the specified user settings to local storage.
    /// </summary>
    /// <remarks>The settings are stored under the key "userSettings" in local storage. Ensure that the
    /// settings object is properly initialized before calling this method.</remarks>
    /// <param name="settings">The user settings to be saved. This parameter cannot be null.</param>
    /// <param name="key">The key under which to store the user settings.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveUserSettingsAsync(UserLocalStorage settings, string key)
    {
        await _localStorage.SetItemAsync(key, settings);
    }

    /// <summary>
    /// Removes the user settings from local storage.
    /// </summary>
    /// <param name="key">The key under which the user settings are stored.</param>
    public async Task RemoveItemAsync(string key)
    {
        await _localStorage.RemoveItemAsync(key);
    }
}

