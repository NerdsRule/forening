namespace Organization.Shared.DatabaseObjects;

[Table("TAppUserDepartmentRoles")]
public class TAppUserDepartmentRole : TBaseTable
{
    [Required, ForeignKey(nameof(TAppUserDepartment))]
    public int AppUserDepartmentId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual TAppUserDepartment? AppUserDepartment { get; set; }

    [Required]
    public RolesEnum Role { get; set; }
}
