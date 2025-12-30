
namespace Organization.ApiService.V1;

/// <summary>
/// Contains endpoint mappings related to application user and organization relationships.
/// </summary>
/// <remarks>
/// This static class is intended to group HTTP endpoint definitions that manage
/// the associations between application users and organizations. Use this class to declare
/// routes, handlers, and request/response wiring for operations such as linking users to organizations,
/// retrieving user-organization relationships, and managing roles within organizations.
/// Endpoint registration is typically done once during application startup and delegates business logic
/// to the service or application layer to keep the endpoint code thin and focused on HTTP concerns.
/// </remarks>
public static class AppUserOrganizationEndpoints
{
    /// <summary>
    /// Maps application user-organization related HTTP endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to which endpoints will be mapped.</param>
    /// <remarks>
    /// This method defines routes, HTTP methods, request/response types, and handler logic for
    /// managing relationships between application users and organizations. It is typically called once
    /// during application startup to register the endpoints with the ASP.NET Core routing system.
    /// </remarks>
    public static void MapAppUserOrganizationEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        /// <summary>
        /// Updates or adds an existing TAppUserOrganization.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="payload">The TAppUserOrganization data to update.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated TAppUserOrganization with a 200 OK status, or 400 Bad Request if the payload is invalid.</returns>
        v1.MapPost("/api/AppUserOrganization", async Task<IResult> (ClaimsPrincipal user, TAppUserOrganization payload, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (payload is null)
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Payload is null"] });
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (!userRoles.Any(c => c.Value == RolesEnum.EnterpriseAdmin.ToString()))
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                }
            }
            try
            {
                var updated = await db.AddUpdateRowAsync(payload, ct);
                return updated is null ? Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Not found"] }) : Results.Ok(updated);
            }
            catch (Exception e)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
            }
        })
        .Accepts<TAppUserOrganization>("application/json")
        .Produces<TAppUserOrganization>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        /// <summary>
        /// Deletes a TAppUserOrganization by its ID.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="id">The ID of the TAppUserOrganization to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A 200 OK status if deletion is successful, or 404 Not Found if the entity does not exist.</returns>
        v1.MapDelete("/api/AppUserOrganization/{id}", async Task<IResult> (ClaimsPrincipal user, int id, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var userRoles = identity.FindAll(identity.RoleClaimType);

                if (!userRoles.Any(c => c.Value == RolesEnum.EnterpriseAdmin.ToString()))
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Forbidden"] });
                }
            }
            try
            {
                await db.DeleteRowAsync(new TAppUserOrganization { Id = id }, ct);
                return Results.Ok();
            }
            catch (Exception e)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = [e.Message] });
            }
        })
        .RequireAuthorization();
    }
}
