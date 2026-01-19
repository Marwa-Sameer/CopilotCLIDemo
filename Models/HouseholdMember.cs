using System.ComponentModel.DataAnnotations;

namespace HouseholdNeedsManager.Models;

/// <summary>
/// Join table for many-to-many relationship between users and households
/// </summary>
public class HouseholdMember
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User ID (foreign key)
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Household ID (foreign key)
    /// </summary>
    public int HouseholdId { get; set; }

    /// <summary>
    /// Navigation property to the household
    /// </summary>
    public virtual Household? Household { get; set; }

    /// <summary>
    /// Role of the user in this household
    /// </summary>
    public HouseholdRole Role { get; set; } = HouseholdRole.Member;

    /// <summary>
    /// When the user joined this household
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
