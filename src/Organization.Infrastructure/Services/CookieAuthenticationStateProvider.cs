
using Blazored.LocalStorage;

namespace Organization.Infrastructure.Services;

    /// <summary>
    /// Handles state for cookie-based auth.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the auth provider.
    /// </remarks>
    /// <param name="httpClientFactory">Factory to retrieve auth client.</param>
    public class CookieAuthenticationStateProvider(IHttpClientFactory httpClientFactory, ILogger<CookieAuthenticationStateProvider> logger, ILocalStorageService localStorageService) : AuthenticationStateProvider, IAccountService
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

        private readonly ILocalStorageService _localStorageService = localStorageService;

        /// <summary>
        /// Authentication state.
        /// </summary>
        private bool authenticated = false;

        /// <summary>
        /// Default principal for anonymous (not authenticated) users.
        /// </summary>
        private readonly ClaimsPrincipal unauthenticated = new(new ClaimsIdentity());

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The result serialized to a <see cref="FormResult"/>.
        /// </returns>
        public async Task<FormResult> RegisterAsync(RegisterModel model)
        {
            string[] defaultDetail = [ "An unknown error prevented registration from succeeding." ];

            try
            {
                // make the request
                var result = await httpClient.PostAsJsonAsync("/v1/api/users/register", model);

                // successful?
                if (result.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }

                // body should contain details about why it failed
                var details = await result.Content.ReadAsStringAsync();
                var problemDetails = JsonDocument.Parse(details);
                var errors = new List<string>();
                var errorList = problemDetails.RootElement.GetProperty("errors");

                foreach (var errorEntry in errorList.EnumerateObject())
                {
                    if (errorEntry.Value.ValueKind == JsonValueKind.String)
                    {
                        errors.Add(errorEntry.Value.GetString()!);
                    }
                    else if (errorEntry.Value.ValueKind == JsonValueKind.Array)
                    {
                        errors.AddRange(
                            errorEntry.Value.EnumerateArray().Select(
                                e => e.GetString() ?? string.Empty)
                            .Where(e => !string.IsNullOrEmpty(e)));
                    }
                }

                // return the error list
                return new FormResult
                {
                    Succeeded = false,
                    ErrorList = problemDetails == null ? defaultDetail : [.. errors]
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "App error");
            }

            // unknown error
            return new FormResult
            {
                Succeeded = false,
                ErrorList = defaultDetail
            };
        }

        /// <summary>
        /// User login.
        /// </summary>
        /// <param name="model">The login model containing email and password.</param>
        /// <returns>The result of the login request serialized to a <see cref="FormResult"/>.</returns>
        public async Task<FormResult> LoginAsync(LoginModel model)
        {
            var email = model.Email ?? string.Empty;
            var password = model.Password ?? string.Empty;

            // make the request
            try
            {
                CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
                // login with cookies
                var result = await httpClient.PostAsJsonAsync("/v1/api/users/login", model, cancellationToken);

                // success?
                if (result.IsSuccessStatusCode)
                {
                    // need to refresh auth state
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    // success!
                    return new FormResult { Succeeded = true };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "App error");
            }

            // unknown error
            return new FormResult
            {
                Succeeded = false,
                ErrorList = [ "Invalid email and/or password." ]
            };
        }
        

        /// <summary>
        /// Change the user's password.
        /// </summary>
        /// <param name="model">The change password model containing current and new password.</param>
        /// <returns>The result of the password change request serialized to a <see cref="FormResult"/>.</returns>
        public async Task<FormResult> ChangePasswordAsync(ChangePasswordModel model)
        {
            try
            {
                // make the request
                var result = await httpClient.PostAsJsonAsync("/v1/api/users/password", model);

                // success?
                if (result.IsSuccessStatusCode)
                {
                    // success!
                    return new FormResult { Succeeded = true };
                }

                // body should contain details about why it failed
                var details = await result.Content.ReadAsStringAsync();
                var problemDetails = JsonHelpers.JsonDeSerialize<FormResult>(details);
                return problemDetails ?? new FormResult { Succeeded = false, ErrorList = [ "An unknown error prevented the password change from succeeding." ] };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "App error");
            }

            // unknown error
            return new FormResult
            {
                Succeeded = false,
                ErrorList = [ "An unknown error prevented the password change from succeeding." ]
            };
        }

        /// <summary>
        /// Get authentication state.
        /// </summary>
        /// <remarks>
        /// Called by Blazor anytime and authentication-based decision needs to be made, then cached
        /// until the changed state notification is raised.
        /// </remarks>
        /// <returns>The authentication state asynchronous request.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            authenticated = false;

            // default to not authenticated
            var user = unauthenticated;
            StaticUserInfoBlazor.User = null;
            StaticUserInfoBlazor.SelectedOrganization = null;
            StaticUserInfoBlazor.SelectedDepartment = null;
            try
            {
                // the user info endpoint is secured, so if the user isn't logged in this will fail
                CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
                var response = await httpClient.GetAsync("v1/api/users/info", cancellationToken);
                //var userInfo = await httpClient.GetFromJsonAsync<UserModel?>("v1/api/users/info", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<UserModel>(jsonSerializerOptions, cancellationToken);
                    if (userInfo == null)
                    {
                        return new AuthenticationState(unauthenticated);
                    }
                    // in this example app, name and email are the same
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, userInfo.UserName),
                        new(ClaimTypes.Email, userInfo.Email),
                    };

                    StaticUserInfoBlazor.User = userInfo;
                    // Read user settings from local storage
                    try
                    {
                        var userLocalStorage = await _localStorageService.GetItemAsync<UserLocalStorage>(StaticUserInfoBlazor.UserLocalStorageKey);
                        if (userLocalStorage != null)
                    {
                        if (userLocalStorage.SelectedOrganizationId != 0)
                        {
                            StaticUserInfoBlazor.SelectedOrganization = userInfo.AppUserOrganizations.FirstOrDefault(o => o.OrganizationId == userLocalStorage.SelectedOrganizationId);
                            if (StaticUserInfoBlazor.SelectedOrganization != null)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, StaticUserInfoBlazor.OrganizationRole.ToString()));
                            }
                            else
                            {
                                StaticUserInfoBlazor.SelectedOrganization = userInfo.AppUserOrganizations.FirstOrDefault();
                                if (StaticUserInfoBlazor.SelectedOrganization != null)
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, StaticUserInfoBlazor.OrganizationRole.ToString()));
                                }
                            }
                        }
                        if (userLocalStorage.SelectedDepartmentId != 0)
                        {
                            StaticUserInfoBlazor.SelectedDepartment = userInfo.AppUserDepartments.FirstOrDefault(d => d.DepartmentId == userLocalStorage.SelectedDepartmentId);
                            if (StaticUserInfoBlazor.SelectedDepartment != null)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, StaticUserInfoBlazor.DepartmentRole.ToString()));
                            }
                            else
                            {
                                StaticUserInfoBlazor.SelectedDepartment = userInfo.AppUserDepartments.FirstOrDefault();
                                if (StaticUserInfoBlazor.SelectedDepartment != null)
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, StaticUserInfoBlazor.DepartmentRole.ToString()));
                                }
                            }
                        }
                    } else {
                        // store static user info for Blazor client
                        if (userInfo.AppUserOrganizations.Count > 0)
                        {
                            StaticUserInfoBlazor.SelectedOrganization = userInfo.AppUserOrganizations[0];
                            claims.Add(new Claim(ClaimTypes.Role, StaticUserInfoBlazor.OrganizationRole.ToString()));
                        }
                        if (userInfo.AppUserDepartments.Count > 0)
                        {
                            StaticUserInfoBlazor.SelectedDepartment = userInfo.AppUserDepartments[0];
                            claims.Add(new Claim(ClaimTypes.Role, StaticUserInfoBlazor.DepartmentRole.ToString()));
                        }
                    }
                    } catch (Exception ex)
                    {
                        logger.LogError(ex, "Error reading user local storage");
                        await _localStorageService.RemoveItemAsync(StaticUserInfoBlazor.UserLocalStorageKey);
                    }
                    
                    

                    // set the principal
                    var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                    user = new ClaimsPrincipal(id);
                    authenticated = true;
                } else
                {
                    // not authenticated
                    return new AuthenticationState(unauthenticated);
                }
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // not authenticated - this is expected if the user is not logged in
                logger.LogInformation("User is not authenticated.");
                return new AuthenticationState(unauthenticated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "App error");
                return new AuthenticationState(unauthenticated);
            }

            // return the state
            return new AuthenticationState(user);
        }

        public async Task LogoutAsync()
        {
            const string Empty = "{}";
            var emptyContent = new StringContent(Empty, Encoding.UTF8, "application/json");
            await httpClient.PostAsync("/v1/api/users/logout", emptyContent);
            //NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        /// <returns>True if authenticated</returns>
        public async Task<bool> CheckAuthenticatedAsync()
        {
            await GetAuthenticationStateAsync();
            return authenticated;
        }

        /// <summary>
        /// Get the roles for a user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Array of roles</returns>
        public async Task<string[]> GetUserRolesAsync(string userId)
        {
            var rolesResponse = await httpClient.GetAsync($"/v1/api/users/{userId}/roles");
            rolesResponse.EnsureSuccessStatusCode();
            var rolesJson = await rolesResponse.Content.ReadAsStringAsync();
            var roles = JsonSerializer.Deserialize<string[]>(rolesJson, jsonSerializerOptions);
            return roles ?? [];
        }

        /// <summary>
        /// Get all available roles.
        /// </summary>
        /// <returns>Array of roles</returns>
        public async Task<string[]> GetAllRolesAsync()
        {
            var rolesResponse = await httpClient.GetAsync("/v1/api/roles/all");
            rolesResponse.EnsureSuccessStatusCode();
            var rolesJson = await rolesResponse.Content.ReadAsStringAsync();
            var roles = JsonSerializer.Deserialize<string[]>(rolesJson, jsonSerializerOptions);
            return roles ?? [];
        }

        /// <summary>
        /// Add roles to a user.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="roles">Array of roles</param>
        /// <returns>True if successful</returns>
        public async Task<bool> AddRolesToUserAsync(string userId, string[] roles)
        {
            var response = await httpClient.PostAsJsonAsync($"/v1/api/users/{userId}/roles", roles);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Remove roles from a user.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="roles">Array of roles</param>
        /// <returns>True if successful</returns>
        public async Task<bool> RemoveRolesFromUserAsync(string userId, string[] roles)
        {
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/v1/api/users/{userId}/roles")
            {
                Content = JsonContent.Create(roles)
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of UserModel</returns>
        public async Task<List<UserModel>> GetUsersAsync()
        {
            var usersResponse = await httpClient.GetAsync("/v1/api/users");
            usersResponse.EnsureSuccessStatusCode();
            var usersJson = await usersResponse.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserModel>>(usersJson, jsonSerializerOptions);
            return users ?? [];
        }

        /// <summary>
        /// Get user by Id
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>UserModel</returns>
        public async Task<UserModel?> GetUserByIdAsync(string userId)
        {
            var userResponse = await httpClient.GetAsync($"/v1/api/users/{userId}");
            if (!userResponse.IsSuccessStatusCode)
            {
                return null;
            }
            var userJson = await userResponse.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserModel>(userJson, jsonSerializerOptions);
            return user;
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>True if successful</returns>
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var response = await httpClient.DeleteAsync($"/v1/api/users/{userId}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="user">User model</param>
        /// <returns>FormResult</returns>
        public async Task<FormResult> UpdateUserAsync(UserModel user)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"/v1/api/users/", user);
                if (response.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                } else if (response.StatusCode != System.Net.HttpStatusCode.Forbidden)
                {
                    // Get FormResult from response and return it
                    var details = await response.Content.ReadAsStringAsync();
                    var problemDetails = JsonHelpers.JsonDeSerialize<FormResult>(details);
                    return problemDetails ?? new FormResult { Succeeded = false, ErrorList = [ "An unknown error prevented the user update from succeeding." ] };
                    
                }
                    return new FormResult { Succeeded = false, ErrorList = [ "Forbidden" ] };
            }
            catch (Exception ex)
            {
                return new FormResult { Succeeded = false, ErrorList = [ ex.Message ] };
            }
        }

        #region TAppUserOrganization
        /// <summary>
        /// Add or update TAppUserOrganization
        /// </summary>
        /// <param name="appUserOrganization">TAppUserOrganization model</param>
        /// <returns>Updated TAppUserOrganization</returns>
        public async Task<(TAppUserOrganization?, FormResult?)> AddUpdateAppUserOrganizationAsync(TAppUserOrganization appUserOrganization)
        {
            var response = await httpClient.PostAsJsonAsync($"/v1/api/AppUserOrganization", appUserOrganization);
            if (response.IsSuccessStatusCode)
            {
                var updatedJson = await response.Content.ReadAsStringAsync();
                var updated = JsonSerializer.Deserialize<TAppUserOrganization>(updatedJson, jsonSerializerOptions);
                return (updated, null);
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var error = JsonSerializer.Deserialize<FormResult>(errorJson, jsonSerializerOptions);
                return (null, error);
            }
        }
        /// <summary>
        /// Delete TAppUserOrganization
        /// </summary>
        /// <param name="appUserOrganization">TAppUserOrganization model</param>
        /// <returns>FormResult</returns>
        public async Task<FormResult> DeleteAppUserOrganizationAsync(TAppUserOrganization appUserOrganization)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"/v1/api/AppUserOrganization/{appUserOrganization.Id}");
                if (response.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    var error = JsonSerializer.Deserialize<FormResult>(errorJson, jsonSerializerOptions);
                    return error ?? new FormResult { Succeeded = false, ErrorList = [ "An unknown error occurred while deleting the user organization." ] };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting AppUserOrganization");
                return new FormResult { Succeeded = false, ErrorList = [ ex.Message ] };
            }
        }
        #endregion

        #region TAppUserDepartment
        /// <summary>
        /// Add or update TAppUserDepartment
        /// </summary>
        /// <param name="appUserDepartment">TAppUserDepartment model</param>
        /// <returns>Updated TAppUserDepartment</returns>
        public async Task<(TAppUserDepartment?, FormResult?)> AddUpdateAppUserDepartmentAsync(TAppUserDepartment appUserDepartment)
        {
            var response = await httpClient.PostAsJsonAsync($"/v1/api/AppUserDepartment", appUserDepartment);
            if (response.IsSuccessStatusCode)
            {
                var updatedJson = await response.Content.ReadAsStringAsync();
                var updated = JsonSerializer.Deserialize<TAppUserDepartment>(updatedJson, jsonSerializerOptions);
                return (updated, null);
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var error = JsonSerializer.Deserialize<FormResult>(errorJson, jsonSerializerOptions);
                return (null, error);
            }
        }
        /// <summary>
        /// Delete TAppUserDepartment
        /// </summary>
        /// <param name="appUserDepartment">TAppUserDepartment model</param>
        /// <returns>FormResult</returns>
        public async Task<FormResult> DeleteAppUserDepartmentAsync(TAppUserDepartment appUserDepartment)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"/v1/api/AppUserDepartment/{appUserDepartment.Id}");
                if (response.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    var error = JsonSerializer.Deserialize<FormResult>(errorJson, jsonSerializerOptions);
                    return error ?? new FormResult { Succeeded = false, ErrorList = [ "An unknown error occurred while deleting the user department." ] };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting AppUserDepartment");
                return new FormResult { Succeeded = false, ErrorList = [ ex.Message ] };
            }
        }
        #endregion
    }