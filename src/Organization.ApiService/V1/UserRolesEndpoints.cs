

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
            var userId = UserRolesHelpers.GetAuthenticatedUserId(user);
            if (userId is null) 
                return Results.Unauthorized();

            try
            {
                var userInfo = await UserRolesHelpers.GetUserInfoAsync(userId, userManager, db, cancellationToken);
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

            var appUser = await userManager.FindByEmailAsync(model.Email.Trim());
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
            var userInfo = await UserRolesHelpers.GetUserInfoAsync(appUser.Id, userManager, db, cancellationToken);
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
            if (!await UserRolesHelpers.IsUserAuthorizedForOrganizationAsync(user, model.OrganizationId, 0, [RolesEnum.DepartmentAdmin, RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin], db, cancellationToken))
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
            bool hasAccess = await UserRolesHelpers.IsUserAuthorizedForOrganizationAsync(user, organizationId, departmentId, [RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin], rootDbReadWrite, cancellationToken);

            if (hasAccess)
            {
                var users = userManager.Users.Select(u => new UserModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    DisplayName = u.DisplayName ?? u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    EmailConfirmed = u.EmailConfirmed,
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
                var (hasAccess, _user) = await UserRolesHelpers.IsUserInSameOrganizationAndInRoleAsync(user, userId, rolesToCheck, userManager, db, cancellationToken);
                if (hasAccess)
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
                var (hasAccess, _user) = await UserRolesHelpers.IsUserInSameOrganizationAndInRoleAsync(user, userId, rolesToCheck, userManager, db, cancellationToken);
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
                var (hasAccess, _user) = await UserRolesHelpers.IsUserInSameOrganizationAndInRoleAsync(user, model.Id, rolesToCheck, userManager, db, cancellationToken);
                if (hasAccess)
                {
                    var appUser = await userManager.FindByIdAsync(model.Id);
                    if (appUser is not null)
                    {
                        appUser.UserName = model.UserName;
                        appUser.Email = model.Email;
                        appUser.EmailConfirmed = model.EmailConfirmed;
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
                if (await UserRolesHelpers.IsUserAuthorizedForOrganizationAsync(user, organizationId, 0, rolesToCheck, db, cancellationToken))
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
                        DisplayName = u.DisplayName ?? u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        EmailConfirmed = u.EmailConfirmed,
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
                if (await UserRolesHelpers.IsUserAuthorizedForDepartmentAsync(user, departmentId, rolesToCheck, db, cancellationToken))
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
                        DisplayName = u.DisplayName ?? u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        EmailConfirmed = u.EmailConfirmed,
                        MemberNumber = u.MemberNumber
                    }).ToList();
                    return Results.Ok(userModels);
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

            return Results.Ok(new UserModel { Id = testUser.Id, UserName = testUser.UserName ?? string.Empty, Email = testUser.Email ?? string.Empty, EmailConfirmed = testUser.EmailConfirmed });
        }).AllowAnonymous();
        #endregion
    }
}
