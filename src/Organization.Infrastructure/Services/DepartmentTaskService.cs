
namespace Organization.Infrastructure.Services;
/// <summary>
/// Service for managing department tasks and tasks within departments
/// </summary>
/// <param name="httpClientFactory"></param>
/// <param name="logger"></param>
public class DepartmentTaskService(IHttpClientFactory httpClientFactory, ILogger<DepartmentTaskService> logger) : IDepartmentTaskService
{
    /// <summary>
    /// Map the JavaScript-formatted properties to C#-formatted classes.
    /// </summary>
    private readonly JsonSerializerOptions jsonSerializerOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

    /// <summary>
    /// Special auth client.
    /// </summary>
    private readonly HttpClient httpClient = httpClientFactory.CreateClient("Auth");

    #region Task Management
    /// <summary>
    /// Get all tasks for a specific department
    /// </summary>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of tasks associated with the department</returns>
    public async Task<(List<TTask>? data, FormResult? formResult)> GetTasksByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving tasks for department {DepartmentId}", departmentId);
            var response = await httpClient.GetAsync($"/v1/api/Task/ByDepartment/{departmentId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var tasks = await response.Content.ReadFromJsonAsync<List<TTask>>(jsonSerializerOptions, cancellationToken);
                return (tasks, null);
            }
            
