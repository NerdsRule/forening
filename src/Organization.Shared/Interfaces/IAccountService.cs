
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
    /// <param name="model">The change password model containing current and new password.</param>
    /// <returns>The result of the password change request serialized to a <see cref="FormResult"/>.</returns>
    public Task<FormResult> ChangePasswordAsync(ChangePasswordModel model);

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

    /// <summary>
    /// Check if the user is authenticated.
    /// </summary>
    /// <returns>True if authenticated</returns>
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

    // <summary>
    /// Get user by Id
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <returns>UserModel</returns>
    public Task<UserModel?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Update user
    /// </summary>
    /// <param name="user">User model</param>
    /// <returns>FormResult</returns>
    public Task<FormResult> UpdateUserAsync(UserModel user);

    #region TAppUserOrganization
    /// <summary>
    /// Add or update TAppUserOrganization
    /// </summary>
    /// <param name="appUserOrganization">TAppUserOrganization model</param>
    /// <returns>Updated TAppUserOrganization</returns>
    public Task<(TAppUserOrganization?, FormResult?)> AddUpdateAppUserOrganizationAsync(TAppUserOrganization appUserOrganization);
    /// <summary>
    /// Delete TAppUserOrganization
    /// </summary>
    /// <param name="appUserOrganization">TAppUserOrganization model</param>
    /// <returns>FormResult</returns>
    public Task<FormResult> DeleteAppUserOrganizationAsync(TAppUserOrganization appUserOrganization);
    #endregion

    #region TAppUserDepartment
    /// <summary>
    /// Add or update TAppUserDepartment
    /// </summary>
    /// <param name="appUserDepartment">TAppUserDepartment model</param>
    /// <returns>Updated TAppUserDepartment</returns>
    public Task<(TAppUserDepartment? appUserDepartment, FormResult? formResult)> AddUpdateAppUserDepartmentAsync(TAppUserDepartment appUserDepartment);
    /// <summary>
    /// Delete TAppUserDepartment
    /// </summary>
    /// <param name="appUserDepartment">TAppUserDepartment model</param>
    /// <returns>FormResult</returns>
    public Task<FormResult> DeleteAppUserDepartmentAsync(TAppUserDepartment appUserDepartment);
    #endregion

    #region TOrganization and TDepartment
    /// <summary>
    /// Get departments by organization Id
    /// </summary>
    /// <param name="organizationId">Organization Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>TOrganization</returns>
    public Task<(List<TDepartment>? departments, FormResult? formResult)> GetDepartmentsByOrganizationIdAsync(int organizationId, CancellationToken ct);
    #endregion
}