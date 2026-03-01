
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
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin, RolesEnum.OrganizationMember };
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
        .Produces<FormResult>(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        /// <summary>
        /// Creates or updates a department.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="payload">The department to create or update.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated department with 200 OK, or an error form result.</returns>
        v1.MapPost("/api/department", async Task<IResult> (ClaimsPrincipal user, TDepartment payload, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (payload is null)
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Payload is null"] });

            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var organizationIdForAccess = payload.OrganizationId;
                    var departmentIdForAccess = payload.Id;

                    if (payload.Id > 0)
                    {
                        var existingDepartment = await db.GetRowAsync<TDepartment>(payload.Id, ct);
                        if (existingDepartment is null)
                        {
                            return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Department not found"] });
                        }

                        organizationIdForAccess = existingDepartment.OrganizationId;
                        departmentIdForAccess = existingDepartment.Id;
                    }

                    var hasAccess = await UserRolesEndpoints.IsUserAuthorizedForOrganizationAsync(user, organizationIdForAccess, departmentIdForAccess, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }

                    var updated = await db.AddUpdateRowAsync(payload, ct);
                    return updated is null ? Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Not found or added"] }) : Results.Ok(updated);
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }

            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Unauthorized"] });
        })
        .Accepts<TDepartment>("application/json")
        .Produces<TDepartment>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        /// <summary>
        /// Deletes a department by its ID.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="id">The ID of the department to delete.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success form result with 200 OK, or an error form result.</returns>
        v1.MapDelete("/api/department/{id:int}", async Task<IResult> (ClaimsPrincipal user, int id, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    var department = await db.GetRowAsync<TDepartment>(id, ct);
                    if (department is null)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Department not found"] });
                    }

                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var hasAccess = await UserRolesEndpoints.IsUserAuthorizedForOrganizationAsync(user, department.OrganizationId, department.Id, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }

                    await db.DeleteRowAsync(department, ct);
                    return Results.Ok(new FormResult { Succeeded = true });
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }

            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Unauthorized"] });
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }
}
