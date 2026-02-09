
namespace Organization.Blazor.Layout;

/// <summary>
/// A component to display the result of a form submission, showing success or error messages based on the provided FormResult object.
/// </summary>
partial class FormResultComponent
{
    [Parameter] public FormResult? Result { get; set; }
}
