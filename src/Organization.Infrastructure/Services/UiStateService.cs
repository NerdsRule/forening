namespace Organization.Infrastructure.Services;

/// <summary>
/// Service to manage UI state, such as notifying components of user updates.
/// </summary>
public class UiStateService : IUiStateService
{
    /// <summary>
    /// Event that is triggered when the user information is updated. Components can subscribe to this event to refresh their data when the user is updated.
    /// </summary>
    public event Action? UserUpdatedEvent;

    /// <summary>
    /// Method to invoke the UserUpdatedEvent, notifying all subscribers that the user information has been updated.
    /// </summary>
    public void NotifyUserUpdated()
    {
        UserUpdatedEvent?.Invoke();
    }
}
