
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
                try
                {
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var hasAccess = await UserRolesEndpoints.IsUserAuthorizedForDepartmentAsync(user, payload.DepartmentId, rolesToCheck, db, ct);
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
            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });
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
                try
                {
                    var _task = await db.GetRowAsync<TTask>(id, ct);
                    if (_task == null)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Task not found"] });
                    }
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var hasAccess = await UserRolesEndpoints.IsUserAuthorizedForDepartmentAsync(user, _task.DepartmentId, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                    await db.DeleteRowAsync<TTask>(new TTask { Id = id, CreatorUserId = "", DueDateUtc = DateTime.UtcNow, Name = "" }, ct);
                    return Results.Ok(new FormResult { Succeeded = true });
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }
            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });
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
                try
                {
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var hasAccess = await UserRolesEndpoints.IsUserAuthorizedForDepartmentAsync(user, departmentId, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                    var tasks = await db.GetTasksByDepartmentAsync(departmentId, ct);
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
                try
                {
                    var _task = await db.GetRowAsync<TTask>(id, ct);
                    if (_task == null)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Task not found"] });
                    }
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var hasAccess = await UserRolesEndpoints.IsUserAuthorizedForDepartmentAsync(user, _task.DepartmentId, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                    return Results.Ok(_task);
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

        /// <summary>
        /// Add or update a TTaskDepartment association.
        /// </summary>
        /// <param name="departmentTask">The TTaskDepartment association to add or update.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The added or updated TTaskDepartment association with a 200 OK status, or 400 Bad Request if unauthorized.</returns>
        v1.MapPost("/api/TaskDepartment", async Task<IResult> (TTaskDepartment departmentTask, ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var hasAccess = await UserRolesEndpoints.IsUserAuthorizedForDepartmentAsync(user, departmentTask.DepartmentId, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                    var updated = await db.AddUpdateRowAsync(departmentTask, ct);
                    return updated is null ? Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Not found or added"] }) : Results.Ok(updated);
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }
            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });
        }).Accepts<TTaskDepartment>("application/json")
        .Produces<TTaskDepartment>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        /// <summary>
        /// Delete a TTaskDepartment association.
        /// </summary>
        /// <param name="id">The ID of the TTaskDepartment association to delete.</param>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A 200 OK status if deletion is successful, or 400 Bad Request if unauthorized.</returns>
        v1.MapDelete("/api/TaskDepartment/{id}", async Task<IResult> (int id, ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    var taskDepartment = await db.GetRowAsync<TTaskDepartment>(id, ct);
                    if (taskDepartment == null)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["TaskDepartment association not found"] });
                    }
                    var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                    var hasAccess = await UserRolesEndpoints.IsUserAuthorizedForDepartmentAsync(user, taskDepartment.DepartmentId, rolesToCheck, db, ct);
                    if (!hasAccess)
                    {
                        return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                    }
                    await db.DeleteRowAsync<TTaskDepartment>(new TTaskDepartment { Id = id, DepartmentId = taskDepartment.DepartmentId, TaskId = taskDepartment.TaskId }, ct);
                    return Results.Ok(new FormResult { Succeeded = true });
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
                }
            }
            return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User not authenticated"] });
        }).Accepts<TTaskDepartment>("application/json")
        .Produces<TTaskDepartment>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }
}