
namespace Organization.Shared.Interfaces;

/// <summary>
/// Interface for account service
/// </summary>
public interface IAccountService
{
/// <summary>
        /// Login service.
        /// </summary>
        /// <param name="model">The login model containing email and password.</param>
        /// <returns>The result of the request serialized to <see cref="FormResult"/>.</returns>
        public Task<FormResult> LoginAsync(LoginModel model);

        /// <summary>
        /// Change the user's password.
        /// </summary>
        /// <param name="currentPassword">The user's current password.</param>
        /// <param name="newPassword">The user's new password.</param>
        /// <returns>The result of the password change request serialized to a <see cref="FormResult"/>.</returns>
        public Task<FormResult> ChangePasswordAsync(string currentPassword, string newPassword);

        /// <summary>
        /// Log out the logged in user.
        /// </summary>
        /// <returns>The asynchronous task.</returns>
        public Task LogoutAsync();

        /// <summary>
        /// Registration service.
        /// </summary>
        /// <param name="email">User's email.</param>
        /// <param name="password">User's password.</param>
        /// <returns>The result of the request serialized to <see cref="FormResult"/>.</returns>
        public Task<FormResult> RegisterAsync(RegisterModel model);

        public Task<bool> CheckAuthenticatedAsync();

        /// <summary>
        /// Get the roles for a user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Array of roles</returns>
        public Task<string[]> GetUserRolesAsync(string userId);

        /// <summary>
        /// Get all available roles.
        /// </summary>
        /// <returns>Array of roles</returns>
        public Task<string[]> GetAllRolesAsync();

        /// <summary>
        /// Remove roles from a user.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="roles">Array of roles</param>
        /// <returns>True if successful</returns>
        public Task<bool> RemoveRolesFromUserAsync(string userId, string[] roles);

        /// <summary>
        /// Add roles to a user.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="roles">Array of roles</param>
        /// <returns>True if successful</returns>
        public Task<bool> AddRolesToUserAsync(string userId, string[] roles);

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of UserModel</returns>
        public Task<List<UserModel>> GetUsersAsync();

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>True if successful</returns>
        public Task<bool> DeleteUserAsync(string userId);
}