            logger.LogWarning("Failed to retrieve tasks for department {DepartmentId}. Status: {StatusCode}", departmentId, response.StatusCode);
            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve tasks"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tasks for department {DepartmentId}", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving tasks"] });
        }
    }

    /// <summary>
    /// Get tasks that are owned by a specific department
    /// </summary>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of tasks owned by the department</returns>
    public async Task<(List<TTask>? data, FormResult? formResult)> GetOwnedTasksByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving owned tasks for department {DepartmentId}", departmentId);
            // Get all tasks for the department and filter for owned tasks
            var allTasks = await GetTasksByDepartmentIdAsync(departmentId, cancellationToken);
            
            // Filter tasks where the department is the owner (DepartmentId matches)
            var ownedTasks = allTasks.data?.Where(task => task.DepartmentId == departmentId).ToList();
            
            return (ownedTasks, allTasks.formResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving owned tasks for department {DepartmentId}", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving owned tasks"] });
        }
    }

    /// <summary>
    /// Add or update a task
    /// </summary>
    /// <param name="task">The task to add or update</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The added or updated task, or an error if the operation failed</returns>
    public async Task<(TTask? data, FormResult? formResult)> AddUpdateTaskAsync(TTask task, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Adding/updating task {TaskId}", task?.Id);
            
            if (task == null)
            {
                return (null, new FormResult { Succeeded = false, ErrorList = ["Task cannot be null"] });
            }

            var json = JsonSerializer.Serialize(task, jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync("/v1/api/Task", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var updatedTask = await response.Content.ReadFromJsonAsync<TTask>(jsonSerializerOptions, cancellationToken);
                return (updatedTask, new FormResult { Succeeded = true });
            }
            else
            {
                var errorResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
                return (null, errorResult ?? new FormResult { Succeeded = false, ErrorList = ["Unknown error occurred"] });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding/updating task {TaskId}", task?.Id);
            return (null, new FormResult { Succeeded = false, ErrorList = [ex.Message] });
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="taskId">The ID of the task to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A result indicating whether the deletion was successful or if there was an error</returns>
    public async Task<FormResult> DeleteTaskAsync(int taskId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting task {TaskId}", taskId);
            
            var response = await httpClient.DeleteAsync($"/v1/api/Task/{taskId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
                return result ?? new FormResult { Succeeded = true };
            }
            else
            {
                var errorResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
                return errorResult ?? new FormResult { Succeeded = false, ErrorList = ["Unknown error occurred"] };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting task {TaskId}", taskId);
            return new FormResult { Succeeded = false, ErrorList = [ex.Message] };
        }
    }

    #endregion

    #region Department Task Management
    /// <summary>
    /// Add a TTaskDepartment association
    /// </summary>
    /// <param name="departmentTask">The TTaskDepartment association to add</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The added TTaskDepartment association, or an error if the operation failed</returns>
    public async Task<(TTaskDepartment? data, FormResult? formResult)> AddDepartmentTaskAsync(TTaskDepartment departmentTask, CancellationToken cancellationToken)
    {
        try
        {
     
            var json = JsonSerializer.Serialize(departmentTask, jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync("/v1/api/TaskDepartment", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var addedDepartmentTask = await response.Content.ReadFromJsonAsync<TTaskDepartment>(jsonSerializerOptions, cancellationToken);
                return (addedDepartmentTask, new FormResult { Succeeded = true });
            }
            else
            {
                var errorResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
                return (null, errorResult ?? new FormResult { Succeeded = false, ErrorList = ["Unknown error occurred"] });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding department task association for TaskId {TaskId} and DepartmentId {DepartmentId}", departmentTask?.TaskId, departmentTask?.DepartmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = [ex.Message] });
        }
    }

    /// <summary>
    /// Delete a TTaskDepartment association
    /// </summary>
    /// <param name="departmentTaskId">The ID of the TTaskDepartment association to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A result indicating whether the deletion was successful or if there was an error</returns>
    public async Task<FormResult> DeleteDepartmentTaskAsync(int departmentTaskId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting department task association {DepartmentTaskId}", departmentTaskId);
            
            // TODO: Implement API endpoint for TTaskDepartment operations
            // For now, return a placeholder response indicating the feature is not yet implemented
            logger.LogWarning("TTaskDepartment API endpoints not yet implemented");
            return new FormResult { 
                Succeeded = false, 
                ErrorList = ["TTaskDepartment API endpoints are not yet implemented. Please implement the API endpoints first."] 
            };

            // When API endpoints are available, uncomment and modify the following:
            /*
            var response = await httpClient.DeleteAsync($"/v1/api/TaskDepartment/{departmentTaskId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
                return result ?? new FormResult { Succeeded = true };
            }
            else
            {
                var errorResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
                return errorResult ?? new FormResult { Succeeded = false, ErrorList = ["Unknown error occurred"] };
            }
            */
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting department task association {DepartmentTaskId}", departmentTaskId);
            return new FormResult { Succeeded = false, ErrorList = [ex.Message] };
        }
    }

    #endregion

    #region Users with access to a department and Organization
    /// <summary>
    /// Get users with access to a specific department
    /// </summary>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of users who have access to the department</returns>
    public async Task<(List<UserModel>? data, FormResult? formResult)> GetUsersWithAccessToDepartmentAsync(int departmentId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving users with access to department {DepartmentId}", departmentId);
            var response = await httpClient.GetAsync($"/v1/api/users/departments/{departmentId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<UserModel>>(jsonSerializerOptions, cancellationToken);
                return (users, null);
            }
            
            logger.LogWarning("Failed to retrieve users for department {DepartmentId}. Status: {StatusCode}", departmentId, response.StatusCode);
            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve users"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users for department {DepartmentId}", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving users"] });
        }
    }

    /// <summary>
    /// Get users with access to a specific organization
    /// </summary>
    /// <param name="organizationId">The ID of the organization</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of users who have access to the organization</returns>
    public async Task<(List<UserModel>? data, FormResult? formResult)> GetUsersWithAccessToOrganizationAsync(int organizationId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving users with access to organization {OrganizationId}", organizationId);
            var response = await httpClient.GetAsync($"/v1/api/users/organizations/{organizationId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<UserModel>>(jsonSerializerOptions, cancellationToken);
                return (users, null);
            }
            
            logger.LogWarning("Failed to retrieve users for organization {OrganizationId}. Status: {StatusCode}", organizationId, response.StatusCode);
            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve users"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users for organization {OrganizationId}", organizationId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving users"] });
        }
    }
    #endregion

    #region Task Points Awarded
    /// <summary>
    /// Get a list of tasks that have points awarded for a specific department
    /// </summary>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of tasks with points awarded for the department</returns>
    public async Task<(List<VTaskPointsAwarded>? data, FormResult? formResult)> GetTasksWithPointsAwardedByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving tasks with points awarded for department {DepartmentId}", departmentId);
            var response = await httpClient.GetAsync($"/v1/api/TaskPointsAwarded/ByDepartment/{departmentId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var tasksWithPoints = await response.Content.ReadFromJsonAsync<List<VTaskPointsAwarded>>(jsonSerializerOptions, cancellationToken);
                return (tasksWithPoints, null);
            }
            
            logger.LogWarning("Failed to retrieve tasks with points awarded for department {DepartmentId}. Status: {StatusCode}", departmentId, response.StatusCode);
            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve tasks with points awarded"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tasks with points awarded for department {DepartmentId}", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving tasks with points awarded"] });
        }
    }

    /// <summary>
    /// Get VTaskPointsAwarded for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of tasks with points awarded for the user</returns>
    public async Task<(List<VTaskPointsAwarded>? data, FormResult? formResult)> GetTasksWithPointsAwardedByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving tasks with points awarded for user {UserId}", userId);
            var response = await httpClient.GetAsync($"/v1/api/TaskPointsAwarded/ByUser/{userId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var tasksWithPoints = await response.Content.ReadFromJsonAsync<List<VTaskPointsAwarded>>(jsonSerializerOptions, cancellationToken);
                return (tasksWithPoints, null);
            }
            
            logger.LogWarning("Failed to retrieve tasks with points awarded for user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve tasks with points awarded"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tasks with points awarded for user {UserId}", userId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving tasks with points awarded"] });
        }
    }
    #endregion
}
