
using Microsoft.AspNetCore.Authentication.Cookies;

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
    public static void MapUserRolesEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        #region Endpoints for user management
        v1.MapPost("/api/users/login", async Task<IResult> (SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, [FromBody] LoginModel model) =>
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

            return Results.Ok(new { appUser.Id, appUser.UserName, appUser.Email });
        }).AllowAnonymous();

        /// <summary>
        /// Registration endpoint
        /// </summary>
        /// <param name="userManager">UserManager</param>
        /// <param name="roleManager">RoleManager</param>
        /// <param name="model">RegisterModel</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/register", async Task<IResult> (UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IRootDbReadWrite db, CancellationToken cancellationToken, [FromBody] RegisterModel model) =>
        {
            if (model is null)
            {
                return Results.BadRequest(new { Error = "Request body is required." });
            }

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Results.BadRequest(new { Error = "Email and password are required." });
            }

            var existing = await userManager.FindByEmailAsync(model.Email);
            if (existing is not null)
            {
                return Results.Conflict(new { Error = "Email already in use." });
            }

            var user = new AppUser
            {
                UserName = string.IsNullOrWhiteSpace(model.UserName) ? model.Email : model.UserName,
                Email = model.Email
            };

            var createResult = await userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                return Results.BadRequest(new { Errors = createResult.Errors.Select(e => e.Description) });
            }

            // Add default role "User" if it exists
            if (await roleManager.RoleExistsAsync(OrganizationRolesEnum.Member.ToString()))
            {
                await userManager.AddToRoleAsync(user, OrganizationRolesEnum.Member.ToString());
            }

            var org = await db.AddRowAsync<TAppUserOrganization>(new TAppUserOrganization
            {
                AppUserId = user.Id,
                OrganizationId = model.OrganizationId,
                Role = OrganizationRolesEnum.Member
            }, cancellationToken);

            return Results.Ok(new UserModel { Id = user.Id, UserName = user.UserName });
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
        /// Get roles for the authenticated user
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <returns>Array of roles</returns>
        v1.MapGet("/api/roles", (ClaimsPrincipal user) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var roles = identity.FindAll(identity.RoleClaimType)
                    .Select(c =>
                        new
                        {
                            c.Issuer,
                            c.OriginalIssuer,
                            c.Type,
                            c.Value,
                            c.ValueType
                        });

                return TypedResults.Json(roles);
            }

            return Results.Unauthorized();
        }).RequireAuthorization();

        /// <summary>
        /// Remove roles from a user if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="roleManager">RoleManager</param>
        /// <param name="userId">User Id</param>
        /// <param name="roles">Roles to remove</param>
        /// <returns>Result</returns>
        v1.MapDelete("/api/users/{userId}/roles", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, string userId, [FromBody] string[] roles) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                {
                    // Only administrators can remove the administrator role
                    if (roles.Contains(OrganizationRolesEnum.Admin.ToString()) && !userRoles.Any(c => c.Value == OrganizationRolesEnum.Admin.ToString()))
                    {
                        return Results.Forbid();
                    }
                    var appUser = await userManager.FindByIdAsync(userId);
                    if (appUser is not null)
                    {
                        await userManager.RemoveFromRolesAsync(appUser, roles);

                        return Results.Ok();
                    }
                }
            }
            return Results.NotFound();
        }).RequireAuthorization();

        /// <summary>
        /// Get all roles
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="roleManager">RoleManager</param>
        /// <returns>Array of roles</returns>
        v1.MapGet("/api/roles/all", (ClaimsPrincipal user, RoleManager<IdentityRole> roleManager) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                {
                    var roles = roleManager.Roles.Select(r => r.Name);

                    return TypedResults.Json(roles);
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Add a role if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="roleManager">RoleManager</param>
        /// <param name="role">Role to add</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/roles", async Task<IResult> (ClaimsPrincipal user, RoleManager<IdentityRole> roleManager, [FromBody] string[] role) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (!userRoles.Any())
                {
                    // If first time setup, create default roles
                    if (!await roleManager.RoleExistsAsync(OrganizationRolesEnum.Member.ToString()))
                    {
                        foreach (var r in Enum.GetNames(typeof(OrganizationRolesEnum)))
                        {
                            var result = await roleManager.CreateAsync(new IdentityRole(r));
                            if (!result.Succeeded)
                            {
                                return Results.BadRequest(result.Errors);
                            }
                        }
                        return Results.Ok();
                    }
                }

                if (userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                {
                    foreach (var r in role)
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(r));
                        if (!result.Succeeded)
                        {
                            return Results.BadRequest(result.Errors);
                        }
                    }
                    return Results.Ok();
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Delete a role if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="roleManager">RoleManager</param>
        /// <param name="role">Role to delete</param>
        /// <returns>Result</returns>
        v1.MapDelete("/api/roles/{role}", async Task<IResult> (ClaimsPrincipal user, RoleManager<IdentityRole> roleManager, string role) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                {
                    var roleToDelete = await roleManager.FindByNameAsync(role);
                    if (roleToDelete is not null)
                    {
                        await roleManager.DeleteAsync(roleToDelete);

                        return Results.Ok();
                    }
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Get roles for a user if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="userId">User Id</param>
        /// <returns>Array of roles</returns>
        v1.MapGet("/api/users/{userId}/roles", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, string userId) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                {
                    var appUser = await userManager.FindByIdAsync(userId);
                    if (appUser is not null)
                    {
                        var roles = await userManager.GetRolesAsync(appUser);

                        return TypedResults.Json(roles);
                    }
                }
            }
            return Results.NotFound();
        }).RequireAuthorization();

        /// <summary>
        /// Add roles to a user if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="roleManager">RoleManager</param>
        /// <param name="userId">User Id</param>
        /// <param name="roles">Roles to add</param>
        /// <returns>Result</returns>
        v1.MapPost("/api/users/{userId}/roles", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, string userId, [FromBody] string[] roles) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                // Add roles to first time setup
                if (!userRoles.Any())
                {
                    // If only one user exists, make the user an EnterpriseAdmin
                    if (userManager.Users.Count() == 1 && roles.Contains(OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                    {
                        var appUser = await userManager.FindByIdAsync(userId);
                        if (appUser is not null)
                        {
                            await userManager.AddToRolesAsync(appUser, roles);
                            return Results.Ok();
                        }
                    }
                }

                if (userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                {
                    // Only administrators can add the administrator role
                    if (roles.Contains(OrganizationRolesEnum.Admin.ToString()) && !userRoles.Any(c => c.Value == OrganizationRolesEnum.Admin.ToString())
                       || roles.Contains(OrganizationRolesEnum.EnterpriseAdmin.ToString()) && !userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                    {
                        return Results.Forbid();
                    }
                    // find user by email and add roles
                    var appUser = await userManager.FindByIdAsync(userId);
                    if (appUser is not null)
                    {
                        await userManager.AddToRolesAsync(appUser, roles);
                    }
                    else
                    {
                        return Results.NotFound();
                    }
                    return Results.Ok();
                }
                return Results.Forbid();
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Get all users if authenticated user is an administrator
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="userManager">UserManager</param>
        /// <returns>List of users</returns>
        v1.MapGet("/api/users", (ClaimsPrincipal user, UserManager<AppUser> userManager) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                {
                    var users = userManager.Users.Select(u => new { u.Id, u.UserName });

                    return TypedResults.Json(users);
                }
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
        v1.MapDelete("/api/users/{userId}", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, string userId) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (userRoles.Any(c => c.Value == OrganizationRolesEnum.EnterpriseAdmin.ToString()))
                {
                    var appUser = await userManager.FindByIdAsync(userId);
                    if (appUser is not null && appUser.NormalizedUserName != "LASSE.TARP@SPACE4IT.DK")
                    {
                        await userManager.DeleteAsync(appUser);
                        return Results.Ok();
                    }
                    else
                    {
                        return Results.NotFound();
                    }
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

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
        #endregion

    }
}
