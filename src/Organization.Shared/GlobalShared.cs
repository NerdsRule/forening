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
/// Defines roles that users can have within a department.
/// </summary>
public enum DepartmentRolesEnum
{
    /// <summary>
    /// User with department-wide administrative privileges.
    /// </summary>
    DepartmentAdmin,
    /// <summary>
    /// Read-only or external collaborator with limited access to department resources.
    /// </summary>
    Member
}

/// <summary>
/// Defines roles that users can have within an organization.
/// </summary>
public enum OrganizationRolesEnum
{
    /// <summary>
    /// User with enterprise-wide administrative privileges.
    /// </summary>
    EnterpriseAdmin,
    /// <summary>
    /// User with administrative privileges within a specific organization.
    /// </summary>
    Admin,
    /// <summary>
    /// Regular member of the organization with standard access rights.
    /// </summary>
    Member
}
