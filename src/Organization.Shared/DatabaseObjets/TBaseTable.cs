
namespace Organization.Shared.DatabaseObjets;
/// <summary>
/// Base class for all table objects
/// </summary>
public class TBaseTable
{
    /// <summary>
    /// Id of the table
    /// </summary>
    [Key]
    public int Id { get; set; }
}