using System.ComponentModel.DataAnnotations;

namespace HouseholdNeedsManager.Models;

/// <summary>
/// Represents a category for items within a household
/// </summary>
public class Category
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the category
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Household ID (foreign key) - categories are household-specific
    /// </summary>
    public int HouseholdId { get; set; }

    /// <summary>
    /// Navigation property to the household
    /// </summary>
    public virtual Household? Household { get; set; }

    /// <summary>
    /// Items in this category
    /// </summary>
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
