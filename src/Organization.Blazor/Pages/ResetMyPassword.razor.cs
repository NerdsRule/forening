namespace Organization.Blazor.Pages;

partial class ResetMyPassword
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "userId")]
    public string? UserId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "token")]
    public string? ResetToken { get; set; }
}