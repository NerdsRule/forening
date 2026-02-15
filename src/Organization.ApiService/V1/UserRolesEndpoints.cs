

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

        return new()
        {
            Id = appUser.Id,
            UserName = appUser.UserName ?? string.Empty,
            Email = appUser.Email ?? string.Empty,
            MemberNumber = appUser.MemberNumber,
            AppUserOrganizations = userAppUserOrgs,
            AppUserDepartments = userAppUserDeps
        };
    }
    #endregion

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
                Email = model.Email
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
