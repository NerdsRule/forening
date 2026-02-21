
namespace Organization.Blazor.Layout;

/// <summary>
/// A component to display the result of a form submission, showing success or error messages based on the provided FormResult object.
/// </summary>
partial class FormResultComponent
{
    private FormResult? Result { get; set; }
    private string? FadeCssClass { get; set; }

    /// <summary>
    /// Set the form result to display and optionally specify a timeout for how long the message should be shown before fading out. This method will update the Result property with the provided FormResult, trigger a fade-in animation, and if a timeout is specified, it will automatically fade out and clear the result after the given duration. This allows for a user-friendly way to display feedback messages after form submissions without requiring manual dismissal by the user.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="timeoutSeconds"></param>
    public void SetFormResult(FormResult result, int timeoutSeconds = 0)
    {
        Result = result;
        StateHasChanged();
        FadeCssClass = "fade-in show";
        StateHasChanged();
        if (timeoutSeconds > 0)
        {
            Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)).ContinueWith(_ =>
            {
                FadeCssClass = "fade-out hide";
                StateHasChanged();
                Task.Delay(500).ContinueWith(_ =>
                {
                    Result = null;
                    StateHasChanged();
                });
            });
        }
        StateHasChanged();
    }
    
    /// <summary>
    /// Clear the form result and reset the fade CSS class. This method can be called to manually clear any displayed messages and reset the component to its default state, ensuring that no messages are shown and that the component is ready for the next form submission.
    /// </summary> 
    public void ClearFormResult()
    {        
        Result = null;
    }

}
