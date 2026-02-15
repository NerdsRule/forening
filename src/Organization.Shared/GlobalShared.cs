global using System;
global using Microsoft.AspNetCore.Identity;
global using System.Text.RegularExpressions;
global using System.Text;
global using System.Text.Json;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;

global using Organization.Shared.Identity;
global using Organization.Shared.DatabaseObjects;

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
    VerifiedCompleted = 3
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