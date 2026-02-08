
namespace Organization.ApiService.V1;

/// <summary>
/// Department endpoints
/// </summary> 
public static class DepartmentEndpoint
{
    /// <summary>
    /// Maps department-related HTTP endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to which endpoints will be mapped.</param>
    public static void MapDepartmentEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        /// <summary>
        /// Retrieves all departments.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="id">The ID of the organization whose departments are to be retrieved.</param>
        /// <returns>A list of departments with a 200 OK status, or 403 Forbidden if the user lacks permissions.</returns>
        v1.MapGet("/api/department/{userId}/{id:int}", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken ct, string userId, int id) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                if (user.Identity is not null && user.Identity.IsAuthenticated)
                {
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var (hasAccess, _user) = await UserRolesEndpoints.IsUserInSameOrganizationAndInRoleAsync(user, userId, rolesToCheck, userManager, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                }
            var departments = await db.GetDepartmentsAsync(id, userId, ct);
            if (departments is not null)
                return Results.Ok(departments);
            }
            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Could not retrieve departments"] });
        }).Produces<List<TDepartment>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();
    }
}
