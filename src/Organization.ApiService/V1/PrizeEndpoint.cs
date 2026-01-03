
namespace Organization.ApiService.V1;

public static class PrizeEndpoint
{
/// <summary>
    /// Maps task-related HTTP endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to which the endpoints will be mapped.</param>
    public static void MapPrizeEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        /// <summary>
        /// Creates or updates a price.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="payload">The price data to create or update.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created or updated price with a 200 OK status, or 400 Bad Request if the payload is invalid.</returns>
        v1.MapPost("/api/Price", async Task<IResult> (ClaimsPrincipal user, TPrize payload, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (payload is null)
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Payload is null"] });
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (!userRoles.Any(c => c.Value == RolesEnum.EnterpriseAdmin.ToString() || c.Value == RolesEnum.DepartmentAdmin.ToString()))
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                }
            }
            try
            {
                var updated = await db.AddUpdateRowAsync(payload, ct);
                return updated is null ? Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Not found or added"] }) : Results.Ok(updated);
            }
            catch (Exception e)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
            }
        })
        .Accepts<TPrize>("application/json")
        .Produces<TPrize>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        /// <summary>
        /// Deletes a price by its ID.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="id">The ID of the price to delete.</param>
        v1.MapDelete("/api/price/{id}", async Task<IResult> (ClaimsPrincipal user, int id, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);
                if (!userRoles.Any(c => c.Value == RolesEnum.EnterpriseAdmin.ToString() || c.Value == RolesEnum.DepartmentAdmin.ToString()))
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                }
            }
            try
            {
                await db.DeleteRowAsync<TPrize>(new TPrize { Id = id, CreatorUserId = "", Name = "" }, ct);
                return Results.Ok(new FormResult { Succeeded = true });
            }
            catch (Exception e)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
            }
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Retrieves all prices for a specific department.
        /// </summary>
        /// <param name="departmentId">The ID of the department whose prices are to be retrieved.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of prices for the specified department with a 200 OK status, or 400 Bad Request if unauthorized.</returns>
        v1.MapGet("/api/price/ByDepartment/{departmentId}", async Task<IResult> (int departmentId, ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);
                if (!userRoles.Any(c => c.Value == RolesEnum.EnterpriseAdmin.ToString() || c.Value == RolesEnum.DepartmentAdmin.ToString()))
                {
                    // Check if user has access to the specific department
                    var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User ID not found"] });
                    }

                    var userDepartmentAccess = (await db.GetRowsAsync<TAppUserDepartment>( ct)).Where(a => a.AppUserId == userId && a.DepartmentId == departmentId);
                    
                    if (!userDepartmentAccess.Any())
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Access denied to department"] });
                    }
                }

                try
                {
                    var prizes = (await db.GetRowsAsync<TPrize>(ct)).Where(t => t.DepartmentId == departmentId).ToList();
                    return Results.Ok(prizes);
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }
            else
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });
            }
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Get a price by its ID.
        /// </summary>
        /// <param name="id">The ID of the price to retrieve.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The price with the specified ID and a 200 OK status, or 400 Bad Request if unauthorized.</returns>
        v1.MapGet("/api/price/{id}", async Task<IResult> (int id, ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);
                // Check if user has access to the specific department where the task is located
                var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User ID not found"] });
                }

                try
                {
                    var prize = (await db.GetRowsAsync<TPrize>(ct)).FirstOrDefault(t => t.Id == id);
                    if (prize == null)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Prize not found"] });
                    }

                    var userDepartmentAccess = (await db.GetRowsAsync<TAppUserDepartment>(ct)).Where(a => a.AppUserId == userId && a.DepartmentId == prize.DepartmentId);
                    
                    if (!userDepartmentAccess.Any())
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Access denied to department"] });
                    }
                    return Results.Ok(prize);
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }
            else
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });
            }
        })
        .Produces<TPrize>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }
}
