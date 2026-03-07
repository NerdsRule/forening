

/// <summary>
/// Provides extension methods to register HTTP endpoints for user role management in API v1.
/// </summary>
/// <remarks>
/// Use MapUserRolesEndpoints during application startup to group and map all user-role-related endpoints under the "/v1" route.
/// This class serves as the registration entry point; endpoint implementations are added inside the extension method.
/// </remarks>
/// <summary>
/// Registers user-role-related endpoints on the provided <see cref="WebApplication"/> instance.
/// </summary>
/// <param name="app">The <see cref="WebApplication"/> to which the user roles endpoints will be mapped.</param>
/// <remarks>
/// This extension method creates an API version group ("/v1") and should register endpoints for operations
/// such as creating, retrieving, updating, and deleting user roles within that group.
/// Call this method during application startup (for example, in Program.cs) after building the WebApplication.
/// </remarks>
namespace Organization.ApiService.V1;

public static class UserRolesEndpoints
{
    /// <summary>
    /// Defines constants and helper methods for user role checks and information retrieval, as well as mapping user role management endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    private const string WebAuthnRegisterCachePrefix = "webauthn:register:";

    /// <summary>
    /// Defines constants and helper methods for user role checks and information retrieval, as well as mapping user role management endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    private const string WebAuthnLoginCachePrefix = "webauthn:login:";

