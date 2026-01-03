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
            Email = "lasse.tarp@space4it.dk", 
            NormalizedEmail = "LASSE.TARP@SPACE4IT.DK", 
            NormalizedUserName = "LASSE.TARP@SPACE4IT.DK", 
            RoleList = [ RolesEnum.EnterpriseAdmin.ToString() ], 
            UserName = "lasse.tarp@space4it.dk"
        }
    ];

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (context.Users.Any())
        {
            return;
        }

        var userStore = new UserStore<AppUser>(context);
        var password = new PasswordHasher<AppUser>();

        using var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Add roles from RoskildeRoleNames
        foreach (var roleName in Enum.GetNames(typeof(RolesEnum)))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

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

        // Add Department seed data if needed
        var department = new TDepartment
        {
            Name = "Department One",
            OrganizationId = org.Entity.Id,
            IsActive = true,
        };
        var dept = await context.Departments.AddAsync(department);

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

            var userDept = new TAppUserDepartment
            {
                AppUserId = firstUser.Id,
                DepartmentId = dept.Entity.Id,
                Role = RolesEnum.DepartmentAdmin
            };
            await context.AppUserDepartments.AddAsync(userDept);
        }

        await context.SaveChangesAsync();
    }
    private class SeedUser : AppUser
    {
        public string[]? RoleList { get; set; }
    }
}
