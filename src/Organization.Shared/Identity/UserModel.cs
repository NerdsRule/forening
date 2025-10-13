
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
}
