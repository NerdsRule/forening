
namespace Organization.ApiService.V1;

/// <summary>
/// Contains endpoint mappings related to application user and department relationships.
/// </summary>
/// <remarks>
/// This static class is intended to group HTTP endpoint definitions that manage
/// the associations between application users and departments. Use this class to declare
/// routes, handlers, and request/response wiring for operations such as linking users to departments,
/// retrieving user-department relationships, and managing roles within departments.
/// Endpoint registration is typically done once during application startup and delegates business logic
/// to the service or application layer to keep the endpoint code thin and focused on HTTP concerns.
/// </remarks>
public static class AppUserDepartmentEndpoints
{
    /// <summary>
    /// Maps application user-department related HTTP endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to which endpoints will be mapped.</param>
    /// <remarks>
    /// This method defines routes, HTTP methods, request/response types, and handler logic for
    /// managing relationships between application users and departments. It is typically called once
    /// during application startup to register the endpoints with the ASP.NET Core routing system.
    /// </remarks>
    public static void MapAppUserDepartmentEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        /// <summary>
        /// Updates or adds an existing TAppUserDepartment.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="payload">The TAppUserDepartment data to update.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated TAppUserDepartment with a 200 OK status, or 400 Bad Request if the payload is invalid.</returns>
        v1.MapPost("/api/AppUserDepartment", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, TAppUserDepartment payload, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (payload is null)
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Payload is null"] });
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                var (hasAccess, _user) = await UserRolesEndpoints.IsUserInSameOrganizationAndInRoleAsync(user, payload.AppUserId, rolesToCheck, userManager, db, ct);
                if (!hasAccess)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                }
                var updated = await db.AddUpdateRowAsync(payload, ct);
                return updated is null ? Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Not found"] }) : Results.Ok(updated);
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }

            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
        })
        .Accepts<TAppUserDepartment>("application/json")
        .Produces<TAppUserDepartment>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        /// <summary>
        /// Deletes a TAppUserDepartment by its ID.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="id">The ID of the TAppUserDepartment to delete.</param>
        v1.MapDelete("/api/AppUserDepartment/{userId}/{id}", async Task<IResult> (ClaimsPrincipal user, string userId, int id, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                var (hasAccess, _user) = await UserRolesEndpoints.IsUserInSameOrganizationAndInRoleAsync(user, userId, rolesToCheck, userManager, db, ct);
                if (!hasAccess)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                }
                await db.DeleteRowAsync(new TAppUserDepartment { Id = id }, ct);
                return Results.Ok(new FormResult { Succeeded = true });
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }
            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
        })
        .RequireAuthorization();
    }
}