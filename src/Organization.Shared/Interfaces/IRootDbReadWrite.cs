
/// <summary>
/// Marker and contract interface for components that provide read/write access to the application's
/// root database/context.
/// </summary>
/// <remarks>
/// Implementations of this interface are expected to encapsulate the operations required to
/// read from and write to the primary data store used by the application. The interface is
/// intentionally minimal so that concrete implementations can expose the specific methods and
/// signatures appropriate for the data access technology in use (EF Core, Dapper, ADO.NET, etc.).
///
/// Typical responsibilities for an implementing type include:
/// - Exposing transactional boundaries or ambient transaction support.
/// - Providing atomic read/write operations and/or unit-of-work semantics.
/// - Ensuring proper resource management (opening/closing connections, disposing contexts).
///
/// This interface can be used as an injection point to decouple higher-level services from the
/// concrete persistence implementation, enabling easier testing and substitution.
/// </remarks>
/// <threadsafety>
/// The interface does not impose a threading model. Implementations must document whether they
/// are safe for concurrent use from multiple threads or are restricted to a single-threaded
/// context (for example, per-request or per-scope lifetimes).
/// </threadsafety>
/// <lifecycle>
/// Register implementations with an appropriate DI lifetime consistent with their concurrency
/// and resource-management characteristics (for example, scoped services for per-request
/// DbContext in web applications).
/// </lifecycle>
/// <example>
/// A consumer resolves an implementation and uses it to perform application-level operations:
/// <code>
/// // var db = serviceProvider.GetRequiredService&lt;IRootDbReadWrite&gt;();
/// // await db.SomeWriteOperationAsync(...);
/// </code>
/// </example>
namespace Organization.Shared.Interfaces;

public interface IRootDbReadWrite
{
    #region Users and Identity and Roles
    /// <summary>
    /// Get TAppUserOrganization by user id
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <returns>List of TAppUserOrganization</returns>
    public Task<List<TAppUserOrganization>> GetUserOrganizationsAsync(string userId, CancellationToken ct);

    /// <summary>
    /// Get TAppUserDepartment by user id
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <returns>List of TAppUserDepartment</returns>
    public Task<List<TAppUserDepartment>> GetUserDepartmentsAsync(string userId, CancellationToken ct);
    #endregion

    #region Organizations and Departments
    /// <summary>
    /// Get departments by organization id
    /// </summary>
    /// <param name="organizationId">Organization Id</param>
    /// <returns>Organization with departments</returns>
    public Task<List<TDepartment>?> GetDepartmentsAsync(int organizationId, CancellationToken ct);
    #endregion

    #region Generic CRUD
    /// <summary>
    /// Gets the row asynchronous.
    /// </summary>
    /// <typeparam name="T">Type of row to get.</typeparam>
    /// <param name="id">The value with Id to search for.</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Row found based on Id from <see cref="TBaseTable"/> class.</returns>
    public Task<T?> GetRowAsync<T>(int id, CancellationToken ct) where T : TBaseTable;

    /// <summary>
    /// Return all rows from T table.
    /// </summary>
    /// <typeparam name="T">Type of table</typeparam>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of rows</returns>
    public Task<List<T>> GetRowsAsync<T>(CancellationToken ct) where T : TBaseTable;
    /// <summary>
    /// Adds or updates row asynchronous.
    /// </summary>
    /// <typeparam name="T">Note that table must extend <see cref="TBaseTable"/></typeparam>
    /// <param name="value">Value to add or update</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated value.</returns>
    public Task<T> AddUpdateRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable;

    /// <summary>
    /// Adds or updates row asynchronous.
    /// </summary>
    /// <typeparam name="T">Note that table must extend <see cref="TBaseTable"/></typeparam>
    /// <param name="value">Value to add or update</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated value.</returns>
    public Task<T> UpdateRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable;

    /// <summary>
    /// Adds row asynchronous.
    /// </summary>
    /// <typeparam name="T">Note that table must extend <see cref="TBaseTable"/></typeparam>
    /// <param name="value">Value to add or update</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated value.</returns>
    public Task<T> AddRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable;
    /// <summary>
    /// Delete a row asynchronous.
    /// </summary>
    /// <typeparam name="T">Note that table must extend <see cref="TBaseTable"/></typeparam>
    /// <param name="value">Value to delete</param>
    /// <param name="ct">Cancellation token</param>
    public Task DeleteRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable;
    #endregion
}
