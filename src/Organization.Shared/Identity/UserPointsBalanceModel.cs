namespace Organization.Shared.Identity;

/// <summary>
/// Represents user points totals and computed balance.
/// </summary>
public class UserPointsBalanceModel
{
    /// <summary>
    /// User id.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// User name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// User email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Total points awarded from verified tasks.
    /// </summary>
    public int TotalPointsAwarded { get; set; }

    /// <summary>
    /// Total points redeemed from redeemed prizes.
    /// </summary>
    public int TotalPointsRedeemed { get; set; }

    /// <summary>
    /// Points balance, calculated as TotalPointsAwarded minus TotalPointsRedeemed.
    /// </summary>
    public int PointsBalance { get; set; }
}
