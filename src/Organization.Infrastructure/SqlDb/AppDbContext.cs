
namespace Organization.Infrastructure.SqlDb;

public class AppDbContext : IdentityDbContext<AppUser>
{
 public AppDbContext() 
    {
        
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        //options.UseSqlite("Data Source=org.db");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AppUser>(entity =>
        {
            entity.Property(e => e.MemberNumber).HasMaxLength(50);
        });
        builder.Entity<TFidoCredential>(entity =>
        {
            entity.HasIndex(c => c.CredentialId).IsUnique();
            entity.HasIndex(c => c.AppUserId);
        });
        builder.Entity<TResetPassword>(entity =>
        {
            entity.HasIndex(r => r.AppUserId).IsUnique();
            entity.Property(r => r.ResetRequestCount).IsRequired().HasDefaultValue(0);
            entity.Property(r => r.IsResetMailBlocked).IsRequired().HasDefaultValue(false);
            entity.Property(r => r.ResetToken).HasMaxLength(4000);
        });
        builder.Entity<TAppUserOrganization>().HasIndex(u => new { u.AppUserId, u.OrganizationId }).IsUnique();
        builder.Entity<TAppUserDepartment>().HasIndex(u => new { u.AppUserId, u.DepartmentId }).IsUnique();
        builder.Entity<TTaskDepartment>().HasIndex(u => new { u.TaskId, u.DepartmentId }).IsUnique();
        builder.Entity<TTaskDepartment>()
            .HasOne(td => td.Department)
            .WithMany()
            .HasForeignKey(td => td.DepartmentId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<TTask>(entity =>
        {
            entity.Property(e => e.PointsAwarded).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.Status).IsRequired().HasDefaultValue(TaskStatusEnum.NotStarted);
        });
         builder.Entity<TPrize>(entity =>
        {
            entity.Property(e => e.PointsCost).IsRequired().HasDefaultValue(0);  
            entity.Property(e => e.Status).IsRequired().HasDefaultValue(PrizeStatusEnum.Available);
        });
        base.OnModelCreating(builder);
    }

    #region Tables
    public DbSet<TOrganization> Organizations { get; set; } = null!;
    public DbSet<TDepartment> Departments { get; set; } = null!;
    public DbSet<TAppUserOrganization> AppUserOrganizations { get; set; } = null!;
    public DbSet<TAppUserDepartment> AppUserDepartments { get; set; } = null!;
    public DbSet<TTask> Tasks { get; set; } = null!;
    public DbSet<TTaskDepartment> TaskDepartments { get; set; } = null!;
    public DbSet<TPrize> Prizes { get; set; } = null!;
    public DbSet<TFidoCredential> FidoCredentials { get; set; } = null!;
    public DbSet<TResetPassword> ResetPasswords { get; set; } = null!;
    #endregion
}
