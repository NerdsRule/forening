using System.Security.Claims;
using Organization.ApiService.V1;
using Organization.Shared.DatabaseObjects;
using Organization.Shared.Interfaces;

namespace Organization.Test;

public class UserRolesHelpersTests
{
    [Fact]
    public async Task CanAccessOwnBudgetAsync_WhenUserIsAuthenticatedAndTargetingOwnBudget_ReturnsTrue()
    {
        var user = CreateUser("user-1");
        var db = new StubRootDbReadWrite();

        var canAccess = await UserRolesHelpers.CanAccessUserBudgetAsync(user, "user-1", 42, db, TestContext.Current.CancellationToken);

        canAccess.Should().BeTrue();
    }

    private static ClaimsPrincipal CreateUser(string userId)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private sealed class StubRootDbReadWrite : IRootDbReadWrite
    {
        public Task<List<TAppUserOrganization>> GetUserOrganizationsAsync(string userId, CancellationToken ct) => Task.FromResult(new List<TAppUserOrganization>());

        public Task<List<TAppUserDepartment>> GetUserDepartmentsAsync(string userId, CancellationToken ct) => Task.FromResult(new List<TAppUserDepartment>());

        public Task<List<AppUser>> GetUsersInOrganizationAsync(int organizationId, CancellationToken ct) => Task.FromResult(new List<AppUser>());

        public Task<List<AppUser>> GetUsersInDepartmentAsync(int departmentId, CancellationToken ct) => Task.FromResult(new List<AppUser>());

        public Task<TResetPassword?> GetResetPasswordsByUserIdAsync(string userId, CancellationToken ct) => Task.FromResult<TResetPassword?>(null);

        public Task<List<TResetPassword>> GetResetPasswordsByOrganizationIdAsync(int organizationId, CancellationToken ct) => Task.FromResult(new List<TResetPassword>());

        public Task<List<TDepartment>?> GetDepartmentsAsync(int organizationId, string userId, CancellationToken ct) => Task.FromResult<List<TDepartment>?>(null);

        public Task<List<TTask>> GetTasksByDepartmentAsync(int departmentId, CancellationToken ct) => Task.FromResult(new List<TTask>());

        public Task<List<string>> GetDistinctTaskTagsByDepartmentAsync(int departmentId, CancellationToken ct) => Task.FromResult(new List<string>());

        public Task<List<TPrize>> GetPrizesByDepartmentAsync(int departmentId, CancellationToken ct) => Task.FromResult(new List<TPrize>());

        public Task<TPrize?> GetPrizeByIdAsync(int id, CancellationToken ct) => Task.FromResult<TPrize?>(null);

        public Task<List<TPrize>> GetPrizesByAssignedUserIdAsync(string assignedUserId, CancellationToken ct) => Task.FromResult(new List<TPrize>());

        public Task<List<TUserBudget>> GetUserBudgetsAsync(string appUserId, int departmentId, CancellationToken ct) => Task.FromResult(new List<TUserBudget>());

        public Task<List<TUserBudget>> GetDepartmentBudgetsAsync(int departmentId, CancellationToken ct) => Task.FromResult(new List<TUserBudget>());

        public Task<List<string>> GetDistinctBudgetTagsByDepartmentAsync(int departmentId, CancellationToken ct) => Task.FromResult(new List<string>());

        public Task<List<string>> GetBudgetDescriptionSuggestionsAsync(int departmentId, string query, CancellationToken ct) => Task.FromResult(new List<string>());

        public Task<List<VTaskPointsAwarded>> GetTasksWithPointsAwardedByDepartmentAsync(int departmentId, CancellationToken ct) => Task.FromResult(new List<VTaskPointsAwarded>());

        public Task<List<VTaskPointsAwarded>> GetTasksWithPointsAwardedByUserAsync(string userId, CancellationToken ct) => Task.FromResult(new List<VTaskPointsAwarded>());

        public Task<List<VTaskPointsAwarded>> GetTopUsersWithPointsAwardedByDepartmentAsync(string userId, int departmentId, int topCount, CancellationToken ct) => Task.FromResult(new List<VTaskPointsAwarded>());

        public Task<T?> GetRowAsync<T>(int id, CancellationToken ct) where T : TBaseTable => Task.FromResult<T?>(null);

        public Task<List<T>> GetRowsAsync<T>(CancellationToken ct) where T : TBaseTable => Task.FromResult(new List<T>());

        public Task<T> AddUpdateRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable => Task.FromResult(value);

        public Task<T> UpdateRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable => Task.FromResult(value);

        public Task<T> AddRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable => Task.FromResult(value);

        public Task DeleteRowAsync<T>(T value, CancellationToken ct) where T : TBaseTable => Task.CompletedTask;
    }
}
