
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
        //options.UseSqlServer("Server=tcp:space4it.database.windows.net,1433;Initial Catalog=roskildefestival;Persist Security Info=False;User ID=roskildeowner;Password=[passsword];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AppUser>(entity =>
        {
            entity.Property(e => e.Points).HasDefaultValue(0);
            entity.Property(e => e.UsedPoints).HasDefaultValue(0);
            entity.Property(e => e.MemberNumber).HasMaxLength(50);
        });
        builder.Entity<TAppUserOrganization>().HasIndex(u => new { u.AppUserId, u.OrganizationId }).IsUnique();
        builder.Entity<TAppUserDepartment>().HasIndex(u => new { u.AppUserId, u.DepartmentId }).IsUnique();
        // builder.Entity<TProduct>().HasIndex(u => new {u.NormalizedName, u.TProductSizeId, u.TProductTypeId}).IsUnique();
        // builder.Entity<TProductType>().HasIndex(u => u.NormalizedName).IsUnique();
        // builder.Entity<TCustomer>().HasIndex(u => u.NormalizedName).IsUnique();
        // builder.Entity<TOrderItem>().HasIndex(u => new {u.TOrderId, u.TProductId}).IsUnique();
        // builder.Entity<TProductSize>().HasIndex(u => u.NormalizedName).IsUnique();
        // builder.Entity<TProductsStock>().HasIndex(u => new {u.TCustomerId, u.TProductId}).IsUnique();
        // builder.Entity<TCustomerRelation>().HasIndex(u => new {u.StockCustomerId, u.SalesCustomerId, u.PurchaseCustomerId}).IsUnique();
        // builder.Entity<TCustomerRelation>().HasOne(x => x.StockCustomer).WithMany().HasForeignKey(x => x.StockCustomerId).OnDelete(DeleteBehavior.Restrict);
        // builder.Entity<TCustomerRelation>().HasOne(x => x.SalesCustomer).WithMany().HasForeignKey(x => x.SalesCustomerId).OnDelete(DeleteBehavior.Restrict);
        // builder.Entity<TCustomerRelation>().HasOne(x => x.PurchaseCustomer).WithMany().HasForeignKey(x => x.PurchaseCustomerId).OnDelete(DeleteBehavior.Restrict);
        // builder.Entity<TCustomerUser>().HasIndex(u => new {u.CustomerId, u.UserId}).IsUnique();
        // builder.Entity<TOrder>().HasOne(x => x.TSourceCustomer).WithMany().HasForeignKey(x => x.TSourceCustomerId).OnDelete(DeleteBehavior.NoAction);
        // builder.Entity<TOrder>().HasOne(x => x.TTargetCustomer).WithMany().HasForeignKey(x => x.TTargetCustomerId).OnDelete(DeleteBehavior.NoAction);
        base.OnModelCreating(builder);
    }

    #region Tables
    public DbSet<TOrganization> Organizations { get; set; } = null!;
    public DbSet<TDepartment> Departments { get; set; } = null!;
    public DbSet<TAppUserOrganization> AppUserOrganizations { get; set; } = null!;
    public DbSet<TAppUserDepartment> AppUserDepartments { get; set; } = null!;
    public DbSet<TTask> Tasks { get; set; } = null!;
    public DbSet<TPrize> Prizes { get; set; } = null!;
    #endregion
}
