using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseholdNeedsManager.Models;

/// <summary>
/// Represents an item in a household needs list
/// </summary>
public class Item
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the item
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Quantity needed
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Whether this item is urgent
    /// </summary>
    public bool IsUrgent { get; set; }

    /// <summary>
    /// Estimated price of the item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedPrice { get; set; }

    /// <summary>
    /// Category ID (foreign key, nullable)
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Navigation property to the category
    /// </summary>
    public virtual Category? Category { get; set; }

    /// <summary>
    /// Preferred vendor or store
    /// </summary>
    [StringLength(100)]
    public string? Vendor { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// User who created this item (foreign key)
    /// </summary>
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the creator
    /// </summary>
    public virtual ApplicationUser? CreatedBy { get; set; }

    /// <summary>
    /// Household ID (foreign key, nullable)
    /// null = personal item, not null = household item
    /// </summary>
    public int? HouseholdId { get; set; }

    /// <summary>
    /// Navigation property to the household
    /// </summary>
    public virtual Household? Household { get; set; }

    /// <summary>
    /// When the item was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the item was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
