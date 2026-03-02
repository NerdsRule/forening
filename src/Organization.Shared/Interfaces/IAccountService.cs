
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
    /// Begin passkey login challenge for a user email.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <returns>Passkey challenge and options or an error result.</returns>
    public Task<(WebAuthnBeginPasskeyRegistrationResult? result, FormResult? error)> BeginPasskeyLoginAsync(string email);

    /// <summary>
    /// Complete passkey login assertion.
    /// </summary>
    /// <param name="model">Completion request payload.</param>
    /// <returns>Operation status.</returns>
    public Task<FormResult> CompletePasskeyLoginAsync(WebAuthnCompleteRequest model);

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
    /// Begin passkey registration for the authenticated user.
    /// </summary>
    /// <returns>Passkey challenge and options or an error result.</returns>
    public Task<(WebAuthnBeginPasskeyRegistrationResult? result, FormResult? error)> BeginPasskeyRegistrationAsync();

    /// <summary>
    /// Complete passkey registration attestation.
    /// </summary>
    /// <param name="model">Completion request payload.</param>
    /// <returns>Operation status.</returns>
    public Task<FormResult> CompletePasskeyRegistrationAsync(WebAuthnCompleteRequest model);

    /// <summary>
    /// Get passkeys registered for the authenticated user.
    /// </summary>
    /// <returns>Registered passkeys.</returns>
    public Task<List<WebAuthnCredentialModel>> GetPasskeyCredentialsAsync();

    /// <summary>
    /// Remove a passkey credential for the authenticated user.
    /// </summary>
    /// <param name="credentialId">Credential row id.</param>
    /// <returns>Operation status.</returns>
    public Task<FormResult> DeletePasskeyCredentialAsync(int credentialId);

    /// <summary>
    /// Update friendly name for an existing passkey.
    /// </summary>
    /// <param name="credentialId">Credential row id.</param>
    /// <param name="friendlyName">Friendly name to store.</param>
    /// <returns>Operation status.</returns>
    public Task<FormResult> UpdatePasskeyFriendlyNameAsync(int credentialId, string friendlyName);

    /// <summary>
    /// Check if the user is authenticated.
    /// </summary>
    /// <returns>True if authenticated</returns>
    public Task<bool> CheckAuthenticatedAsync();
    
    /// <summary>
    /// Get all users
    /// </summary>
    /// <param name="organizationId">Organization Id</param>
    /// <param name="departmentId">Department Id</param>
    /// <returns>List of UserModel</returns>
    public Task<List<UserModel>> GetUsersAsync(int organizationId, int departmentId);

    /// <summary>
    /// Delete user
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if successful</returns>
    public Task<FormResult> DeleteUserAsync(string userId, CancellationToken ct);

    // <summary>
    /// Get user by Id
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>UserModel</returns>
    public Task<UserModel?> GetUserByIdAsync(string userId, CancellationToken ct);

    /// <summary>
    /// Update user
    /// </summary>
    /// <param name="user">User model</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>FormResult</returns>
    public Task<FormResult> UpdateUserAsync(UserModel user, CancellationToken ct);

    #region Password Management
    /// <summary>
    /// Change the user's password.
    /// </summary>
    /// <param name="model">The change password model containing current and new password.</param>
    /// <returns>The result of the password change request serialized to a <see cref="FormResult"/>.</returns>
    public Task<FormResult> ChangePasswordAsync(ChangePasswordModel model);

    /// <summary>
    /// Reset password for user
    /// </summary>
    /// <param name="model">ResetPasswordModel</param>
    /// <returns>FormResult</returns>
    public Task<FormResult> ResetPasswordAsync(ResetPasswordModel model);
    #endregion

    #region TAppUserOrganization
    /// <summary>
    /// Add or update TAppUserOrganization
    /// </summary>
    /// <param name="appUserOrganization">TAppUserOrganization model</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated TAppUserOrganization</returns>
    public Task<(TAppUserOrganization?, FormResult?)> AddUpdateAppUserOrganizationAsync(TAppUserOrganization appUserOrganization, CancellationToken ct);
    /// <summary>
    /// Delete TAppUserOrganization
    /// </summary>
    /// <param name="appUserOrganization">TAppUserOrganization model</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>FormResult</returns>
    public Task<FormResult> DeleteAppUserOrganizationAsync(TAppUserOrganization appUserOrganization, CancellationToken ct);
    #endregion

    #region TAppUserDepartment
    /// <summary>
    /// Add or update TAppUserDepartment
    /// </summary>
    /// <param name="appUserDepartment">TAppUserDepartment model</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated TAppUserDepartment</returns>
    public Task<(TAppUserDepartment? appUserDepartment, FormResult? formResult)> AddUpdateAppUserDepartmentAsync(TAppUserDepartment appUserDepartment, CancellationToken ct);
    /// <summary>
    /// Delete TAppUserDepartment
    /// </summary>
    /// <param name="appUserDepartment">TAppUserDepartment model</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>FormResult</returns>
    public Task<FormResult> DeleteAppUserDepartmentAsync(TAppUserDepartment appUserDepartment, CancellationToken ct);
    #endregion

    #region TOrganization and TDepartment
    /// <summary>
    /// Get departments by organization Id
    /// </summary>
    /// <param name="organizationId">Organization Id</param>
    /// <param name="userId">User Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>TOrganization</returns>
    public Task<(List<TDepartment>? departments, FormResult? formResult)> GetDepartmentsByOrganizationIdAsync(int organizationId, string userId, CancellationToken ct);
    #endregion
}