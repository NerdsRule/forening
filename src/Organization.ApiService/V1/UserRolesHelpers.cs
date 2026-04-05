namespace Organization.ApiService.V1;

public static class UserRolesHelpers
{
    /// <summary>
    /// Check if user is authorized and is RolesEnum.EnterpriseAdmin in any organization.
    /// </summary>
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
    public static async Task<bool> IsUserAuthorizedForOrganizationAsync(ClaimsPrincipal user, int organizationId, int departmentId, RolesEnum[] roles, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        if (user.Identity is not null && user.Identity.IsAuthenticated && organizationId > 0)
        {
            var userId = GetAuthenticatedUserId(user);
            if (userId is null) return false;

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
    public static async Task<bool> IsUserAuthorizedForDepartmentAsync(ClaimsPrincipal user, int departmentId, RolesEnum[] roles, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        if (user.Identity is not null && user.Identity.IsAuthenticated && departmentId > 0)
        {
            var userId = GetAuthenticatedUserId(user);
            if (userId is null) return false;

            var userDepRoles = await db.GetUserDepartmentsAsync(userId, cancellationToken);
            return userDepRoles.Any(c => c.DepartmentId == departmentId && roles.Contains(c.Role));
        }

        return false;
    }

    /// <summary>
    /// Checks if the user is in the same organization and has one of the specified roles.
    /// </summary>
    public static async Task<(bool hasAccess, UserModel? user)> IsUserInSameOrganizationAndInRoleAsync(ClaimsPrincipal user, string userId, RolesEnum[] rolesToCheck, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        var loggedInUserId = GetAuthenticatedUserId(user);
        if (loggedInUserId is null)
        {
            return (false, null);
        }

        var loggedInUser = await GetUserInfoAsync(loggedInUserId, userManager, db, cancellationToken);
        var requestedUser = await GetUserInfoAsync(userId, userManager, db, cancellationToken);
        if (loggedInUser is not null && requestedUser is not null)
        {
            var hasAccess = loggedInUser.AppUserOrganizations.Any(o =>
                requestedUser.AppUserOrganizations.Any(uo => uo.OrganizationId == o.OrganizationId) &&
                rolesToCheck.Contains(o.Role));
            return (hasAccess, requestedUser);
        }

        return (false, null);
    }

    /// <summary>
    /// Get user information.
    /// </summary>
    public static async Task<UserModel?> GetUserInfoAsync(string userId, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken)
    {
        var appUser = await userManager.FindByIdAsync(userId);
        if (appUser is null) return null;

        var userAppUserOrgs = await db.GetUserOrganizationsAsync(userId, cancellationToken);
        var userAppUserDeps = await db.GetUserDepartmentsAsync(userId, cancellationToken);
        var totalPointsAwarded = await db.GetTasksWithPointsAwardedByUserAsync(userId, cancellationToken);
        var totalPointsRedeemed = await db.GetPrizesByAssignedUserIdAsync(userId, cancellationToken);

        return new UserModel
        {
            Id = appUser.Id,
            UserName = appUser.UserName ?? string.Empty,
            Email = appUser.Email ?? string.Empty,
            EmailConfirmed = appUser.EmailConfirmed,
            MemberNumber = appUser.MemberNumber,
            AppUserOrganizations = userAppUserOrgs,
            AppUserDepartments = userAppUserDeps,
            DisplayName = appUser.DisplayName ?? appUser.UserName ?? appUser.Email,
            TotalPointsAwarded = totalPointsAwarded.Sum(t => t.TaskPointsAwarded),
            TotalPointsRedeemed = totalPointsRedeemed.Where(p => p.Status == PrizeStatusEnum.Redeemed).Sum(p => p.PointsCost)
        };
    }

    /// <summary>
    /// Extracts the Fido2 service instance from the current HTTP context.
    /// </summary>
    public static Fido2 GetFido2(HttpContext httpContext)
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
    /// Extracts the authenticated user's ID from the provided claims principal.
    /// </summary>
    public static string? GetAuthenticatedUserId(ClaimsPrincipal user)
    {
        if (user.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return null;
        }

        return identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
