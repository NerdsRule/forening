namespace Organization.ApiService.V1;

/// <summary>
/// Endpoints for managing user budget entries.
/// </summary>
public static class UserBudgetEndpoint
{
    /// <summary>
    /// Maps the user budget endpoints to the application.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    public static void MapUserBudgetEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        /// <summary>
        /// Returns the current user's budget entries for the specified department.
        /// </summary>
        v1.MapGet("/api/UserBudget/My/{departmentId}", async Task<IResult> (ClaimsPrincipal user, int departmentId, IRootDbReadWrite db, CancellationToken ct) =>
        {
            var userId = UserRolesHelpers.GetAuthenticatedUserId(user);
            if (userId is null)
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });

            if (!await UserRolesHelpers.CanAccessUserBudgetAsync(user, userId, departmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });

            var budgets = await db.GetUserBudgetsAsync(userId, departmentId, ct);
            return Results.Ok(budgets);
        })
        .Produces<List<TUserBudget>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Returns the budget entries for a specific user in the specified department.
        /// </summary>
        v1.MapGet("/api/UserBudget/User/{departmentId}/{userId}", async Task<IResult> (ClaimsPrincipal user, int departmentId, string userId, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (!await UserRolesHelpers.IsBudgetAdministratorAsync(user, departmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });

            if (!await UserRolesHelpers.IsUserInDepartmentAsync(userId, departmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User is not in department"] });

            var budgets = await db.GetUserBudgetsAsync(userId, departmentId, ct);
            return Results.Ok(budgets);
        })
        .Produces<List<TUserBudget>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Returns all budget entries for the specified department.
        /// </summary>
        v1.MapGet("/api/UserBudget/Department/{departmentId}", async Task<IResult> (ClaimsPrincipal user, int departmentId, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (!await UserRolesHelpers.IsBudgetAdministratorAsync(user, departmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });

            var budgets = await db.GetDepartmentBudgetsAsync(departmentId, ct);
            return Results.Ok(budgets);
        })
        .Produces<List<TUserBudget>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Creates or updates a budget entry for a department user.
        /// </summary>
        v1.MapPost("/api/UserBudget", async Task<IResult> (ClaimsPrincipal user, TUserBudget payload, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (payload is null)
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Payload is null"] });

            if (!await UserRolesHelpers.IsBudgetAdministratorAsync(user, payload.DepartmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });

            if (!await UserRolesHelpers.IsUserInDepartmentAsync(payload.AppUserId, payload.DepartmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User is not in department"] });

            payload.Description = payload.Description.Trim();
            payload.Tags = NormalizeTags(payload.Tags);

            if (string.IsNullOrWhiteSpace(payload.Description))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Description is required"] });

            var updated = await db.AddUpdateRowAsync(payload, ct);
            return Results.Ok(updated);
        })
        .Accepts<TUserBudget>("application/json")
        .Produces<TUserBudget>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Deletes a budget entry by identifier.
        /// </summary>
        v1.MapDelete("/api/UserBudget/{id}", async Task<IResult> (ClaimsPrincipal user, int id, IRootDbReadWrite db, CancellationToken ct) =>
        {
            var budget = await db.GetRowAsync<TUserBudget>(id, ct);
            if (budget is null)
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Budget entry not found"] });

            if (!await UserRolesHelpers.IsBudgetAdministratorAsync(user, budget.DepartmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });

            await db.DeleteRowAsync(new TUserBudget { Id = id, AppUserId = budget.AppUserId, DepartmentId = budget.DepartmentId, Description = budget.Description }, ct);
            return Results.Ok(new FormResult { Succeeded = true });
        })
        .Produces<FormResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Returns distinct budget tags for the specified department.
        /// </summary>
        v1.MapGet("/api/UserBudget/Tags/ByDepartment/{departmentId}", async Task<IResult> (ClaimsPrincipal user, int departmentId, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (!await UserRolesHelpers.IsBudgetAdministratorAsync(user, departmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });

            var tags = await db.GetDistinctBudgetTagsByDepartmentAsync(departmentId, ct);
            return Results.Ok(tags);
        })
        .Produces<List<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Returns budget description suggestions for the specified department.
        /// </summary>
        v1.MapGet("/api/UserBudget/Descriptions/ByDepartment/{departmentId}", async Task<IResult> (ClaimsPrincipal user, int departmentId, string? query, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (!await UserRolesHelpers.IsBudgetAdministratorAsync(user, departmentId, db, ct))
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });

            var descriptions = await db.GetBudgetDescriptionSuggestionsAsync(departmentId, query ?? string.Empty, ct);
            return Results.Ok(descriptions);
        })
        .Produces<List<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }

    /// <summary>
    /// Normalizes optional text values by trimming whitespace and returning <c>null</c> for blank content.
    /// </summary>
    /// <param name="value">The input text.</param>
    /// <returns>The trimmed value, or <c>null</c> when empty.</returns>
    private static List<string> NormalizeTags(List<string>? tags)
    {
        if (tags is null || tags.Count == 0)
            return [];

        return tags
            .Select(tag => tag?.Trim().ToLowerInvariant())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()!;
    }
}