namespace Organization.Shared.Interfaces;

/// <summary>
/// Interface for the UI state service, which manages UI state and notifies components of user updates.
/// </summary>
public interface IUiStateService
{
    /// <summary>
    /// Event that is triggered when the user information is updated. Components can subscribe to this event to refresh their data when the user is updated.
    /// </summary>
    event Action? UserUpdatedEvent;

    /// <summary>
    /// Method to invoke the UserUpdatedEvent, notifying all subscribers that the user information has been updated.
    /// </summary>
    void NotifyUserUpdated();
}
