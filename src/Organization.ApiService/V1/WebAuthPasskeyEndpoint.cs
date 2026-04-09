namespace Organization.ApiService.V1;

public static class WebAuthPasskeyEndpoint
{
    private const string WebAuthnRegisterCachePrefix = "webauthn:register:";
    private const string WebAuthnLoginCachePrefix = "webauthn:login:";

    public static void MapWebAuthPasskeyEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        v1.MapPost("/api/users/webauthn/register/begin", async Task<IResult> (HttpContext httpContext, ClaimsPrincipal user, UserManager<AppUser> userManager, AppDbContext appDbContext, IMemoryCache memoryCache, CancellationToken cancellationToken) =>
        {
            var userId = UserRolesHelpers.GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var appUser = await userManager.FindByIdAsync(userId);
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var existingCredentials = await appDbContext.FidoCredentials
                .Where(c => c.AppUserId == userId)
                .Select(c => new PublicKeyCredentialDescriptor(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(c.CredentialId)))
                .ToListAsync(cancellationToken);

            var fido2 = UserRolesHelpers.GetFido2(httpContext);
            var fidoUser = new Fido2User
            {
                DisplayName = appUser.DisplayName ?? appUser.UserName ?? appUser.Email ?? userId,
                Name = appUser.Email ?? appUser.UserName ?? userId,
                Id = Encoding.UTF8.GetBytes(userId)
            };

            var options = fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fidoUser,
                ExcludeCredentials = existingCredentials,
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    ResidentKey = ResidentKeyRequirement.Preferred,
                    UserVerification = UserVerificationRequirement.Preferred
                },
                AttestationPreference = AttestationConveyancePreference.None,
                Extensions = new AuthenticationExtensionsClientInputs
                {
                    CredProps = true
                }
            });

            var requestId = Guid.NewGuid().ToString("N");
            memoryCache.Set($"{WebAuthnRegisterCachePrefix}{requestId}", new WebAuthnRequestState
            {
                UserId = userId,
                OptionsJson = options.ToJson()
            }, TimeSpan.FromMinutes(5));

            return Results.Ok(new WebAuthnBeginPasskeyRegistrationResult
            {
                RequestId = requestId,
                Options = JsonSerializer.Deserialize<JsonElement>(options.ToJson())
            });
        }).RequireAuthorization();

        v1.MapPost("/api/users/webauthn/register/complete", async Task<IResult> (HttpContext httpContext, ClaimsPrincipal user, UserManager<AppUser> userManager, AppDbContext appDbContext, IMemoryCache memoryCache, [FromBody] WebAuthnCompleteRequest model, CancellationToken cancellationToken) =>
        {
            var userId = UserRolesHelpers.GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (model is null || string.IsNullOrWhiteSpace(model.RequestId) || string.IsNullOrWhiteSpace(model.CredentialJson))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["RequestId and credential payload are required."] });
            }

            if (!memoryCache.TryGetValue($"{WebAuthnRegisterCachePrefix}{model.RequestId}", out WebAuthnRequestState? requestState) || requestState is null)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey registration request expired. Please try again."] });
            }

            memoryCache.Remove($"{WebAuthnRegisterCachePrefix}{model.RequestId}");
            if (!string.Equals(requestState.UserId, userId, StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }

            try
            {
                var options = CredentialCreateOptions.FromJson(requestState.OptionsJson);
                var attestationResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(model.CredentialJson);
                if (attestationResponse is null)
                {
                    return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid passkey attestation payload."] });
                }

                var fido2 = UserRolesHelpers.GetFido2(httpContext);
                IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, _) =>
                {
                    var credentialId = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(args.CredentialId);
                    return !await appDbContext.FidoCredentials.AnyAsync(c => c.CredentialId == credentialId, cancellationToken);
                };

                var credentialResult = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = attestationResponse,
                    OriginalOptions = options,
                    IsCredentialIdUniqueToUserCallback = callback
                });

                var newCredential = new TFidoCredential
                {
                    AppUserId = userId,
                    CredentialId = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(credentialResult.Id),
                    PublicKey = Convert.ToBase64String(credentialResult.PublicKey),
                    UserHandle = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(options.User.Id),
                    SignatureCounter = credentialResult.SignCount,
                    FriendlyName = string.IsNullOrWhiteSpace(model.FriendlyName) ? $"Passkey {DateTime.Now:yyyy-MM-dd}" : model.FriendlyName.Trim(),
                    CredentialType = credentialResult.Type.ToString(),
                    Transports = credentialResult.Transports is null ? null : string.Join(',', credentialResult.Transports)
                };

                await appDbContext.FidoCredentials.AddAsync(newCredential, cancellationToken);
                await appDbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Passkey registered successfully."] });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey registration failed.", ex.Message] });
            }
        }).RequireAuthorization();

        v1.MapGet("/api/users/webauthn/credentials", async Task<IResult> (ClaimsPrincipal user, AppDbContext appDbContext, CancellationToken cancellationToken) =>
        {
            var userId = UserRolesHelpers.GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var credentials = (await appDbContext.FidoCredentials
                .Where(c => c.AppUserId == userId)
                .Select(c => new WebAuthnCredentialModel
                {
                    Id = c.Id,
                    FriendlyName = c.FriendlyName,
                    CredentialType = c.CredentialType,
                    CredentialHint = c.CredentialId.Length <= 12
                        ? c.CredentialId
                        : $"{c.CredentialId.Substring(0, 8)}...{c.CredentialId.Substring(c.CredentialId.Length - 4)}",
                    Fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(c.CredentialId))).Substring(0, 10),
                    Transports = c.Transports,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(cancellationToken))
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return Results.Ok(credentials);
        }).RequireAuthorization();

        v1.MapDelete("/api/users/webauthn/credentials/{credentialId:int}", async Task<IResult> (ClaimsPrincipal user, AppDbContext appDbContext, int credentialId, CancellationToken cancellationToken) =>
        {
            var userId = UserRolesHelpers.GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var credential = await appDbContext.FidoCredentials
                .FirstOrDefaultAsync(c => c.Id == credentialId && c.AppUserId == userId, cancellationToken);
            if (credential is null)
            {
                return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Passkey not found."] });
            }

            appDbContext.FidoCredentials.Remove(credential);
            await appDbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Passkey removed."] });
        }).RequireAuthorization();

        v1.MapPut("/api/users/webauthn/credentials/{credentialId:int}/friendly-name", async Task<IResult> (ClaimsPrincipal user, AppDbContext appDbContext, int credentialId, [FromBody] WebAuthnUpdateFriendlyNameRequest model, CancellationToken cancellationToken) =>
        {
            var userId = UserRolesHelpers.GetAuthenticatedUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (model is null || string.IsNullOrWhiteSpace(model.FriendlyName))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Friendly name is required."] });
            }

            var friendlyName = model.FriendlyName.Trim();
            if (friendlyName.Length > 100)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Friendly name cannot exceed 100 characters."] });
            }

            var credential = await appDbContext.FidoCredentials
                .FirstOrDefaultAsync(c => c.Id == credentialId && c.AppUserId == userId, cancellationToken);
            if (credential is null)
            {
                return Results.NotFound(new FormResult { Succeeded = false, ErrorList = ["Passkey not found."] });
            }

            credential.FriendlyName = friendlyName;
            appDbContext.FidoCredentials.Update(credential);
            await appDbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(new FormResult { Succeeded = true, ErrorList = ["Friendly name updated."] });
        }).RequireAuthorization();

        v1.MapPost("/api/users/webauthn/login/begin", async Task<IResult> (HttpContext httpContext, UserManager<AppUser> userManager, AppDbContext appDbContext, IMemoryCache memoryCache, [FromBody] WebAuthnBeginPasskeyLoginRequest model, CancellationToken cancellationToken) =>
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Email))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Email is required."] });
            }

            var appUser = await userManager.FindByEmailAsync(model.Email.Trim());
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var allowedCredentials = await appDbContext.FidoCredentials
                .Where(c => c.AppUserId == appUser.Id)
                .Select(c => new PublicKeyCredentialDescriptor(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(c.CredentialId)))
                .ToListAsync(cancellationToken);

            if (allowedCredentials.Count == 0)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["No passkey is registered for this account."] });
            }

            var fido2 = UserRolesHelpers.GetFido2(httpContext);
            var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                AllowedCredentials = allowedCredentials,
                UserVerification = UserVerificationRequirement.Preferred,
                Extensions = new AuthenticationExtensionsClientInputs
                {
                    Extensions = true
                }
            });

            var requestId = Guid.NewGuid().ToString("N");
            memoryCache.Set($"{WebAuthnLoginCachePrefix}{requestId}", new WebAuthnRequestState
            {
                UserId = appUser.Id,
                OptionsJson = options.ToJson()
            }, TimeSpan.FromMinutes(5));

            return Results.Ok(new WebAuthnBeginPasskeyRegistrationResult
            {
                RequestId = requestId,
                Options = JsonSerializer.Deserialize<JsonElement>(options.ToJson())
            });
        }).AllowAnonymous();

        v1.MapPost("/api/users/webauthn/login/complete", async Task<IResult> (HttpContext httpContext, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IRootDbReadWrite db, AppDbContext appDbContext, IMemoryCache memoryCache, [FromBody] WebAuthnCompleteRequest model, CancellationToken cancellationToken) =>
        {
            if (model is null || string.IsNullOrWhiteSpace(model.RequestId) || string.IsNullOrWhiteSpace(model.CredentialJson))
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["RequestId and credential payload are required."] });
            }

            if (!memoryCache.TryGetValue($"{WebAuthnLoginCachePrefix}{model.RequestId}", out WebAuthnRequestState? requestState) || requestState is null)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey login request expired. Please try again."] });
            }

            memoryCache.Remove($"{WebAuthnLoginCachePrefix}{model.RequestId}");

            var assertionResponse = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(model.CredentialJson);
            if (assertionResponse is null)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Invalid passkey assertion payload."] });
            }

            var credential = await appDbContext.FidoCredentials
                .FirstOrDefaultAsync(c => c.CredentialId == assertionResponse.Id && c.AppUserId == requestState.UserId, cancellationToken);
            if (credential is null)
            {
                return Results.Unauthorized();
            }

            var appUser = await userManager.FindByIdAsync(requestState.UserId);
            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            try
            {
                var options = AssertionOptions.FromJson(requestState.OptionsJson);
                var fido2 = UserRolesHelpers.GetFido2(httpContext);

                IsUserHandleOwnerOfCredentialIdAsync callback = async (args, _) =>
                {
                    var userHandle = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(args.UserHandle);
                    return await appDbContext.FidoCredentials.AnyAsync(c =>
                        c.UserHandle == userHandle &&
                        c.CredentialId == Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(args.CredentialId) &&
                        c.AppUserId == requestState.UserId,
                        cancellationToken);
                };

                var assertionResult = await fido2.MakeAssertionAsync(new MakeAssertionParams
                {
                    AssertionResponse = assertionResponse,
                    OriginalOptions = options,
                    StoredPublicKey = Convert.FromBase64String(credential.PublicKey),
                    StoredSignatureCounter = credential.SignatureCounter,
                    IsUserHandleOwnerOfCredentialIdCallback = callback
                });

                credential.SignatureCounter = assertionResult.SignCount;
                appDbContext.FidoCredentials.Update(credential);
                await appDbContext.SaveChangesAsync(cancellationToken);

                await signInManager.SignInAsync(appUser, isPersistent: false);
                var userInfo = await UserRolesHelpers.GetUserInfoAsync(appUser.Id, userManager, db, cancellationToken);
                if (userInfo is null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(userInfo);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new FormResult { Succeeded = false, ErrorList = ["Passkey login failed.", ex.Message] });
            }
        }).AllowAnonymous();
    }
}
