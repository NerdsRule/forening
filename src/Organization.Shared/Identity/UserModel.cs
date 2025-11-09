
using System.Security.Claims;

namespace Organization.Shared.Identity;

/// <summary>
/// User info returned to the client from the API
/// </summary>
public class UserModel
{
    /// <summary>
    /// User Id
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Email of the user
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Claims associated with the user
    /// </summary>
    public List<Claim> Claims { get; set; } = [];
}
