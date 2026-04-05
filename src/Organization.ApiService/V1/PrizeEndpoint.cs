
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
                try
                {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                var hasAccess = await UserRolesHelpers.IsUserAuthorizedForDepartmentAsync(user, payload.DepartmentId, rolesToCheck, db, ct);
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
                try
                {
                    var _price = await db.GetRowAsync<TPrize>(id, ct);
                    if (_price is null)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Price not found"] });
                    }
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var hasAccess = await UserRolesHelpers.IsUserAuthorizedForDepartmentAsync(user, _price.DepartmentId, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                    await db.DeleteRowAsync<TPrize>(new TPrize { Id = id, CreatorUserId = "", Name = "" }, ct);
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
                try
                {
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin, RolesEnum.DepartmentMember };
                    var hasAccess = await UserRolesHelpers.IsUserAuthorizedForDepartmentAsync(user, departmentId, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                    var prizes = await db.GetPrizesByDepartmentAsync(departmentId, ct);
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
                try
                {
                    var _prize = await db.GetPrizeByIdAsync(id, ct);
                    if (_prize is null)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Price not found"] });
                    }
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin, RolesEnum.DepartmentMember };
                    var hasAccess = await UserRolesHelpers.IsUserAuthorizedForDepartmentAsync(user, _prize.DepartmentId, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                    return Results.Ok(_prize);
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }
            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });
        })
        .Produces<TPrize>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Get points balance for a specific user.
        /// Points balance = total awarded points - total redeemed prize points.
        /// </summary>
        /// <param name="userId">The user id to calculate balance for.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="userManager">User manager used for access checks.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Points totals and balance for the user.</returns>
        v1.MapGet("/api/price/PointsBalance/ByUser/{userId}", async Task<IResult> (string userId, ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });
            }

            try
            {
                var authenticatedUserId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var isSelf = string.Equals(authenticatedUserId, userId, StringComparison.Ordinal);

                if (!isSelf)
                {
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.OrganizationMember };
                    var (hasAccess, _) = await UserRolesHelpers.IsUserInSameOrganizationAndInRoleAsync(user, userId, rolesToCheck, userManager, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                }

                var totalPointsAwarded = await db.GetTasksWithPointsAwardedByUserAsync(userId, ct);
                var totalPointsRedeemed = await db.GetPrizesByAssignedUserIdAsync(userId, ct);
                var awarded = totalPointsAwarded.Sum(t => t.TaskPointsAwarded);
                var redeemed = totalPointsRedeemed.Where(p => p.Status == Shared.PrizeStatusEnum.Redeemed).Sum(p => p.PointsCost);
                var userInfo = await UserRolesHelpers.GetUserInfoAsync(userId, userManager, db, ct);
                if (userInfo is null)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not found"] });
                }

                var points = new UserPointsBalanceModel
                {
                    UserId = userInfo.Id,
                    DisplayName = userInfo.DisplayName,
                    UserName = userInfo.UserName,
                    Email = userInfo.Email,
                    TotalPointsAwarded = awarded,
                    TotalPointsRedeemed = redeemed,
                    PointsBalance = awarded - redeemed
                };

                return Results.Ok(points);
            }
            catch (Exception e)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
            }
        })
        .Produces<UserPointsBalanceModel>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }
}