    #region Helper methods for user role checks and information retrieval
    /// <summary>
    /// Check if user is authorized and is RolesEnum.EnterpriseAdmin in any organization
    /// </summary> <param name="user">ClaimsPrincipal</param>
    /// <param name="db">IRootDbReadWrite</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>True if user is an EnterpriseAdmin in any organization, otherwise false</returns>
    public static async Task<bool> IsUserEnterpriseAdminAsync(ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            var identity = (ClaimsIdentity)user.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return false;

            var userOrgRoles = await db.GetUserOrganizationsAsync(userId, cancellationToken);
            return userOrgRoles.Any(c => c.Role == RolesEnum.EnterpriseAdmin);
        }
        return false;
    }

    /// <summary>
    /// Checks if the user is authorized for the specified organization and role.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="organizationId"></param>
    /// <param name="departmentId"></param>
    /// <param name="role"></param>
    /// <param name="db"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> IsUserAuthorizedForOrganizationAsync(ClaimsPrincipal user, int organizationId, int departmentId, RolesEnum[] roles, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        if (user.Identity is not null && user.Identity.IsAuthenticated && organizationId > 0)
        {

            var userId = GetAuthenticatedUserId(user);

            if (userId is null) return false;

            // get user roles from TAppUserOrganization
            var userOrgRoles = await db.GetUserOrganizationsAsync(userId, cancellationToken);
            var userDepRoles = await db.GetUserDepartmentsAsync(userId, cancellationToken);
            return userOrgRoles.Any(c => c.OrganizationId == organizationId && roles.Contains(c.Role)) ||
                   userDepRoles.Any(c => c.DepartmentId == departmentId && roles.Contains(c.Role));
        }
        return false;
    }

    /// <summary>
    /// Checks if the user is authorized for the specified department and role.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="departmentId"></param>
    /// <param name="roles"></param>
    /// <param name="db"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> IsUserAuthorizedForDepartmentAsync(ClaimsPrincipal user, int departmentId, RolesEnum[] roles, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        if (user.Identity is not null && user.Identity.IsAuthenticated && departmentId > 0)
        {
            var userId = GetAuthenticatedUserId(user);

            if (userId is null) return false;

            // get user roles from TAppUserOrganization
            var userDepRoles = await db.GetUserDepartmentsAsync(userId, cancellationToken);
            var r1 = userDepRoles.Any(c => c.DepartmentId == departmentId);
            var r2 = userDepRoles.Any(c => roles.Contains(c.Role));
            bool res = userDepRoles.Any(c => c.DepartmentId == departmentId && roles.Contains(c.Role));       
            return res;
        }
        return false;
    }

    /// <summary>
    /// Checks if the user is in the same organization and has one of the specified roles.
    /// </summary>
    /// <param name="user">Instance of ClaimsPrincipal representing the authenticated user.</param>
    /// <param name="userId">User Id to check against.</param>
    /// <param name="rolesToCheck">Array of roles to check.</param>
    /// <param name="userManager">Instance of UserManager for managing user-related operations.</param>
    /// <param name="db">Instance of IRootDbReadWrite for database operations.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Returns true if the user is in the same organization and has one of the specified roles; otherwise, false.</returns>
    public static async Task<(bool hasAccess, UserModel? user)> IsUserInSameOrganizationAndInRoleAsync(ClaimsPrincipal user, string userId, RolesEnum[] rolesToCheck, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        var _loggedInUserId = GetAuthenticatedUserId(user);
        // Build logged in user roles
        var loggedInUser = await GetUserInfoAsync(_loggedInUserId!, userManager, db, cancellationToken);
        var _user = await GetUserInfoAsync(userId, userManager, db, cancellationToken);
        if (loggedInUser is not null && _user is not null)
        {
            // Check if logged in user has access to the requested user's organization and is an admin
            return (loggedInUser.AppUserOrganizations.Any(o => _user.AppUserOrganizations.Any(uo => uo.OrganizationId == o.OrganizationId) && rolesToCheck.Contains(o.Role)), _user);
        }
        return (false, null);
    }

    /// <summary>
    /// Get user information
    /// </summary>
    /// <param name="user">Instance of ClaimsPrincipal representing the authenticated user.</param>
    /// <param name="userManager">Instance of UserManager for managing user-related operations.</param>
    /// <param name="db">Instance of IRootDbReadWrite for database operations.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Returns a UserModel containing user information if authenticated; otherwise, returns an Unauthorized result.</returns>
    public static async Task<UserModel?> GetUserInfoAsync(string userId, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        var appUser = await userManager.FindByIdAsync(userId);
        if (appUser is null) return null;

        var userAppUserOrgs = await db.GetUserOrganizationsAsync(userId, cancellationToken);
        var userAppUserDeps = await db.GetUserDepartmentsAsync(userId, cancellationToken);
        var totalPointsAwarded = await db.GetTasksWithPointsAwardedByUserAsync(userId, cancellationToken);
        var totalPointsRedeemed = await db.GetPrizesByAssignedUserIdAsync(userId, cancellationToken);

        return new()
        {
            Id = appUser.Id,
            UserName = appUser.UserName ?? string.Empty,
            Email = appUser.Email ?? string.Empty,
            MemberNumber = appUser.MemberNumber,
            AppUserOrganizations = userAppUserOrgs,
            AppUserDepartments = userAppUserDeps,
            DisplayName = appUser.DisplayName ?? appUser.UserName ?? appUser.Email,
            TotalPointsAwarded = totalPointsAwarded.Sum(t => t.TaskPointsAwarded),
            TotalPointsRedeemed = totalPointsRedeemed.Where(p => p.Status == PrizeStatusEnum.Redeemed).Sum(p => p.PointsCost)
        };
    }

    /// <summary>
    /// Extracts the Fido2 service instance from the current HTTP context, configuring it with appropriate server domain, name, and allowed origins based on the incoming request.
    /// </summary>
    /// <param name="httpContext">The current HTTP context from which to derive the Fido2 configuration.</param>
    /// <returns>A configured Fido2 service instance.</returns>
    private static Fido2 GetFido2(HttpContext httpContext)
    {
        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var originHeader = httpContext.Request.Headers.Origin.ToString();
        Uri.TryCreate(originHeader, UriKind.Absolute, out var originUri);

        var configuredRpId = configuration["WebAuthn:RpId"];
        var rpId = !string.IsNullOrWhiteSpace(configuredRpId)
            ? configuredRpId
            : (originUri?.Host ?? httpContext.Request.Host.Host);

        var configuredServerName = configuration["WebAuthn:ServerName"];
        var serverName = string.IsNullOrWhiteSpace(configuredServerName) ? "Organization" : configuredServerName;

        var allowedOrigins = ApiServiceStatic.AllowedOriginsSet;

        var configuredOrigins = configuration.GetSection("WebAuthn:Origins").Get<string[]>();
        if (configuredOrigins is not null)
        {
            foreach (var origin in configuredOrigins.Where(o => !string.IsNullOrWhiteSpace(o)))
            {
                allowedOrigins.Add(origin);
            }
        }

        if (!string.IsNullOrWhiteSpace(originHeader))
        {
            allowedOrigins.Add(originHeader);
        }

        var requestOrigin = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        allowedOrigins.Add(requestOrigin);

        return new Fido2(new Fido2Configuration
        {
            ServerDomain = rpId,
            ServerName = serverName,
            Origins = allowedOrigins
        });
    }

    /// <summary>
    /// Extracts the authenticated user's ID from the provided <see cref="ClaimsPrincipal"/>. Returns an empty string if the user is not authenticated or if the ID claim is missing.
    /// </summary>
    /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the current user.</param>
    /// <returns>The authenticated user's ID as a string, or null if not authenticated.</returns>
    private static string? GetAuthenticatedUserId(ClaimsPrincipal user)
    {
        if (user.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return null;
        }

        return identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
    #endregion

    /// <summary>
    /// Registers user-role-related endpoints on the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to which the user roles endpoints will be mapped.</param>
    public static void MapUserRolesEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        #region Endpoints for WebAuthn passkey registration and management
        /// <summary>
        /// Handle the initiation of passkey registration by generating FIDO2 credential creation options and storing the request state in memory cache for later verification.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="userManager">The user manager for accessing user information.</param>
        /// <param name="appDbContext">The application database context for accessing stored credentials.</param>
        /// <param name="memoryCache">The memory cache for storing temporary request state.</param>
        /// <param name="cancellationToken">The cancellation token for async operations.</param>
        /// <returns>The asynchronous task resulting in an HTTP response containing the registration options or an error status.</returns>
        v1.MapPost("/api/users/webauthn/register/begin", async Task<IResult> (HttpContext httpContext, ClaimsPrincipal user, UserManager<AppUser> userManager, AppDbContext appDbContext, IMemoryCache memoryCache, CancellationToken cancellationToken) =>
        {
            var userId = GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var appUser = await userManager.FindByIdAsync(userId);
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var existingCredentials = await appDbContext.FidoCredentials
                .Where(c => c.AppUserId == userId)
                .Select(c => new PublicKeyCredentialDescriptor(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(c.CredentialId)))
                .ToListAsync(cancellationToken);

            var fido2 = GetFido2(httpContext);
            var fidoUser = new Fido2User
            {
                DisplayName = appUser.DisplayName ?? appUser.UserName ?? appUser.Email ?? userId,
                Name = appUser.Email ?? appUser.UserName ?? userId,
                Id = Encoding.UTF8.GetBytes(userId)
            };

            var options = fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fidoUser,
                ExcludeCredentials = existingCredentials,
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    ResidentKey = ResidentKeyRequirement.Preferred,
                    UserVerification = UserVerificationRequirement.Preferred
                },
                AttestationPreference = AttestationConveyancePreference.None,
                Extensions = new AuthenticationExtensionsClientInputs
                {
                    CredProps = true
                }
            });

            var requestId = Guid.NewGuid().ToString("N");
            memoryCache.Set($"{WebAuthnRegisterCachePrefix}{requestId}", new WebAuthnRequestState
            {
                UserId = userId,
                OptionsJson = options.ToJson()
            }, TimeSpan.FromMinutes(5));

            return Results.Ok(new WebAuthnBeginPasskeyRegistrationResult
            {
                RequestId = requestId,
                Options = JsonSerializer.Deserialize<JsonElement>(options.ToJson())
            });
        }).RequireAuthorization();

        /// <summary>
        /// Handles the completion of passkey registration by verifying the provided attestation response against the original options and, if valid, storing the new credential in the database.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="userManager">The user manager for accessing user information.</param>
        /// <param name="appDbContext">The application database context for accessing stored credentials.</param>
        /// <param name="memoryCache">The memory cache for retrieving temporary request state.</param>
        /// <param name="model">The request model containing the registration completion data.</param>
        /// <param name="cancellationToken">The cancellation token for async operations.</param>
        /// <returns>The asynchronous task resulting in an HTTP response indicating the success or failure of the registration completion.</returns>
        v1.MapPost("/api/users/webauthn/register/complete", async Task<IResult> (HttpContext httpContext, ClaimsPrincipal user, UserManager<AppUser> userManager, AppDbContext appDbContext, IMemoryCache memoryCache, [FromBody] WebAuthnCompleteRequest model, CancellationToken cancellationToken) =>
        {
            var userId = GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (model is null || string.IsNullOrWhiteSpace(model.RequestId) || string.IsNullOrWhiteSpace(model.CredentialJson))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["RequestId and credential payload are required."] });
            }

            if (!memoryCache.TryGetValue($"{WebAuthnRegisterCachePrefix}{model.RequestId}", out WebAuthnRequestState? requestState) || requestState is null)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey registration request expired. Please try again."] });
            }

            memoryCache.Remove($"{WebAuthnRegisterCachePrefix}{model.RequestId}");
            if (!string.Equals(requestState.UserId, userId, StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }

            try
            {
                var options = CredentialCreateOptions.FromJson(requestState.OptionsJson);
                var attestationResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(model.CredentialJson);
                if (attestationResponse is null)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid passkey attestation payload."] });
                }

                var fido2 = GetFido2(httpContext);
                IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, _) =>
                {
                    var credentialId = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(args.CredentialId);
                    return !await appDbContext.FidoCredentials.AnyAsync(c => c.CredentialId == credentialId, cancellationToken);
                };

                var credentialResult = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = attestationResponse,
                    OriginalOptions = options,
                    IsCredentialIdUniqueToUserCallback = callback
                });

                var newCredential = new TFidoCredential
                {
                    AppUserId = userId,
                    CredentialId = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(credentialResult.Id),
                    PublicKey = Convert.ToBase64String(credentialResult.PublicKey),
                    UserHandle = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(options.User.Id),
                    SignatureCounter = credentialResult.SignCount,
                    FriendlyName = string.IsNullOrWhiteSpace(model.FriendlyName) ? $"Passkey {DateTime.Now:yyyy-MM-dd}" : model.FriendlyName.Trim(),
                    CredentialType = credentialResult.Type.ToString(),
                    Transports = credentialResult.Transports is null ? null : string.Join(',', credentialResult.Transports)
                };

                await appDbContext.FidoCredentials.AddAsync(newCredential, cancellationToken);
                await appDbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Passkey registered successfully."] });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey registration failed.", ex.Message] });
            }
        }).RequireAuthorization();

        /// <summary>
        /// Retrieves the list of registered passkey credentials for the authenticated user, returning them in a structured format that includes metadata such as friendly name, credential type, hint, fingerprint, transports, and
        /// creation date.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="appDbContext">The application database context for accessing stored credentials.
        /// param name="cancellationToken">The cancellation token for async operations.</param>
        /// <returns>The asynchronous task resulting in an HTTP response containing the list of registered passkey credentials or an error status.</returns>
        v1.MapGet("/api/users/webauthn/credentials", async Task<IResult> (ClaimsPrincipal user, AppDbContext appDbContext, CancellationToken cancellationToken) =>
        {
            var userId = GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var credentials = (await appDbContext.FidoCredentials
                .Where(c => c.AppUserId == userId)
                .Select(c => new WebAuthnCredentialModel
                {
                    Id = c.Id,
                    FriendlyName = c.FriendlyName,
                    CredentialType = c.CredentialType,
                    CredentialHint = c.CredentialId.Length <= 12
                        ? c.CredentialId
                        : $"{c.CredentialId.Substring(0, 8)}...{c.CredentialId.Substring(c.CredentialId.Length - 4)}",
                    Fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(c.CredentialId))).Substring(0, 10),
                    Transports = c.Transports,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(cancellationToken))
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return Results.Ok(credentials);
        }).RequireAuthorization();

        /// <summary>
        /// Handle updating the friendly name of a registered passkey credential by validating the input, updating the corresponding database record, and returning an appropriate response indicating success or failure of the operation.
        /// </summary>
        /// <param name="credentialId">The ID of the credential to update.</param>
        /// <param name="model">The request model containing the new friendly name.</param>
        /// <param name="cancellationToken">The cancellation token for async operations.</param>
        /// <returns>The asynchronous task resulting in an HTTP response indicating the success or failure of the update operation.</returns>
        v1.MapDelete("/api/users/webauthn/credentials/{credentialId:int}", async Task<IResult> (ClaimsPrincipal user, AppDbContext appDbContext, int credentialId, CancellationToken cancellationToken) =>
        {
            var userId = GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var credential = await appDbContext.FidoCredentials
                .FirstOrDefaultAsync(c => c.Id == credentialId && c.AppUserId == userId, cancellationToken);
            if (credential is null)
            {
                return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Passkey not found."] });
            }

            appDbContext.FidoCredentials.Remove(credential);
            await appDbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Passkey removed."] });
        }).RequireAuthorization();

        /// <summary>
        /// Handle updating the friendly name of a registered passkey credential by validating the input, updating the corresponding database record, and returning an appropriate response indicating success or failure of the operation.
        /// </summary>
        /// <param name="credentialId">The ID of the credential to update.</param>
        /// <param name="model">The request model containing the new friendly name.</param>
        /// <param name="cancellationToken">The cancellation token for async operations.</param>
        /// <returns>The asynchronous task resulting in an HTTP response indicating the success or failure of the update operation.</returns>
        v1.MapPut("/api/users/webauthn/credentials/{credentialId:int}/friendly-name", async Task<IResult> (ClaimsPrincipal user, AppDbContext appDbContext, int credentialId, [FromBody] WebAuthnUpdateFriendlyNameRequest model, CancellationToken cancellationToken) =>
        {
            var userId = GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (model is null || string.IsNullOrWhiteSpace(model.FriendlyName))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Friendly name is required."] });
            }

            var friendlyName = model.FriendlyName.Trim();
            if (friendlyName.Length > 100)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Friendly name cannot exceed 100 characters."] });
            }

            var credential = await appDbContext.FidoCredentials
                .FirstOrDefaultAsync(c => c.Id == credentialId && c.AppUserId == userId, cancellationToken);
            if (credential is null)
            {
                return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Passkey not found."] });
            }

            credential.FriendlyName = friendlyName;
            appDbContext.FidoCredentials.Update(credential);
            await appDbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Friendly name updated."] });
        }).RequireAuthorization();

            /// <summary>
            /// Handle the initiation of passkey login by generating FIDO2 assertion options based on the user's registered credentials and storing the request state in memory cache for later verification.
            /// </summary>
            /// <param name="httpContext">The current HTTP context.</param>
            /// <param name="userManager">The user manager for accessing user information.</param>
            /// <param name="appDbContext">The application database context for accessing stored credentials.</param>
            /// <param name="memoryCache">The memory cache for storing temporary request state.</param>
            /// <param name="model">The request model containing the email for which to initiate the passkey login.</param>
            /// <param name="cancellationToken">The cancellation token for async operations.</param>
            /// <returns>The asynchronous task resulting in an HTTP response containing the assertion options or an error status.</returns>
        v1.MapPost("/api/users/webauthn/login/begin", async Task<IResult> (HttpContext httpContext, UserManager<AppUser> userManager, AppDbContext appDbContext, IMemoryCache memoryCache, [FromBody] WebAuthnBeginPasskeyLoginRequest model, CancellationToken cancellationToken) =>
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Email))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Email is required."] });
            }

            var appUser = await userManager.FindByEmailAsync(model.Email.Trim());
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var allowedCredentials = await appDbContext.FidoCredentials
                .Where(c => c.AppUserId == appUser.Id)
                .Select(c => new PublicKeyCredentialDescriptor(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(c.CredentialId)))
                .ToListAsync(cancellationToken);

            if (allowedCredentials.Count == 0)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["No passkey is registered for this account."] });
            }

            var fido2 = GetFido2(httpContext);
            var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                AllowedCredentials = allowedCredentials,
                UserVerification = UserVerificationRequirement.Preferred,
                Extensions = new AuthenticationExtensionsClientInputs
                {
                    Extensions = true
                }
            });

            var requestId = Guid.NewGuid().ToString("N");
            memoryCache.Set($"{WebAuthnLoginCachePrefix}{requestId}", new WebAuthnRequestState
            {
                UserId = appUser.Id,
                OptionsJson = options.ToJson()
            }, TimeSpan.FromMinutes(5));

            return Results.Ok(new WebAuthnBeginPasskeyRegistrationResult
            {
                RequestId = requestId,
                Options = JsonSerializer.Deserialize<JsonElement>(options.ToJson())
            });
        }).AllowAnonymous();

        /// <summary>
        /// Handle the completion of passkey login by verifying the provided assertion response against the original options, and if valid, signing in the user and returning their information.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="signInManager">The sign-in manager for handling user sign-in operations.</param>
        /// <param name="userManager">The user manager for accessing user information.</param>
        /// <param name="db">The database context for accessing user-related data.</param>
        /// <param name="appDbContext">The application database context for accessing FIDO credentials.</param>
        /// <param name="memoryCache">The memory cache for storing temporary data.</param>
        /// <param name="model">The request model containing the passkey assertion response.</param>
        /// <param name="cancellationToken">The cancellation token for async operations.</param>
        /// <returns>The asynchronous task resulting in an HTTP response containing the assertion verification result.</returns>
        v1.MapPost("/api/users/webauthn/login/complete", async Task<IResult> (HttpContext httpContext, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IRootDbReadWrite db, AppDbContext appDbContext, IMemoryCache memoryCache, [FromBody] WebAuthnCompleteRequest model, CancellationToken cancellationToken) =>
        {
            if (model is null || string.IsNullOrWhiteSpace(model.RequestId) || string.IsNullOrWhiteSpace(model.CredentialJson))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["RequestId and credential payload are required."] });
            }

            if (!memoryCache.TryGetValue($"{WebAuthnLoginCachePrefix}{model.RequestId}", out WebAuthnRequestState? requestState) || requestState is null)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey login request expired. Please try again."] });
            }

            memoryCache.Remove($"{WebAuthnLoginCachePrefix}{model.RequestId}");

            var assertionResponse = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(model.CredentialJson);
            if (assertionResponse is null)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid passkey assertion payload."] });
            }

            var credential = await appDbContext.FidoCredentials
                .FirstOrDefaultAsync(c => c.CredentialId == assertionResponse.Id && c.AppUserId == requestState.UserId, cancellationToken);
            if (credential is null)
            {
                return Results.Unauthorized();
            }

            var appUser = await userManager.FindByIdAsync(requestState.UserId);
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            try
            {
                var options = AssertionOptions.FromJson(requestState.OptionsJson);
                var fido2 = GetFido2(httpContext);

                IsUserHandleOwnerOfCredentialIdAsync callback = async (args, _) =>
                {
                    var userHandle = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(args.UserHandle);
                    return await appDbContext.FidoCredentials.AnyAsync(c =>
                        c.UserHandle == userHandle &&
                        c.CredentialId == Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(args.CredentialId) &&
                        c.AppUserId == requestState.UserId,
                        cancellationToken);
                };

                var assertionResult = await fido2.MakeAssertionAsync(new MakeAssertionParams
                {
                    AssertionResponse = assertionResponse,
                    OriginalOptions = options,
                    StoredPublicKey = Convert.FromBase64String(credential.PublicKey),
                    StoredSignatureCounter = credential.SignatureCounter,
                    IsUserHandleOwnerOfCredentialIdCallback = callback
                });

                credential.SignatureCounter = assertionResult.SignCount;
                appDbContext.FidoCredentials.Update(credential);
                await appDbContext.SaveChangesAsync(cancellationToken);

                await signInManager.SignInAsync(appUser, isPersistent: false);
                var userInfo = await GetUserInfoAsync(appUser.Id, userManager, db, cancellationToken);
                if (userInfo is null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(userInfo);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey login failed.", ex.Message] });
            }
        }).AllowAnonymous();
        #endregion

        #region Endpoints for user management
        /// <summary>
        /// Get user info endpoint
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="db">IRootDbReadWrite</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result</returns>
        v1.MapGet("/api/users/info", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken) =>
        {
            var userId = GetAuthenticatedUserId(user);
            if (userId is null) 
                return Results.Unauthorized();

            try
            {
                var userInfo = await GetUserInfoAsync(userId, userManager, db, cancellationToken);
                if (userInfo is null) 
                    return Results.Unauthorized();
                return Results.Ok(userInfo);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
        }).RequireAuthorization();

        /// <summary>
        /// Login endpoint
        /// </summary>
        /// <param name="signInManager">SignInManager</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="model">LoginModel</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/login", async Task<IResult> (SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken, [FromBody] LoginModel model) =>
        {
            if (model is null)
            {
                return Results.BadRequest(new { Error = "Request body is required." });
            }

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Results.BadRequest(new { Error = "Email and password are required." });
            }

            var appUser = await userManager.FindByEmailAsync(model.Email);
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var check = await signInManager.CheckPasswordSignInAsync(appUser, model.Password, lockoutOnFailure: false);
            if (!check.Succeeded)
            {
                return Results.Unauthorized();
            }

            await signInManager.SignInAsync(appUser, isPersistent: false);
            var userInfo = await GetUserInfoAsync(appUser.Id, userManager, db, cancellationToken);
            if (userInfo is null) return Results.Unauthorized();
            return Results.Ok(userInfo);
        }).AllowAnonymous();
        /// <summary>
        /// Registration endpoint
        /// </summary>
        /// <param name="userManager">UserManager</param>
        /// <param name="model">RegisterModel</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/register", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken, [FromBody] RegisterModel model) =>
        {
            // TODO: Check if the authenticated user has permission to add users to the organization
            if (!await IsUserAuthorizedForOrganizationAsync(user, model.OrganizationId, 0, [RolesEnum.DepartmentAdmin, RolesEnum.EnterpriseAdmin], db, cancellationToken))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["You are not authorized to add users to this organization."] });
            }
            // Validate input
            if (model is null)
            {
                return Results.BadRequest(new { Error = "Request body is required." });
            }

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Email and password are required."] });
            }

            var existing = await userManager.FindByEmailAsync(model.Email);
            if (existing is not null)
            {
                return Results.Conflict(new FormResult { Succeeded = false, ErrorList = ["Email already in use."] });
            }

            var newUser = new AppUser
            {
                UserName = string.IsNullOrWhiteSpace(model.UserName) ? model.Email : model.UserName,
                Email = model.Email,
                DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? model.UserName ?? model.Email : model.DisplayName
            };

            var createResult = await userManager.CreateAsync(newUser, model.Password);
            if (!createResult.Succeeded)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = createResult.Errors.Select(e => e.Description).ToArray() });
            }
            // Assign default role
            var org = await db.AddRowAsync<TAppUserOrganization>(new TAppUserOrganization
            {
                AppUserId = newUser.Id,
                OrganizationId = model.OrganizationId,
                Role = RolesEnum.OrganizationMember
            }, cancellationToken);

            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["User registered successfully."] });
        }).AllowAnonymous();


        /// <summary>
        /// Logout endpoint
        /// </summary>
        /// <param name="signInManager">SignInManager</param>
        /// <param name="empty">Empty object</param>
        v1.MapPost("/api/users/logout", async (SignInManager<AppUser> signInManager, [FromBody] object empty) =>
        {
            if (empty is not null)
            {
                await signInManager.SignOutAsync();

                return Results.Ok();
            }

            return Results.Unauthorized();
        }).RequireAuthorization();

        /// <summary>
        /// Get all users if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="organizationId">Organization Id</param>
        /// <param name="departmentId">Department Id</param>
        /// <returns>List of users</returns>
        v1.MapGet("/api/users/{organizationId}/{departmentId}/", async (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite rootDbReadWrite, int organizationId, int departmentId, CancellationToken cancellationToken) =>
        {
            bool hasAccess = await IsUserAuthorizedForOrganizationAsync(user, organizationId, departmentId, [RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin], rootDbReadWrite, cancellationToken);

            if (hasAccess)
            {
                var users = userManager.Users.Select(u => new UserModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    MemberNumber = u.MemberNumber
                }).ToList();
                return TypedResults.Json(users);
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Delete a user if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="userId">User Id</param>
        /// <returns>Result</returns>
        v1.MapDelete("/api/users/{userId}", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, string userId, CancellationToken cancellationToken) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                var (hasAccess, _user) = await IsUserInSameOrganizationAndInRoleAsync(user, userId, rolesToCheck, userManager, db, cancellationToken);
                {
                    var appUser = await userManager.FindByIdAsync(userId);
                    if (appUser is not null && appUser.NormalizedUserName != "LASSE.TARP@SPACE4IT.DK")
                    {
                        await userManager.DeleteAsync(appUser);
                        return Results.Ok(new FormResult { Succeeded = true });
                    }
                    else
                    {
                        return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["User not found."] });
                    }
                }
            }
            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["You are not authorized to delete this user."] });
        }).RequireAuthorization();

        /// <summary>
        /// Get a user by Id if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="db">IRootDbReadWrite</param>
        /// <param name="userId">User Id</param>
        /// <returns>UserModel</returns>
        v1.MapGet("/api/users/{userId}", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, string userId, CancellationToken cancellationToken) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                var (hasAccess, _user) = await IsUserInSameOrganizationAndInRoleAsync(user, userId, rolesToCheck, userManager, db, cancellationToken);
                if (hasAccess)
                {
                    return Results.Ok(_user);
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Update user info
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="model">UserModel</param>
        /// <returns>Result</returns>
        v1.MapPut("/api/users", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, [FromBody] UserModel model, CancellationToken cancellationToken) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
                var isModelValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
                if (!isModelValid)
                {
                    FormResult validationFormResult = new()
                    {
                        Succeeded = false,
                        ErrorList = [.. validationResults.Select(v => v.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m!)]
                    };
                    return Results.BadRequest(validationFormResult);
                }

                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                var (hasAccess, _user) = await IsUserInSameOrganizationAndInRoleAsync(user, model.Id, rolesToCheck, userManager, db, cancellationToken);
                {
                    var appUser = await userManager.FindByIdAsync(model.Id);
                    if (appUser is not null)
                    {
                        appUser.UserName = model.UserName;
                        appUser.Email = model.Email;
                        appUser.MemberNumber = model.MemberNumber;
                        appUser.DisplayName = model.DisplayName;
                        var result = await userManager.UpdateAsync(appUser);
                        if (result.Succeeded)
                        {
                            return Results.Ok(new FormResult { Succeeded = true });
                        }
                        else
                        {
                            FormResult formResult = new()
                            {
                                Succeeded = false,
                                ErrorList = [.. result.Errors.Select(e => e.Description)]
                            };
                            return Results.BadRequest(formResult);
                        }
                    }
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();
        #endregion

        #region Endpoints for getting users in organizations and departments
        /// <summary>
        /// Get users in an organization if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="db">IRootDbReadWrite</param>
        /// <param name="organizationId">Organization Id</param>
        /// <returns>List of users in the organization</returns>
        v1.MapGet("/api/users/organizations/{organizationId}", async (ClaimsPrincipal user, IRootDbReadWrite db, int organizationId, CancellationToken cancellationToken) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin };
                if (await IsUserAuthorizedForOrganizationAsync(user, organizationId, 0, rolesToCheck, db, cancellationToken))
                {
                    var users = await db.GetUsersInOrganizationAsync(organizationId, cancellationToken);
                    if (users is null || !users.Any())                    {
                        return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["No users found in this organization."] });
                    }
                    // Map users to UserModel
                    var userModels = users.Select(u => new UserModel
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        MemberNumber = u.MemberNumber
                    }).ToList();
                    return Results.Ok(userModels);
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Get users in a department if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="db">IRootDbReadWrite</param>
        /// <param name="departmentId">Department Id</param>
        /// <returns>List of users in the department</returns>
        v1.MapGet("/api/users/departments/{departmentId}", async (ClaimsPrincipal user, IRootDbReadWrite db, int departmentId, CancellationToken cancellationToken) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var rolesToCheck = new[] { RolesEnum.DepartmentAdmin, RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin };
                if (await IsUserAuthorizedForDepartmentAsync(user, departmentId, rolesToCheck, db, cancellationToken))
                {
                    var users = await db.GetUsersInDepartmentAsync(departmentId, cancellationToken);
                    if (users is null || !users.Any())                    {
                        return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["No users found in this department."] });
                    }
                    // Map users to UserModel
                    var userModels = users.Select(u => new UserModel
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        MemberNumber = u.MemberNumber
                    }).ToList();
                    return Results.Ok(userModels);
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();
        #endregion
        
        #region Password management
        /// <summary>
        /// Change password for a user
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="model">ChangePasswordModel</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/password", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, [FromBody] ChangePasswordModel model) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var appUser = await userManager.GetUserAsync(user);
                if (appUser is not null)
                {
                    IdentityResult result = await userManager.ChangePasswordAsync(appUser, model.CurrentPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return Results.Ok(new FormResult { Succeeded = true });
                    }
                    else
                    {
                        FormResult formResult = new()
                        {
                            Succeeded = false,
                            ErrorList = [.. result.Errors.Select(e => e.Description)]
                        };
                        return Results.BadRequest(formResult);
                    }

                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Reset password for a user
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="model">ResetPasswordModel</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/password/reset", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken, [FromBody] ResetPasswordModel model) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated && !string.IsNullOrEmpty(model.UserId))
            {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                var (hasAccess, _user) = await IsUserInSameOrganizationAndInRoleAsync(user, model.UserId, rolesToCheck, userManager, db, cancellationToken);
                {
                    var appUser = await userManager.FindByIdAsync(model.UserId!);
                    if (appUser is not null)
                    {
                        var resetToken = await userManager.GeneratePasswordResetTokenAsync(appUser);
                        IdentityResult result = await userManager.ResetPasswordAsync(appUser, resetToken, model.Password!);
                        if (result.Succeeded)
                        {
                            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Password has been reset successfully."] });
                        }
                        else
                        {
                            FormResult formResult = new()
                            {
                                Succeeded = false,
                                ErrorList = [.. result.Errors.Select(e => e.Description)]
                            };
                            return Results.BadRequest(formResult);
                        }
                    }
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();
        #endregion

        #region Test
        v1.MapGet("/api/users/test/{organizationId:int}", async Task<IResult> (UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IRootDbReadWrite db, CancellationToken cancellationToken, int organizationId) =>
        {
            const string testUserEmail = "testuser@example.com";
            var testUser = await userManager.FindByEmailAsync(testUserEmail);

            if (testUser is null)
            {
                testUser = new AppUser
                {
                    UserName = testUserEmail,
                    Email = testUserEmail
                };

                var createUserResult = await userManager.CreateAsync(testUser, "TestPassword123!");
                if (!createUserResult.Succeeded)
                {
                    return Results.BadRequest(new { Errors = createUserResult.Errors.Select(e => e.Description) });
                }
            }

            // Create all roles from enum
            var roleNames = Enum.GetNames(typeof(RolesEnum));
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var createRoleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!createRoleResult.Succeeded)
                    {
                        return Results.BadRequest(new { Errors = createRoleResult.Errors.Select(e => e.Description) });
                    }
                }
            }

            // Add user to all roles (only missing ones)
            var currentRoles = await userManager.GetRolesAsync(testUser);
            var missingRoles = roleNames.Where(role => !currentRoles.Contains(role)).ToArray();
            if (missingRoles.Length > 0)
            {
                var addToRolesResult = await userManager.AddToRolesAsync(testUser, missingRoles);
                if (!addToRolesResult.Succeeded)
                {
                    return Results.BadRequest(new { Errors = addToRolesResult.Errors.Select(e => e.Description) });
                }
            }

            var appUserOrganizations = await db.GetRowsAsync<TAppUserOrganization>(cancellationToken);
            var existingOrganizationMapping = appUserOrganizations.FirstOrDefault(o => o.AppUserId == testUser.Id && o.OrganizationId == organizationId);
            if (existingOrganizationMapping is null)
            {
                await db.AddRowAsync<TAppUserOrganization>(new TAppUserOrganization
                {
                    AppUserId = testUser.Id,
                    OrganizationId = organizationId,
                    Role = RolesEnum.EnterpriseAdmin
                }, cancellationToken);
            }
            else if (existingOrganizationMapping.Role != RolesEnum.EnterpriseAdmin)
            {
                existingOrganizationMapping.Role = RolesEnum.EnterpriseAdmin;
                await db.UpdateRowAsync(existingOrganizationMapping, cancellationToken);
            }

            return Results.Ok(new UserModel { Id = testUser.Id, UserName = testUser.UserName ?? string.Empty, Email = testUser.Email ?? string.Empty });
        }).AllowAnonymous();
        #endregion
    }
}
