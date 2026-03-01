

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
            var identity = (ClaimsIdentity)user.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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
            var identity = (ClaimsIdentity)user.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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
        var identity = (ClaimsIdentity)user.Identity!;
        var _loggedInUserId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

        return new()
        {
            Id = appUser.Id,
            UserName = appUser.UserName ?? string.Empty,
            Email = appUser.Email ?? string.Empty,
            MemberNumber = appUser.MemberNumber,
            AppUserOrganizations = userAppUserOrgs,
            AppUserDepartments = userAppUserDeps,
            DisplayName = appUser.DisplayName ?? appUser.UserName ?? appUser.Email,
            TotalPointsAwarded = totalPointsAwarded.Sum(t => t.TaskPointsAwarded)
        };
    }

    /// <summary>
    /// Helper method to encode byte array to Base64 URL format.
    /// </summary>
    /// <param name="data">The byte array to encode.</param>
    /// <returns>A Base64 URL encoded string.</returns>
    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    /// Helper method to decode a Base64 URL encoded string to a byte array.
    /// </summary>
    /// <param name="data">The Base64 URL encoded string to decode.</param>
    /// <returns>The decoded byte array.</returns>
    private static byte[] Base64UrlDecode(string data)
    {
        var incoming = data.Replace('-', '+').Replace('_', '/');
        switch (incoming.Length % 4)
        {
            case 2:
                incoming += "==";
                break;
            case 3:
                incoming += "=";
                break;
        }
        return Convert.FromBase64String(incoming);
    }

    /// <summary>
    /// Validates the client data received from the WebAuthn API against expected values for type, challenge, and origin.
    /// </summary>
    /// <param name="clientDataJsonBase64Url">The client data in Base64 URL encoded format.</param>
    /// <param name="expectedType">The expected type of the WebAuthn operation (e.g., "webauthn.create" or "webauthn.get").</param>
    /// <param name="expectedChallenge">The expected challenge that was originally sent to the client.</param>
    /// <param name="expectedOrigin">The expected origin (e.g., "https://example.com") that should match the client's origin.</param>
    /// <returns>True if the client data is valid and matches the expected values; otherwise, false.</returns>
    private static bool TryValidateClientData(string clientDataJsonBase64Url, string expectedType, string expectedChallenge, string expectedOrigin)
    {
        try
        {
            var clientDataBytes = Base64UrlDecode(clientDataJsonBase64Url);
            using var document = JsonDocument.Parse(clientDataBytes);

            var root = document.RootElement;
            var type = root.GetProperty("type").GetString();
            var challenge = root.GetProperty("challenge").GetString();
            var origin = root.GetProperty("origin").GetString();

            var challengeMatches = false;
            if (!string.IsNullOrWhiteSpace(challenge))
            {
                var incomingChallengeBytes = Base64UrlDecode(challenge);
                var expectedChallengeBytes = Base64UrlDecode(expectedChallenge);
                challengeMatches = incomingChallengeBytes.AsSpan().SequenceEqual(expectedChallengeBytes);
            }

            return string.Equals(type, expectedType, StringComparison.Ordinal)
                && challengeMatches
                && string.Equals(origin, expectedOrigin, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifies the RP ID hash in the authenticator data against the expected RP ID and extracts the signature counter if valid.
    /// </summary>
    /// <param name="authenticatorDataBase64Url">The authenticator data in Base64 URL encoded format.</param>
    /// <param name="rpId">The expected RP ID (e.g., "example.com") that should match the hash in the authenticator data.</param>
    /// <param name="signCounter">Outputs the signature counter extracted from the authenticator data if the RP ID hash is valid.</param>
    /// <returns>True if the RP ID hash is valid and matches the expected RP ID; otherwise, false.</returns>
    private static bool TryVerifyAuthenticatorDataRpIdHash(string authenticatorDataBase64Url, string rpId, out uint signCounter)
    {
        signCounter = 0;
        try
        {
            var authenticatorData = Base64UrlDecode(authenticatorDataBase64Url);
            if (authenticatorData.Length < 37)
            {
                return false;
            }

            var expectedRpIdHash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(rpId));
            for (var i = 0; i < 32; i++)
            {
                if (authenticatorData[i] != expectedRpIdHash[i])
                {
                    return false;
                }
            }

            signCounter = (uint)(authenticatorData[33] << 24 | authenticatorData[34] << 16 | authenticatorData[35] << 8 | authenticatorData[36]);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifies the signature of the assertion using the provided public key and client data.
    /// </summary>
    /// <param name="publicKeySpki">The public key in Subject Public Key Info (SPKI) format.</param>
    /// <param name="authenticatorDataBase64Url">The authenticator data in Base64 URL encoded format.</param>
    /// <param name="clientDataJsonBase64Url">The client data in Base64 URL encoded format.</param>
    /// <param name="signatureBase64Url">The signature in Base64 URL encoded format.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    private static bool VerifyAssertionSignature(byte[] publicKeySpki, string authenticatorDataBase64Url, string clientDataJsonBase64Url, string signatureBase64Url)
    {
        try
        {
            var authenticatorData = Base64UrlDecode(authenticatorDataBase64Url);
            var clientDataJson = Base64UrlDecode(clientDataJsonBase64Url);
            var signature = Base64UrlDecode(signatureBase64Url);
            var clientDataHash = System.Security.Cryptography.SHA256.HashData(clientDataJson);

            var signedData = new byte[authenticatorData.Length + clientDataHash.Length];
            Buffer.BlockCopy(authenticatorData, 0, signedData, 0, authenticatorData.Length);
            Buffer.BlockCopy(clientDataHash, 0, signedData, authenticatorData.Length, clientDataHash.Length);

            using var ecdsa = System.Security.Cryptography.ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKeySpki, out _);
            return ecdsa.VerifyData(
                signedData,
                signature,
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                System.Security.Cryptography.DSASignatureFormat.Rfc3279DerSequence);
        }
        catch
        {
            return false;
        }
    }
    #endregion

    /// <summary>
    /// Registers user-role-related endpoints on the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to which the user roles endpoints will be mapped.</param>
    public static void MapUserRolesEndpoints(this WebApplication app)
    {

        var v1 = app.MapGroup("/v1");

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
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return Results.Unauthorized();

                var userInfo = await GetUserInfoAsync(userId, userManager, db, cancellationToken);
                if (userInfo is null) return Results.Unauthorized();
                return Results.Ok(userInfo);
            }
            //return Results.Ok();
            return Results.Unauthorized();
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
        /// WebAuthn registration options endpoint
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="appDbContext">AppDbContext</param>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/webauth/register/options", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, AppDbContext appDbContext, HttpContext httpContext, CancellationToken cancellationToken) =>
        {
            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var identity = (ClaimsIdentity)user.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var appUser = await userManager.FindByIdAsync(userId);
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var rpId = httpContext.Request.Host.Host;
            var originHeader = httpContext.Request.Headers.Origin.ToString();
            var origin = string.IsNullOrWhiteSpace(originHeader)
                ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
                : originHeader;
            var challenge = Base64UrlEncode(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

            appDbContext.WebAuthChallenges.RemoveRange(appDbContext.WebAuthChallenges.Where(c => c.AppUserId == userId && c.Purpose == "register"));

            await appDbContext.WebAuthChallenges.AddAsync(new TWebAuthChallenge
            {
                AppUserId = userId,
                Purpose = "register",
                Challenge = challenge,
                Origin = origin,
                RelyingPartyId = rpId,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
            }, cancellationToken);
            await appDbContext.SaveChangesAsync(cancellationToken);

            var options = new WebAuthOptionsResponse
            {
                Challenge = challenge,
                RpId = rpId,
                RpName = "Organization",
                UserId = Base64UrlEncode(Encoding.UTF8.GetBytes(appUser.Id)),
                UserName = appUser.UserName ?? appUser.Email ?? string.Empty,
                DisplayName = appUser.DisplayName ?? appUser.UserName ?? appUser.Email ?? string.Empty,
                TimeoutMs = 60000
            };

            return Results.Ok(options);
        }).RequireAuthorization();

        /// <summary>
        /// WebAuthn registration verification endpoint
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="model">WebAuthRegisterCompleteRequest</param>
        /// <param name="appDbContext">AppDbContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/webauth/register/verify", async Task<IResult> (ClaimsPrincipal user, [FromBody] WebAuthRegisterCompleteRequest model, AppDbContext appDbContext, CancellationToken cancellationToken) =>
        {
            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var identity = (ClaimsIdentity)user.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var challenge = await appDbContext.WebAuthChallenges
                .Where(c => c.AppUserId == userId && c.Purpose == "register")
                .OrderByDescending(c => c.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (challenge is null || challenge.ExpiresAtUtc < DateTime.UtcNow)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Registration challenge expired."] });
            }

            var validClientData = TryValidateClientData(model.ClientDataJson, "webauthn.create", challenge.Challenge, challenge.Origin);
            if (!validClientData)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid WebAuth client data."] });
            }

            var existing = await appDbContext.WebAuthCredentials.FirstOrDefaultAsync(c => c.CredentialId == model.CredentialId, cancellationToken);
            if (existing is not null)
            {
                return Results.Conflict(new FormResult { Succeeded = false, ErrorList = ["Credential already registered."] });
            }

            byte[] publicKeySpki;
            try
            {
                publicKeySpki = Base64UrlDecode(model.PublicKeySpki);
            }
            catch
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid public key payload."] });
            }

            await appDbContext.WebAuthCredentials.AddAsync(new TWebAuthCredential
            {
                AppUserId = userId,
                CredentialId = model.CredentialId,
                PublicKeySpki = publicKeySpki,
                PublicKeyAlgorithm = model.PublicKeyAlgorithm,
                FriendlyName = model.FriendlyName,
                SignatureCounter = 0,
                CreatedAtUtc = DateTime.UtcNow
            }, cancellationToken);

            appDbContext.WebAuthChallenges.Remove(challenge);
            await appDbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Passkey registered successfully."] });
        }).RequireAuthorization();

        /// <summary>
        /// WebAuthn login options endpoint
        /// </summary> 
        /// <param name="model">WebAuthAuthenticationOptionsRequest</param>
        /// <param name="userManager">UserManager<AppUser></param>
        /// <param name="appDbContext">AppDbContext</param>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/webauth/login/options", async Task<IResult> ([FromBody] WebAuthAuthenticationOptionsRequest model, UserManager<AppUser> userManager, AppDbContext appDbContext, HttpContext httpContext, CancellationToken cancellationToken) =>
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Email))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Email is required."] });
            }

            var appUser = await userManager.FindByEmailAsync(model.Email);
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var credentials = await appDbContext.WebAuthCredentials
                .Where(c => c.AppUserId == appUser.Id)
                .Select(c => c.CredentialId)
                .ToListAsync(cancellationToken);

            if (credentials.Count == 0)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["No passkey is registered for this user."] });
            }

            var rpId = httpContext.Request.Host.Host;
            var originHeader = httpContext.Request.Headers.Origin.ToString();
            var origin = string.IsNullOrWhiteSpace(originHeader)
                ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
                : originHeader;
            var challenge = Base64UrlEncode(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

            appDbContext.WebAuthChallenges.RemoveRange(appDbContext.WebAuthChallenges.Where(c => c.AppUserId == appUser.Id && c.Purpose == "authenticate"));

            await appDbContext.WebAuthChallenges.AddAsync(new TWebAuthChallenge
            {
                AppUserId = appUser.Id,
                Purpose = "authenticate",
                Challenge = challenge,
                Origin = origin,
                RelyingPartyId = rpId,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
            }, cancellationToken);

            await appDbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new WebAuthOptionsResponse
            {
                Challenge = challenge,
                RpId = rpId,
                RpName = "Organization",
                UserId = Base64UrlEncode(Encoding.UTF8.GetBytes(appUser.Id)),
                UserName = appUser.UserName ?? appUser.Email ?? string.Empty,
                DisplayName = appUser.DisplayName ?? appUser.UserName ?? appUser.Email ?? string.Empty,
                AllowCredentialIds = credentials,
                TimeoutMs = 60000
            });
        }).AllowAnonymous();

        /// <summary>
        /// WebAuthn login verification endpoint
        /// </summary>
        /// <param name="model">WebAuthAuthenticateCompleteRequest</param>
        /// <param name="signInManager">SignInManager<AppUser></param>
        /// <param name="userManager">UserManager<AppUser></param>
        /// <param name="db">IRootDbReadWrite</param>
        /// <param name="appDbContext">AppDbContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/webauth/login/verify", async Task<IResult> ([FromBody] WebAuthAuthenticateCompleteRequest model, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IRootDbReadWrite db, AppDbContext appDbContext, CancellationToken cancellationToken) =>
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Email))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Email is required."] });
            }

            var appUser = await userManager.FindByEmailAsync(model.Email);
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var challenge = await appDbContext.WebAuthChallenges
                .Where(c => c.AppUserId == appUser.Id && c.Purpose == "authenticate")
                .OrderByDescending(c => c.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (challenge is null || challenge.ExpiresAtUtc < DateTime.UtcNow)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Authentication challenge expired."] });
            }

            var credential = await appDbContext.WebAuthCredentials
                .FirstOrDefaultAsync(c => c.AppUserId == appUser.Id && c.CredentialId == model.CredentialId, cancellationToken);

            if (credential is null)
            {
                return Results.Unauthorized();
            }

            var validClientData = TryValidateClientData(model.ClientDataJson, "webauthn.get", challenge.Challenge, challenge.Origin);
            if (!validClientData)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid WebAuth client data."] });
            }

            if (!TryVerifyAuthenticatorDataRpIdHash(model.AuthenticatorData, challenge.RelyingPartyId, out var signCounter))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid authenticator data."] });
            }

            var signatureValid = VerifyAssertionSignature(credential.PublicKeySpki, model.AuthenticatorData, model.ClientDataJson, model.Signature);
            if (!signatureValid)
            {
                return Results.Unauthorized();
            }

            if (signCounter > 0 && credential.SignatureCounter > 0 && signCounter <= credential.SignatureCounter)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey counter validation failed."] });
            }

            credential.SignatureCounter = signCounter;
            credential.LastUsedAtUtc = DateTime.UtcNow;
            appDbContext.WebAuthChallenges.Remove(challenge);
            await appDbContext.SaveChangesAsync(cancellationToken);

            await signInManager.SignInAsync(appUser, isPersistent: false);

            var userInfo = await GetUserInfoAsync(appUser.Id, userManager, db, cancellationToken);
            if (userInfo is null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(userInfo);
        }).AllowAnonymous();

        /// <summary>
        /// Get registered WebAuthn credentials endpoint
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="appDbContext">AppDbContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result</returns>
        v1.MapGet("/api/users/webauth/credentials", async Task<IResult> (ClaimsPrincipal user, AppDbContext appDbContext, CancellationToken cancellationToken) =>
        {
            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var identity = (ClaimsIdentity)user.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var credentials = await appDbContext.WebAuthCredentials
                .Where(c => c.AppUserId == userId)
                .OrderByDescending(c => c.CreatedAtUtc)
                .Select(c => new WebAuthCredentialModel
                {
                    Id = c.Id,
                    CredentialId = c.CredentialId,
                    FriendlyName = c.FriendlyName,
                    CreatedAtUtc = c.CreatedAtUtc,
                    LastUsedAtUtc = c.LastUsedAtUtc
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(credentials);
        }).RequireAuthorization();

        /// <summary>
        /// Update WebAuthn credential friendly name endpoint
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="credentialId">Credential Id</param>
        /// <param name="model">WebAuthCredentialRenameRequest</param>
        /// <param name="appDbContext">AppDbContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result</returns>
        v1.MapPut("/api/users/webauth/credentials/{credentialId:int}", async Task<IResult> (ClaimsPrincipal user, int credentialId, [FromBody] WebAuthCredentialRenameRequest model, AppDbContext appDbContext, CancellationToken cancellationToken) =>
        {
            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var identity = (ClaimsIdentity)user.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var credential = await appDbContext.WebAuthCredentials
                .FirstOrDefaultAsync(c => c.Id == credentialId && c.AppUserId == userId, cancellationToken);

            if (credential is null)
            {
                return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Passkey not found."] });
            }

            var friendlyName = model?.FriendlyName?.Trim();
            if (!string.IsNullOrWhiteSpace(friendlyName) && friendlyName.Length > 200)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey name cannot exceed 200 characters."] });
            }

            credential.FriendlyName = string.IsNullOrWhiteSpace(friendlyName) ? null : friendlyName;
            await appDbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Passkey name updated."] });
        }).RequireAuthorization();

        /// <summary>
        /// Delete WebAuthn credential endpoint
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="credentialId">Credential Id</param>
        /// <param name="appDbContext">AppDbContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result</returns>
        v1.MapDelete("/api/users/webauth/credentials/{credentialId:int}", async Task<IResult> (ClaimsPrincipal user, int credentialId, AppDbContext appDbContext, CancellationToken cancellationToken) =>
        {
            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var identity = (ClaimsIdentity)user.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var credential = await appDbContext.WebAuthCredentials
                .FirstOrDefaultAsync(c => c.Id == credentialId && c.AppUserId == userId, cancellationToken);

            if (credential is null)
            {
                return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Passkey not found."] });
            }

            appDbContext.WebAuthCredentials.Remove(credential);
            await appDbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Passkey deleted."] });
        }).RequireAuthorization();

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
            // Create test user
            var testUser = new AppUser
            {
                UserName = "testuser@example.com",
                Email = "testuser@example.com"
            };

            var createUserResult = await userManager.CreateAsync(testUser, "TestPassword123!");
            if (!createUserResult.Succeeded)
            {
                return Results.BadRequest(new { Errors = createUserResult.Errors.Select(e => e.Description) });
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

            // Add user to all roles
            var addToRolesResult = await userManager.AddToRolesAsync(testUser, roleNames);
            if (!addToRolesResult.Succeeded)
            {
                return Results.BadRequest(new { Errors = addToRolesResult.Errors.Select(e => e.Description) });
            }

            var org = await db.AddRowAsync<TAppUserOrganization>(new TAppUserOrganization
            {
                AppUserId = testUser.Id,
                OrganizationId = organizationId,
                Role = RolesEnum.EnterpriseAdmin
            }, cancellationToken);

            return Results.Ok(new UserModel { Id = testUser.Id, UserName = testUser.UserName, Email = testUser.Email });
        }).AllowAnonymous();
        #endregion
    }
}
