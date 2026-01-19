using Microsoft.AspNetCore.Identity;

namespace HouseholdNeedsManager.Models;

/// <summary>
/// Extended ApplicationUser with navigation properties for households and items
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Households where this user is a member
    /// </summary>
    public virtual ICollection<HouseholdMember> HouseholdMemberships { get; set; } = new List<HouseholdMember>();

    /// <summary>
    /// Items created by this user
    /// </summary>
    public virtual ICollection<Item> CreatedItems { get; set; } = new List<Item>();

    /// <summary>
    /// Households owned by this user
    /// </summary>
    public virtual ICollection<Household> OwnedHouseholds { get; set; } = new List<Household>();
}
