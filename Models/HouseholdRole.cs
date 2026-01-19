namespace HouseholdNeedsManager.Models;

/// <summary>
/// Roles for household members
/// </summary>
public enum HouseholdRole
{
    /// <summary>
    /// Owner of the household (full permissions)
    /// </summary>
    Owner,
    
    /// <summary>
    /// Administrator with elevated permissions
    /// </summary>
    Admin,
    
    /// <summary>
    /// Regular member
    /// </summary>
    Member
}
