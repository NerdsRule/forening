namespace Organization.Shared.DatabaseObjects;

[Table("TAppUserOrganizationRoles")]
public class TAppUserOrganizationRole : TBaseTable
{
    [Required, ForeignKey(nameof(TAppUserOrganization))]
    public int AppUserOrganizationId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual TAppUserOrganization? AppUserOrganization { get; set; }

    [Required]
    public RolesEnum Role { get; set; }
}
