namespace Organization.Shared.Interfaces;
    public interface IPrivateLocalStorageService
    {
    /// <summary>
    /// Asynchronously retrieves the user settings from local storage.
    /// </summary>
    /// <remarks>Ensure that local storage is available and accessible before calling this method. The
    /// returned settings may be null if no user settings have been previously saved.</remarks>
    /// <param name="key">The key under which the user settings are stored in local storage.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user settings if they exist;
    /// otherwise, null.</returns>
    public Task<UserLocalStorage?> GetUserSettingsAsync(string key);

    /// <summary>
    /// Asynchronously saves the specified user settings to local storage.
    /// </summary>
    /// <remarks>The settings are stored under the key "userSettings" in local storage. Ensure that the
    /// settings object is properly initialized before calling this method.</remarks>
    /// <param name="settings">The user settings to be saved. This parameter cannot be null.</param>
    /// <param name="key"
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public Task SaveUserSettingsAsync(UserLocalStorage settings, string key);

    /// <summary>
    /// Removes the user settings from local storage.
    /// </summary>
    /// <param name="key">The key under which the user settings are stored.</param>
    public Task RemoveItemAsync(string key);
    }
