namespace Organization.ApiService.V1;

public static class PasswordEndpoint
{
    public static void MapPasswordEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        /// <summary>
        /// Change password for a user.
        /// </summary>
        v1.MapPost("/api/users/password", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, [FromBody] ChangePasswordModel model) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var appUser = await userManager.GetUserAsync(user);
                if (appUser is not null)
                {
                    IdentityResult result = await userManager.ChangePasswordAsync(appUser, model.CurrentPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return Results.Ok(new FormResult { Succeeded = true });
                    }
                    else
                    {
                        FormResult formResult = new()
                        {
                            Succeeded = false,
                            ErrorList = [.. result.Errors.Select(e => e.Description)]
                        };
                        return Results.BadRequest(formResult);
                    }
                }
            }

            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Request password reset for a user that is not logged in.
        /// </summary>
        v1.MapPost("/api/users/password/requestOfResetPassword", async Task<IResult> (HttpContext httpContext, UserManager<AppUser> userManager, IRootDbReadWrite db, IEmailSender emailSender, CancellationToken cancellationToken, [FromBody] RequestPasswordResetModel model) =>
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Email))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Email is required."] });
            }

            var appUser = await userManager.FindByEmailAsync(model.Email.Trim());
            if (appUser is null)
            {
                return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["If the account exists, a reset email will be sent."] });
            }

            var resetRows = await db.GetRowsAsync<TResetPassword>(cancellationToken);
            var resetRow = resetRows.FirstOrDefault(r => r.AppUserId == appUser.Id);

            if (resetRow is not null && (resetRow.IsResetMailBlocked || !resetRow.CanSendResetMail))
            {
                return Results.BadRequest(new FormResult
                {
                    Succeeded = false,
                    ErrorList = ["Reset is blocked. Please contact an administrator."]
                });
            }

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(appUser);
            var requestedAtUtc = DateTimeOffset.UtcNow;

            if (resetRow is null)
            {
                resetRow = new TResetPassword
                {
                    AppUserId = appUser.Id
                };
                resetRow.RegisterResetRequest(requestedAtUtc, resetToken);
                await db.AddRowAsync(resetRow, cancellationToken);
            }
            else
            {
                resetRow.RegisterResetRequest(requestedAtUtc, resetToken);
                await db.UpdateRowAsync(resetRow, cancellationToken);
            }

            var originHeader = httpContext.Request.Headers.Origin.ToString();
            var linkBase = Uri.TryCreate(originHeader, UriKind.Absolute, out var originUri)
                ? $"{originUri.Scheme}://{originUri.Authority}"
                : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            var resetLink = $"{linkBase}/reset-my-password?userId={Uri.EscapeDataString(appUser.Id)}&token={Uri.EscapeDataString(resetToken)}";
            var hostEnvironment = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            if (!string.IsNullOrWhiteSpace(appUser.Email))
            {
                var htmlBody = $"<p>You requested to reset your password. Please click the link below to reset it:</p><p><a href=\"{WebUtility.HtmlEncode(resetLink)}\">Reset password</a></p>";
                if (!hostEnvironment.IsDevelopment())
                    await emailSender.SendAsync(appUser.Email, "Reset your password", htmlBody, cancellationToken);
            }

            var messages = new List<string> { "If the account exists, a reset email will be sent." };
            if (hostEnvironment.IsDevelopment())
            {
                messages.Add(resetLink);
            }

            return Results.Ok(new FormResult { Succeeded = true, ErrorList = [.. messages] });
        }).AllowAnonymous();

        /// <summary>
        /// Reset password for a non-authenticated user by validating user id and reset token.
        /// </summary>
        v1.MapPost("/api/users/password/reset/self", async Task<IResult> (UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken, [FromBody] SelfResetPasswordModel model) =>
        {
            if (model is null || string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.ResetToken))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["User ID and reset token are required."] });
            }

            if (!string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["The password and confirmation password do not match."] });
            }

            var appUser = await userManager.FindByIdAsync(model.UserId);
            if (appUser is null)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid password reset request."] });
            }

            var resetRow = await db.GetResetPasswordsByUserIdAsync(appUser.Id, cancellationToken);
            if (resetRow is null || !resetRow.IsMatchingResetToken(model.ResetToken))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid reset token."] });
            }

            IdentityResult result = await userManager.ResetPasswordAsync(appUser, model.ResetToken, model.Password!);
            if (!result.Succeeded)
            {
                FormResult formResult = new()
                {
                    Succeeded = false,
                    ErrorList = [.. result.Errors.Select(e => e.Description)]
                };
                return Results.BadRequest(formResult);
            }

            await db.DeleteRowAsync(resetRow, cancellationToken);
            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Password has been reset successfully."] });
        }).AllowAnonymous();

        /// <summary>
        /// Reset password for a user.
        /// </summary>
        v1.MapPost("/api/users/password/reset", async Task<IResult> (ClaimsPrincipal user, UserManager<AppUser> userManager, IRootDbReadWrite db, CancellationToken cancellationToken, [FromBody] ResetPasswordModel model) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated && !string.IsNullOrEmpty(model.UserId))
            {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin, RolesEnum.DepartmentAdmin };
                var (hasAccess, _user) = await UserRolesHelpers.IsUserInSameOrganizationAndInRoleAsync(user, model.UserId, rolesToCheck, userManager, db, cancellationToken);
                {
                    var appUser = await userManager.FindByIdAsync(model.UserId!);
                    if (appUser is not null)
                    {
                        var resetToken = await userManager.GeneratePasswordResetTokenAsync(appUser);
                        IdentityResult result = await userManager.ResetPasswordAsync(appUser, resetToken, model.Password!);
                        if (result.Succeeded)
                        {
                            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Password has been reset successfully."] });
                        }
                        else
                        {
                            FormResult formResult = new()
                            {
                                Succeeded = false,
                                ErrorList = [.. result.Errors.Select(e => e.Description)]
                            };
                            return Results.BadRequest(formResult);
                        }
                    }
                }
            }

            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        #region Endpoints for administrators to manage password reset requests
        /// <summary>
        /// Get all password reset requests for an organization
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="db">IRootDbReadWrite</param>
        /// <param name="organizationId">Organization Id</param>
        /// <returns>List of password reset requests</returns>
        v1.MapGet("/api/users/password/reset-requests/{organizationId}", async (ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken cancellationToken, int organizationId) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin };
                if (await UserRolesHelpers.IsUserAuthorizedForOrganizationAsync(user, organizationId, 0, rolesToCheck, db, cancellationToken))
                {
                    var resetRequests = await db.GetResetPasswordsByOrganizationIdAsync(organizationId, cancellationToken);
                    if (resetRequests is null || !resetRequests.Any())
                    {
                        return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["No password reset requests found for this organization."] });
                    }
                    return Results.Ok(resetRequests);
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();

        /// <summary>
        /// Delete a password reset request for an organization by reset request id.
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <param name="db">IRootDbReadWrite</param>
        /// <param name="organizationId">Organization Id</param>
        /// <param name="id">Reset request Id</param>
        /// <returns>Result</returns>
        v1.MapDelete("/api/users/password/reset-requests/{organizationId}/{id:int}", async (ClaimsPrincipal user, IRootDbReadWrite db, CancellationToken cancellationToken, int organizationId, int id) =>
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var rolesToCheck = new[] { RolesEnum.OrganizationAdmin, RolesEnum.EnterpriseAdmin };
                if (await UserRolesHelpers.IsUserAuthorizedForOrganizationAsync(user, organizationId, 0, rolesToCheck, db, cancellationToken))
                {
                    var resetRequests = await db.GetResetPasswordsByOrganizationIdAsync(organizationId, cancellationToken);
                    var resetRequest = resetRequests.FirstOrDefault(r => r.Id == id);
                    if (resetRequest is null)
                    {
                        return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Password reset request not found for this organization."] });
                    }

                    var row = await db.GetRowAsync<TResetPassword>(id, cancellationToken);
                    if (row is null)
                    {
                        return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Password reset request not found."] });
                    }

                    await db.DeleteRowAsync(row, cancellationToken);
                    return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Password reset request deleted successfully."] });
                }
            }
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }).RequireAuthorization();
        #endregion
    }
}
