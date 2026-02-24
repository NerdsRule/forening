using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Organization.Shared.DatabaseObjects;

namespace Organization.Infrastructure.SqlDb;

public class SeedData
{
    /// <summary>
    /// Startup users
    /// </summary>
    private static readonly IEnumerable<SeedUser> seedUsers =
    [
        new ()
        {
            Email = "first.user@forening.dk",
            NormalizedEmail = "FIRST.USER@FORENING.DK",
            NormalizedUserName = "FIRST.USER@FORENING.DK",
            DisplayName = "First User",
            UserName = "first.user@forening.dk"
        },
        new ()
        {
            Email = "another.user@forening.dk",
            NormalizedEmail = "ANOTHER.USER@FORENING.DK",
            NormalizedUserName = "ANOTHER.USER@FORENING.DK",
            DisplayName = "Another User",
            UserName = "another.user@forening.dk"
        }
    ];

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (context.Users.Any())
        {
            return;
        }

        var userStore = new UserStore<AppUser>(context);
        var password = new PasswordHasher<AppUser>();

        using var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        // Create users
        foreach (var user in seedUsers)
        {
            var hashed = password.HashPassword(user, "ChangeMeFast1!");
            user.PasswordHash = hashed;
            await userStore.CreateAsync(user);
        }

        // Add Organzation seed data if needed
        var organization = new TOrganization
        {
            Name = "Organization One",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var org = await context.Organizations.AddAsync(organization);
        await context.SaveChangesAsync();

        // Add Department seed data if needed
        var department = new TDepartment
        {
            Name = "Department One",
            OrganizationId = org.Entity.Id,
            IsActive = true,
        };
        var dept = await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();

        // Add Department seed data if needed
        var department2 = new TDepartment
        {
            Name = "Department Two",
            OrganizationId = org.Entity.Id,
            IsActive = true,
        };
        await context.Departments.AddAsync(department2);
        await context.SaveChangesAsync();
        #region First user assignment and task seeding
        // Assign first user to Organization and Department as EnterpriseAdmin
        var firstUser = await userManager.FindByEmailAsync(seedUsers.First().Email!);
        if (firstUser is not null)
        {
            var userOrg = new TAppUserOrganization
            {
                AppUserId = firstUser.Id,
                OrganizationId = org.Entity.Id,
                Role = RolesEnum.EnterpriseAdmin
            };
            await context.AppUserOrganizations.AddAsync(userOrg);
            await context.SaveChangesAsync();
            var userDept = new TAppUserDepartment
            {
                AppUserId = firstUser.Id,
                DepartmentId = dept.Entity.Id,
                Role = RolesEnum.DepartmentAdmin
            };
            await context.AppUserDepartments.AddAsync(userDept);
            await context.SaveChangesAsync();
            var userDept2 = new TAppUserDepartment
            {
                AppUserId = firstUser.Id,
                DepartmentId = department2.Id,
                Role = RolesEnum.DepartmentAdmin
            };
            await context.AppUserDepartments.AddAsync(userDept2);
            await context.SaveChangesAsync();
            // Add a task to the department
            var task = new TTask
            {
                Name = "Initial Task",
                Description = "This is a seeded task for the department.",
                DepartmentId = dept.Entity.Id,
                CreatorUserId = firstUser.Id,
                EstimatedTimeMinutes = 60,
                PointsAwarded = 100,
                Status = TaskStatusEnum.NotStarted,
                DueDateUtc = DateTime.UtcNow.AddDays(7)
            };
            await context.Tasks.AddAsync(task);
            await context.SaveChangesAsync();
        }
        #endregion
        #region Second user assignment
        // Assign second user to Organization and Department as User
        var secondUser = await userManager.FindByEmailAsync(seedUsers.Last().Email!);
        if (secondUser is not null)
        {
            var userOrg = new TAppUserOrganization
            {
                AppUserId = secondUser.Id,
                OrganizationId = org.Entity.Id,
                Role = RolesEnum.OrganizationMember
            };
            await context.AppUserOrganizations.AddAsync(userOrg);
            await context.SaveChangesAsync();
            var userDept = new TAppUserDepartment
            {
                AppUserId = secondUser.Id,
                DepartmentId = dept.Entity.Id,
                Role = RolesEnum.DepartmentMember
            };
            await context.AppUserDepartments.AddAsync(userDept);
            await context.SaveChangesAsync();
            var userDept2 = new TAppUserDepartment
            {
                AppUserId = secondUser.Id,
                DepartmentId = department2.Id,
                Role = RolesEnum.DepartmentMember
            };
            await context.AppUserDepartments.AddAsync(userDept2);
            await context.SaveChangesAsync();
        }
        #endregion
    }
    private class SeedUser : AppUser
    {
        public string[]? RoleList { get; set; }
    }
}
