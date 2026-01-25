
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
        v1.MapGet("/api/department/{id:int}", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken ct, int id) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (!userRoles.Any(c => c.Value == RolesEnum.EnterpriseAdmin.ToString()))
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User does not have permission to retrieve departments"] });
                }

                var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return Results.Unauthorized();
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
