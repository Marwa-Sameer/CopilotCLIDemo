using System.ComponentModel.DataAnnotations;

namespace HouseholdNeedsManager.Models;

/// <summary>
/// Represents a household that contains items and members
/// </summary>
public class Household
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the household
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the household was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Owner's user ID (foreign key)
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the owner
    /// </summary>
    public virtual ApplicationUser? Owner { get; set; }

    /// <summary>
    /// Members of this household
    /// </summary>
    public virtual ICollection<HouseholdMember> Members { get; set; } = new List<HouseholdMember>();

    /// <summary>
    /// Items belonging to this household
    /// </summary>
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    /// <summary>
    /// Categories for this household
    /// </summary>
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
