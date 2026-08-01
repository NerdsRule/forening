namespace Organization.Shared.DatabaseObjects;

/// <summary>
/// Represents a signed money entry for a user within a department.
/// </summary>
[Table("TUserBudgets")]
public class TUserBudget : TBaseTable
{
    /// <summary>
    /// Foreign key to the user this budget entry belongs to.
    /// </summary>
    [Required, ForeignKey("AspNetUsers")]
    public string AppUserId { get; set; } = null!;

    /// <summary>
    /// Navigation property for the linked user.
    /// </summary>
    public virtual AppUser? AppUser { get; set; }

    /// <summary>
    /// Foreign key to the department this budget entry belongs to.
    /// </summary>
    [Required, ForeignKey(nameof(TDepartment))]
    public int DepartmentId { get; set; }

    /// <summary>
    /// Navigation property for the linked department.
    /// </summary>
    public virtual TDepartment? Department { get; set; }

    /// <summary>
    /// Signed money value. Positive amounts add budget, negative amounts spend budget.
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Description of the budget entry.
    /// </summary>
    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Tags for categorising the budget entry. Stored as JSON in the database.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Returns a non-null tag collection for the budget entry.
    /// </summary>
    /// <returns>Normalized tag collection.</returns>
    public List<string> GetNormalizedTags()
    {
        Tags ??= [];
        return Tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}