global using System;
global using Microsoft.AspNetCore.Identity;
global using System.Text.RegularExpressions;
global using System.Text;
global using System.Text.Json;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;

global using Organization.Shared.Identity;
global using Organization.Shared.DatabaseObjects;
global using Organization.Shared.Helpers;
global using Organization.Shared.Interfaces;
global using Organization.Shared;

namespace Organization.Shared;

/// <summary>
/// Defines roles that users can have within a department or organization.
/// </summary>
public enum RolesEnum
{
    /// <summary>
    /// User with department-wide administrative privileges.
    /// </summary>
    DepartmentAdmin = 0,
    /// <summary>
    /// Read-only or external collaborator with limited access to department resources.
    /// </summary>
    DepartmentMember = 1,
    /// <summary>
    /// Regular member of the department with standard access rights.
    /// </summary>
    OrganizationMember = 10,
    /// <summary>
    /// User with organization-wide administrative privileges.
    /// </summary>
    OrganizationAdmin = 11,
    /// <summary>
    /// User with enterprise-wide administrative privileges.
    /// </summary>
    EnterpriseAdmin = 12,
    /// <summary>
    /// No specific role assigned.
    /// </summary>
    None = 99
}

/// <summary>
/// Defines the status of a task within a department.
/// </summary>
public enum TaskStatusEnum
{
    /// <summary>
    /// Task is not yet started.
    /// </summary>
    NotStarted = 0,
    /// <summary> Task is currently in progress.
    /// </summary>
    InProgress = 1,
    /// <summary> Task is completed.
    /// </summary>
    Completed = 2,
    /// <summary> 
    /// Task is verified as completed and approved.
    /// </summary>
    VerifiedCompleted = 3,
    /// <summary>
    /// Task was rejected and needs to be redone or revised.
    /// </summary>
    Rejected = 4
}

/// <summary>
/// Defines the status of a prize within the organization.
/// </summary>
public enum PrizeStatusEnum
{
    /// <summary> Prize is available for redemption.
    /// </summary>
    Available = 0,
    /// <summary> Prize is currently being redeemed by a user.
    /// </summary>
    PendingRedemption = 1,
    /// <summary> Prize has been redeemed and is no longer available.
    /// </summary>
    Redeemed = 2
}

public static class GlobalShared
{
    /// <summary>
    /// Api version number. This is used by the frontend to verify that the backend API is compatible with the expected version. It can be incremented when breaking changes are made to the API.
    /// </summary>
    public const string ApiVersion = "0.5.0";

    /// <summary>
    /// Blazor version number. This is used to verify that the frontend Blazor is up to date.
    /// </summary>
    public const string BlazorVersion = "0.17.0";  //Guid.NewGuid().ToString();
}