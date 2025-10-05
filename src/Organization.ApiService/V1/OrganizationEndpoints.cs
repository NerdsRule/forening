
using SQLitePCL;


/// <summary>
/// Defines and groups HTTP endpoints for organization-related operations exposed by the API service.
/// </summary>
/// <remarks>
/// Use this class to declare route mappings, handlers, and any request/response wiring for
/// organization resources (for example: create, update, delete, list, and retrieve organization details).
/// Endpoint registration is typically done once during application startup and delegates business logic
/// to the service or application layer to keep the endpoint code thin and focused on HTTP concerns.
/// </remarks>
/// <threadsafety>
/// Registration/configuration methods are not required to be thread-safe and should be executed during
/// application startup on a single thread. Handler methods invoked by the runtime should assume
/// concurrent execution and rely on injected services that are registered with appropriate lifetimes.
/// </threadsafety>
/// <example>
/// Example (conceptual):
/// <code>
/// // In Program.cs or Startup.cs
/// var endpoints = new OrganizationEndpoints();
/// endpoints.MapEndpoints(app);
/// </code>
/// </example>
namespace Organization.ApiService.V1;

public static class OrganizationEndpoints
{
    /// <summary>
    /// Maps organization-related HTTP endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to which endpoints will be mapped.</param>
    /// <remarks>
    /// This method defines routes, HTTP methods, request/response types, and handler logic for
    /// organization resources. It is typically called once during application startup to register
    /// the endpoints with the ASP.NET Core routing system.
    /// </remarks>
    /// <example>
    /// Example (conceptual):
    /// <code>
    /// var app = builder.Build();
    /// OrganizationEndpoints.MapOrganizationEndpoints(app);
    /// app.Run();
    /// </code>
    public static void MapOrganizationEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        v1.MapGet("/api/organization/all", async Task<IResult> (IRootDbReadWrite db) =>
        {
            var organizations = await db.GetRowsAsync<TOrganization>();
            if (organizations is not null) 
                return Results.Ok(organizations);
            return Results.NotFound();
        }).Produces<List<TOrganization>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        /// <summary>
        /// Maps organization-related HTTP endpoints to the provided <see cref="WebApplication"/> instance.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> instance to which endpoints will be mapped.</param>
        /// <remarks>
        v1.MapGet("/api/organization/{id:int}", async Task<IResult> (int id, IRootDbReadWrite db) =>
        {
            var organization = await db.GetRowAsync<TOrganization>(id);
            return organization is null ? Results.NotFound() : Results.Ok(organization);
        })
        .Produces<TOrganization>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        /// <summary>
        /// Creates a new organization.
        /// </summary>
        /// <param name="payload">The organization data to create.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created organization with a 201 Created status, or 400 Bad Request if the payload is invalid.</returns>
        v1.MapPut("/api/organization", async Task<IResult> (TOrganization payload, IRootDbReadWrite db, CancellationToken ct) =>
        {
            if (payload is null)
                return Results.BadRequest();

            try
            {
                var updated = await db.AddUpdateRowAsync(payload, ct);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (Exception e)
            {
                return Results.Problem(detail: e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Accepts<TOrganization>("application/json")
        .Produces<TOrganization>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        /// <summary>
        /// Deletes an organization by its ID.
        /// </summary>
        /// <param name="id">The ID of the organization to delete.</param>
        /// <param name="db">The database service for data access.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A 200 OK status if deletion is successful, or 404 Not Found
        /// if the organization does not exist.</returns>
        v1.MapDelete("/api/organization/{id:int}", async Task<IResult> (int id, IRootDbReadWrite db, CancellationToken ct) =>
        {
            try
            {
                var organization = await db.GetRowAsync<TOrganization>(id);
                if (organization is null)
                    return Results.NotFound();

                await db.DeleteRowAsync(organization, ct);
                return Results.Ok();
            }
            catch (Exception e)
            {
                return Results.Problem(detail: e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
