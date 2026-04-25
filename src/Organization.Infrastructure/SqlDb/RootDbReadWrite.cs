
namespace Organization.Infrastructure.SqlDb;

public class RootDbReadWrite : IRootDbReadWrite
{
    #region Constructor and properties
    /// <summary>
    /// Constructor with injected service
    /// </summary>
    /// <param name="dbContext">Injected db context</param>
    public RootDbReadWrite(AppDbContext dbContext)
    {
        Db = dbContext;
    }

    private AppDbContext Db { get; init; }
    #endregion

    #region Users and Identity and Roles
    /// <summary>
    /// Get TAppUserOrganization by user id
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <returns>List of TAppUserOrganization</returns>
    public async Task<List<TAppUserOrganization>> GetUserOrganizationsAsync(string userId, CancellationToken ct)
    {
        return await Db.AppUserOrganizations
            .Where(c => c.AppUserId == userId)
            .Include(c => c.Organization)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <summary>
    /// Get TAppUserDepartment by user id
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <returns>List of TAppUserDepartment</returns>
    public async Task<List<TAppUserDepartment>> GetUserDepartmentsAsync(string userId, CancellationToken ct)
    {
        return await Db.AppUserDepartments
            .Where(c => c.AppUserId == userId)
            .Include(c => c.Department)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <summary>
    /// Get users in organization
    /// </summary>
    /// <param name="organizationId">Organization Id</param>
    /// <returns>List of users in organization</returns>
    public async Task<List<AppUser>> GetUsersInOrganizationAsync(int organizationId, CancellationToken ct)
    {
        var res = Db.AppUserOrganizations.Where(c => c.OrganizationId == organizationId)
            !.Include(c => c.AppUser).AsNoTracking();
        if (res is null || !res.Any() || res.Select(c => c.AppUser).All(u => u == null))
            return [];
        return await res.Select(c => c.AppUser!).ToListAsync(ct) ?? [];
    }

    /// <summary>
    /// Get users in department
    /// </summary>
    /// <param name="departmentId">Department Id</param>
    /// <returns>List of users in department</returns>
    public async Task<List<AppUser>> GetUsersInDepartmentAsync(int departmentId, CancellationToken ct)
    {
        var res = Db.AppUserDepartments.Where(c => c.DepartmentId == departmentId)
            !.Include(c => c.AppUser).AsNoTracking();
        if (res is null || !res.Any() || res.Select(c => c.AppUser).All(u => u == null))
            return [];
        return await res.Select(c => c.AppUser!).ToListAsync(ct) ?? [];
    }

    /// <summary>
    /// Get TResetPassword by user id
    /// </summary> <param name="userId">User Id</param>
    /// <returns>List of TResetPassword</returns>
    public async Task<TResetPassword?> GetResetPasswordsByUserIdAsync(string userId, CancellationToken ct)
    {
        return await Db.ResetPasswords.AsNoTracking().FirstOrDefaultAsync(c => c.AppUserId == userId, ct);
    }

    /// <summary>
    /// Get all TResetPassword for one organization
    /// </summary> <param name="organizationId">Organization Id</param>
    /// <returns>List of TResetPassword</returns>
    public async Task<List<TResetPassword>> GetResetPasswordsByOrganizationIdAsync(int organizationId, CancellationToken ct)
    {
        var res = from r in Db.ResetPasswords
                  join uo in Db.AppUserOrganizations on r.AppUserId equals uo.AppUserId
                  join u in Db.Users on r.AppUserId equals u.Id
                  where uo.OrganizationId == organizationId
                  select new TResetPassword
                  {
                      Id = r.Id,
                      AppUserId = r.AppUserId,
                      ResetRequestCount = r.ResetRequestCount,
                      IsResetMailBlocked = r.IsResetMailBlocked,
                      LastResetRequestAt = r.LastResetRequestAt,
                      ResetToken = r.ResetToken,
                      ResetTokenCreatedAt = r.ResetTokenCreatedAt,
                      ResetMailBlockedAt = r.ResetMailBlockedAt,
                      AppUser = new AppUser
                      {
                          UserName = u.UserName,
                          DisplayName = u.DisplayName,
                          Email = u.Email,
                          EmailConfirmed = u.EmailConfirmed
                      }
                  };

        return await res.ToListAsync(ct);
    }
    #endregion

    #region Organizations and Departments
    /// <summary>
    /// Get departments by organization id
    /// </summary>
    /// <param name="organizationId">Organization Id</param>
    /// <param name="userId">User Id</param>
    /// <returns>Organization with departments</returns>
    public async Task<List<TDepartment>?> GetDepartmentsAsync(int organizationId, string userId, CancellationToken ct)
    {
        // Check if user is part of organization
        var org = Db.AppUserOrganizations.AsNoTracking().FirstOrDefault(user => user.AppUserId == userId && user.OrganizationId == organizationId);
        if (org is null)
            return [];
        var res = Db.Departments.Where(department => department.OrganizationId == organizationId).AsNoTracking();
        return await res.ToListAsync(ct);
    }
    #endregion

    #region TTasks and Departments
    /// <summary>
    /// Get tasks by department id
    /// </summary>
    /// <param name="departmentId">Department Id</param>
    /// <returns>List of tasks</returns>
    public async Task<List<TTask>> GetTasksByDepartmentAsync(int departmentId, CancellationToken ct)
    {
        // Check if user is part of department
        var res = new List<TTask>();
        var ownedTasks = Db.Tasks.Where(task => task.DepartmentId == departmentId).Include(d => d.Department).Include(u => u.AssignedUser).AsNoTracking();
        //var relatedTaskDepartments = Db.TaskDepartments.Where(td => td.DepartmentId == departmentId).Select(td => td.Task).Where(t => t != null).Cast<TTask>().Include(d => d.Department).AsNoTracking();
        var relatedTaskDepartments = Db.TaskDepartments.Where(td => td.DepartmentId == departmentId).Select(td => td.Task).Where(t => t != null).Cast<TTask>().AsNoTracking();
        if (ownedTasks is not null && ownedTasks.Any())
            res.AddRange(await ownedTasks.ToListAsync(ct));
        if (relatedTaskDepartments is not null && relatedTaskDepartments.Any())
        {
            // Can not include department and assigned user in relatedTaskDepartments query because of EF Core tracking issues, so we need to load them separately for each task
            var userInDepartment = await Db.AppUserDepartments.Where(u => u.DepartmentId == departmentId).Select(u => u).AsNoTracking().ToListAsync(ct);
            foreach (var task in relatedTaskDepartments)
            {
                task.CreatorUser = userInDepartment.Where(u => u.AppUserId == task.CreatorUserId).Select(u => u.AppUser).FirstOrDefault();
            }
            res.AddRange(relatedTaskDepartments);
        }
        return res;
    }

    public async Task<List<string>> GetDistinctTaskTagsByDepartmentAsync(int departmentId, CancellationToken ct)
    {
        var tagLists = await Db.Tasks
            .Where(t => t.DepartmentId == departmentId)
            .Select(t => t.Tags)
            .AsNoTracking()
            .ToListAsync(ct);

        return tagLists
            .SelectMany(tags => tags)
            .Select(tag => tag.Trim().ToLowerInvariant())
            .Where(tag => tag.Length > 0)
            .Distinct()
            .OrderBy(tag => tag)
            .ToList();
    }

    #endregion

    #region Prizes and Departments
    /// <summary>
    /// Get prizes by department id.
    /// </summary>
    /// <param name="departmentId">Department Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of prizes with related users loaded.</returns>
    public async Task<List<TPrize>> GetPrizesByDepartmentAsync(int departmentId, CancellationToken ct)
    {
        return await Db.Prizes
            .Where(prize => prize.DepartmentId == departmentId)
            .Include(prize => prize.CreatorUser)
            .Include(prize => prize.AssignedUser)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <summary>
    /// Get a prize by id.
    /// </summary>
    /// <param name="id">Prize id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Prize or null if not found.</returns>
    public async Task<TPrize?> GetPrizeByIdAsync(int id, CancellationToken ct)
    {
        return await Db.Prizes
            .Where(prize => prize.Id == id)
            .Include(prize => prize.CreatorUser)
            .Include(prize => prize.AssignedUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Get prices by assigned user id.
    /// </summary>
    /// <param name="assignedUserId">Assigned user id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of prizes assigned to user.</returns>
    public async Task<List<TPrize>> GetPrizesByAssignedUserIdAsync(string assignedUserId, CancellationToken ct)
    {        
        return await Db.Prizes
            .Where(prize => prize.AssignedUserId == assignedUserId)
            .Include(prize => prize.CreatorUser)
            .Include(prize => prize.AssignedUser)
            .AsNoTracking()
            .ToListAsync(ct);
    }
    #endregion

    #region View for tasks with points awarded
    /// <summary>
    /// Get tasks with points awarded by department id
    /// </summary>
    /// <param name="departmentId">Department Id</param>
    /// <returns>List of tasks with points awarded</returns>
    public async Task<List<VTaskPointsAwarded>> GetTasksWithPointsAwardedByDepartmentAsync(int departmentId, CancellationToken ct)
    {
        var query = from t in Db.Tasks
                    join u in Db.Users on t.AssignedUserId equals u.Id
                    join d in Db.Departments on t.DepartmentId equals d.Id
                    where t.DepartmentId == departmentId && t.Status == Shared.TaskStatusEnum.VerifiedCompleted
                    select new VTaskPointsAwarded
                    {
                        UserId = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        UserEmail = u.Email ?? string.Empty,
                        UserDisplayName = u.DisplayName,
                        TaskId = t.Id,
                        TaskName = t.Name,
                        TaskDescription = t.Description,
                        TaskStatus = t.Status,
                        TaskPointsAwarded = t.PointsAwarded,
                        DepartmentId = d.Id,
                        DepartmentName = d.Name
                    };

        return await query.AsNoTracking().ToListAsync(ct);
    }

    /// <summary>
    /// Get VTaskPointsAwarded by user id.
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <returns>List of VTaskPointsAwarded</returns>
    public async Task<List<VTaskPointsAwarded>> GetTasksWithPointsAwardedByUserAsync(string userId, CancellationToken ct)
    {
        var query = from t in Db.Tasks
                    join u in Db.Users on t.AssignedUserId equals u.Id
                    join d in Db.Departments on t.DepartmentId equals d.Id
                    where t.AssignedUserId == userId && t.Status == Shared.TaskStatusEnum.VerifiedCompleted
                    select new VTaskPointsAwarded
                    {
                        UserId = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        UserEmail = u.Email ?? string.Empty,
                        UserDisplayName = u.DisplayName,
                        TaskId = t.Id,
                        TaskName = t.Name,
                        TaskDescription = t.Description,
                        TaskStatus = t.Status,
                        TaskPointsAwarded = t.PointsAwarded,
                        DepartmentId = d.Id,
                        DepartmentName = d.Name
                    };
        return await query.AsNoTracking().ToListAsync(ct);
    }

    /// <summary>
    /// Get top 5 users with most points awarded in department and return as list of VTaskPointsAwarded.
    /// If user is not within top 5, add user after the top users with their points awarded and ranking.
    /// </summary> 
    /// <param name="userId">User Id</param>
    /// <param name="departmentId">Department Id</param>
    /// <param name="topCount">Number of top users to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of VTaskPointsAwarded</returns>
    public async Task<List<VTaskPointsAwarded>> GetTopUsersWithPointsAwardedByDepartmentAsync(string userId, int departmentId, int topCount, CancellationToken ct)
    {
        var query = from t in Db.Tasks
                    join u in Db.Users on t.AssignedUserId equals u.Id
                    join d in Db.Departments on t.DepartmentId equals d.Id
                    where t.DepartmentId == departmentId && t.Status == Shared.TaskStatusEnum.VerifiedCompleted
                    group t by new { u.Id, u.UserName, u.Email, u.DisplayName, DepartmentId = d.Id, DepartmentName = d.Name } into g
                    orderby g.Sum(t => t.PointsAwarded) descending
                    select new VTaskPointsAwarded
                    {
                        UserId = g.Key.Id,
                        UserName = g.Key.UserName ?? string.Empty,
                        UserEmail = g.Key.Email ?? string.Empty,
                        UserDisplayName = g.Key.DisplayName,
                        DepartmentId = g.Key.DepartmentId,
                        DepartmentName = g.Key.DepartmentName,
                        TaskPointsAwarded = g.Sum(t => t.PointsAwarded)
                    };
        var res = await query.AsNoTracking().OrderByDescending(v => v.TaskPointsAwarded).ToListAsync(ct);
        // Update user ranking based on position in list
        for (int i = 0; i < res.Count; i++)
        {
            res[i].UserRanking = i + 1; // Ranking starts at 1
        }
        // If user is not within topCount, add user after the top users with their points awarded and ranking
        if (res.Any(v => v.UserId == userId) && res.First(u => u.UserId == userId).UserRanking > topCount)
        {
            var userInList = res.First(u => u.UserId == userId);
            res = res.Take(topCount).ToList();
            res.Add(userInList);
        }
        return res;
    }

    #endregion

    #region Generic CRUD
    /// <summary>
    /// Gets the row asynchronous.
    /// </summary>
    /// <typeparam name="T">Type of row to get.</typeparam>
    /// <param name="value">The value with Id to search for.</param>
    /// <returns>Row found based on Id from <see cref="TBaseTable"/> class.</returns>
    public async Task<T?> GetRowAsync<T>(int id, CancellationToken ct) where T : TBaseTable
    {
        var dbSet = Db.Set<T>();
        return await dbSet.AsNoTracking().FirstOrDefaultAsync<T>(c => c.Id == id, ct);
    }

    /// <summary>
    /// Return all rows from T table.
    /// </summary>
    /// <typeparam name="T">Type of table</typeparam>
    /// <returns>List of rows</returns>
    public async Task<List<T>> GetRowsAsync<T>(CancellationToken ct) where T : TBaseTable
    {
        var dbSet = Db.Set<T>();
        return await dbSet.AsNoTracking().Select(c => c).ToListAsync<T>(ct);
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
