
namespace Organization.ApiService.V1;

/// <summary>
/// Endpoints for managing tasks.
/// </summary>
public static class TaskEndpoint
{
    /// <summary>
    /// Maps task-related HTTP endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to which the endpoints will be mapped.</param>
    public static void MapTaskEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        /// <summary>
        /// Creates or updates a task.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="payload">The task data to create or update.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created or updated task with a 200 OK status, or 400 Bad Request if the payload is invalid.</returns>
        v1.MapPost("/api/Task", async Task<IResult> (ClaimsPrincipal user, TTask payload, IRootDbReadWrite db, CancellationToken ct) =>
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
        .Accepts<TTask>("application/json")
        .Produces<TTask>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="id">The ID of the task to delete.</param>
        v1.MapDelete("/api/Task/{id}", async Task<IResult> (ClaimsPrincipal user, int id, IRootDbReadWrite db, CancellationToken ct) =>
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
                await db.DeleteRowAsync<TTask>(new TTask { Id = id, CreatorUserId = "", DueDateUtc = DateTime.UtcNow, Name = "" }, ct);
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
        /// Retrieves all tasks for a specific department.
        /// </summary>
        /// <param name="departmentId">The ID of the department whose tasks are to be retrieved.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of tasks for the specified department with a 200 OK status, or 400 Bad Request if unauthorized.</returns>
        v1.MapGet("/api/Task/ByDepartment/{departmentId}", async Task<IResult> (int departmentId, ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken ct) =>
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
                    var tasks = (await db.GetRowsAsync<TTask>(ct)).Where(t => t.DepartmentId == departmentId).ToList();
                    return Results.Ok(tasks);
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
        /// Get a task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The task with the specified ID and a 200 OK status, or 400 Bad Request if unauthorized.</returns>
        v1.MapGet("/api/Task/{id}", async Task<IResult> (int id, ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken ct) =>
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
                    var task = (await db.GetRowsAsync<TTask>(ct)).FirstOrDefault(t => t.Id == id);
                    if (task == null)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Task not found"] });
                    }

                    var userDepartmentAccess = (await db.GetRowsAsync<TAppUserDepartment>(ct)).Where(a => a.AppUserId == userId && a.DepartmentId == task.DepartmentId);
                    
                    if (!userDepartmentAccess.Any())
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Access denied to department"] });
                    }
                    return Results.Ok(task);
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
        .Produces<TTask>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }
}