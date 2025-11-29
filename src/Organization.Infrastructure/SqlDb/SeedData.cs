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

        foreach (var user in seedUsers)
        {
            var hashed = password.HashPassword(user, "ChangeMeFast1!");
            user.PasswordHash = hashed;
            await userStore.CreateAsync(user);

            if (user.Email is not null)
            {
                var appUser = await userManager.FindByEmailAsync(user.Email);

                if (appUser is not null && user.RoleList is not null)
                {
                    await userManager.AddToRolesAsync(appUser, user.RoleList);
                }
            }
        }

        await context.SaveChangesAsync();
    }
    private class SeedUser : AppUser
    {
        public string[]? RoleList { get; set; }
    }
}
