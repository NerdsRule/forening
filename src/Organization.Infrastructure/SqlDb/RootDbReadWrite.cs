
namespace Organization.Infrastructure.SqlDb;

public class RootDbReadWrite : IRootDbReadWrite
{
#region Constructor and properties
    /// <summary>
    /// Constructor with injected service
    /// </summary>
    /// <param name="injectedService">Injected service</param>
    public RootDbReadWrite(IServiceCollection injectedService)
    {
        var ServiceProvider = injectedService.BuildServiceProvider();
        Db = ServiceProvider.GetRequiredService<AppDbContext>();        
    }

    private AppDbContext Db { get; init; }
    #endregion

    #region Generic CRUD
    /// <summary>
    /// Gets the row asynchronous.
    /// </summary>
    /// <typeparam name="T">Type of row to get.</typeparam>
    /// <param name="value">The value with Id to search for.</param>
    /// <returns>Row found based on Id from <see cref="TBaseTable"/> class.</returns>
    public async Task<T?> GetRowAsync<T>(int id, int timeoutSeconds = 60) where T : TBaseTable
    {
        var dbSet = Db.Set<T>();
        return await dbSet.AsNoTracking().FirstOrDefaultAsync<T>(c => c.Id == id);
    }

    /// <summary>
    /// Return all rows from T table.
    /// </summary>
    /// <typeparam name="T">Type of table</typeparam>
    /// <returns>List of rows</returns>
    public async Task<List<T>> GetRowsAsync<T>() where T : TBaseTable
    {
        var dbSet = Db.Set<T>();
        return await dbSet.AsNoTracking().Select(c => c).ToListAsync<T>();
    }

    /// <summary>
    /// Adds or updates row asynchronous.
    /// </summary>
    /// <typeparam name="T">Note that table must extend <see cref="TBaseTable"/></typeparam>
    /// <param name="value">Value to add or update</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated value.</returns>
    public async Task<T> AddUpdateRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable
    {
        var dbSet = Db.Set<T>();
        var entryFound = await dbSet.AnyAsync<T>(c => c.Id == value.Id);
        try
        {
            if (entryFound)
            {
                Db.Entry<T>(value).State = EntityState.Modified;
            }
            else
            {
                Db.Entry<T>(value).State = EntityState.Added;
            }
            await Db.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            Db.Entry<T>(value).State = EntityState.Detached;
        }
        return value;
    }

    /// <summary>
    /// Adds or updates row asynchronous.
    /// </summary>
    /// <typeparam name="T">Note that table must extend <see cref="TBaseTable"/></typeparam>
    /// <param name="value">Value to add or update</param>
    /// <returns>Updated value.</returns>
    public async Task<T> UpdateRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable
    {
        var dbSet = Db.Set<T>();
        var entryFound = await dbSet.AnyAsync<T>(c => c.Id == value.Id);
        if (!entryFound)
            throw new Exception($"Could not find row with id {value.Id}");

        try
        {
            if (entryFound)
            {
                Db.Entry<T>(value).State = EntityState.Modified;
            }
            await Db.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            Db.Entry<T>(value).State = EntityState.Detached;
        }
        return value;
    }

    /// <summary>
    /// Adds row asynchronous.
    /// </summary>
    /// <typeparam name="T">Note that table must extend <see cref="TBaseTable"/></typeparam>
    /// <param name="value">Value to add or update</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated value.</returns>
    public async Task<T> AddRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable
    {
        var dbSet = Db.Set<T>();
        try
        {
            Db.Entry<T>(value).State = EntityState.Added;
            await Db.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            Db.Entry<T>(value).State = EntityState.Detached;
        }
        return value;
    }

    /// <summary>
    /// Delete a row asynchronous.
    /// </summary>
    /// <typeparam name="T">Note that table must extend <see cref="TBaseTable"/></typeparam>
    /// <param name="value">Value to delete</param>
    /// <param name="ct">Cancellation token</param>
    public async Task DeleteRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable
    {
        var dbSet = Db.Set<T>();
        var entryFound = await dbSet.AnyAsync<T>(c => c.Id == value.Id);
        try
        {
            if (entryFound)
            {
                Db.Entry<T>(value).State = EntityState.Deleted;
            }
            await Db.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            Db.Entry<T>(value).State = EntityState.Detached;
        }
    }
    #endregion
}
