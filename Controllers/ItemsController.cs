using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseholdNeedsManager.Data;
using HouseholdNeedsManager.Models;
using HouseholdNeedsManager.Services;

namespace HouseholdNeedsManager.Controllers;

[Authorize]
public class ItemsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHouseholdContextService _householdContext;

    public ItemsController(ApplicationDbContext context, IHouseholdContextService householdContext)
    {
        _context = context;
        _householdContext = householdContext;
    }

    // GET: Items
    public async Task<IActionResult> Index(string? filter, int? categoryId, bool? urgent, string? search)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var activeHouseholdId = _householdContext.GetActiveHouseholdId();

        IQueryable<Item> query = _context.Items
            .Include(i => i.Category)
            .Include(i => i.CreatedBy)
            .Include(i => i.Household);

        // Filter by personal vs household items
        if (filter == "personal")
        {
            query = query.Where(i => i.HouseholdId == null && i.CreatedByUserId == userId);
        }
        else if (filter == "household" && activeHouseholdId.HasValue)
        {
            query = query.Where(i => i.HouseholdId == activeHouseholdId.Value);
        }
        else
        {
            // Default: show both personal items and active household items
            if (activeHouseholdId.HasValue)
            {
                query = query.Where(i => 
                    (i.HouseholdId == null && i.CreatedByUserId == userId) || 
                    i.HouseholdId == activeHouseholdId.Value);
            }
            else
            {
                query = query.Where(i => i.HouseholdId == null && i.CreatedByUserId == userId);
            }
        }

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        // Filter by urgency
        if (urgent.HasValue)
        {
            query = query.Where(i => i.IsUrgent == urgent.Value);
        }

        // Search by name
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search));
        }

        var items = await query.OrderByDescending(i => i.IsUrgent)
            .ThenByDescending(i => i.CreatedAt)
            .ToListAsync();

        // Get categories for filter dropdown
        var categories = new List<Category>();
        if (activeHouseholdId.HasValue)
        {
            categories = await _context.Categories
                .Where(c => c.HouseholdId == activeHouseholdId.Value)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        ViewBag.Categories = categories;
        ViewBag.ActiveHouseholdId = activeHouseholdId;
        ViewBag.CurrentFilter = filter;
        ViewBag.CurrentCategoryId = categoryId;
        ViewBag.CurrentUrgent = urgent;
        ViewBag.CurrentSearch = search;

        return View(items);
    }

    // GET: Items/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var item = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.CreatedBy)
            .Include(i => i.Household)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        // Check authorization
        if (!await CanAccessItem(item, userId))
        {
            return Forbid();
        }

        return View(item);
    }

    // GET: Items/Create
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var activeHouseholdId = _householdContext.GetActiveHouseholdId();
        var categories = new List<Category>();

        if (activeHouseholdId.HasValue)
        {
            categories = await _context.Categories
                .Where(c => c.HouseholdId == activeHouseholdId.Value)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        ViewBag.Categories = categories;
        ViewBag.ActiveHouseholdId = activeHouseholdId;

        return View();
    }

    // POST: Items/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Quantity,IsUrgent,EstimatedPrice,CategoryId,Vendor,Notes,HouseholdId")] Item item)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        // Validate household assignment
        if (item.HouseholdId.HasValue)
        {
            var isMember = await _context.HouseholdMembers
                .AnyAsync(hm => hm.HouseholdId == item.HouseholdId.Value && hm.UserId == userId);

            if (!isMember)
            {
                ModelState.AddModelError("HouseholdId", "You are not a member of this household.");
            }
        }

        if (ModelState.IsValid)
        {
            item.CreatedByUserId = userId;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            _context.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item created successfully.";
            return RedirectToAction(nameof(Index));
        }

        var activeHouseholdId = _householdContext.GetActiveHouseholdId();
        var categories = new List<Category>();

        if (activeHouseholdId.HasValue)
        {
            categories = await _context.Categories
                .Where(c => c.HouseholdId == activeHouseholdId.Value)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        ViewBag.Categories = categories;
        ViewBag.ActiveHouseholdId = activeHouseholdId;

        return View(item);
    }

    // GET: Items/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var item = await _context.Items.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        // Check authorization
        if (!await CanModifyItem(item, userId))
        {
            TempData["ErrorMessage"] = "You don't have permission to edit this item.";
            return RedirectToAction(nameof(Index));
        }

        var categories = new List<Category>();
        if (item.HouseholdId.HasValue)
        {
            categories = await _context.Categories
                .Where(c => c.HouseholdId == item.HouseholdId.Value)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        ViewBag.Categories = categories;

        return View(item);
    }

    // POST: Items/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Quantity,IsUrgent,EstimatedPrice,CategoryId,Vendor,Notes,HouseholdId,CreatedByUserId,CreatedAt")] Item item)
    {
        if (id != item.Id)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var existingItem = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (existingItem == null)
        {
            return NotFound();
        }

        // Check authorization
        if (!await CanModifyItem(existingItem, userId))
        {
            TempData["ErrorMessage"] = "You don't have permission to edit this item.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                item.UpdatedAt = DateTime.UtcNow;
                _context.Update(item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Item updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(item.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        var categories = new List<Category>();
        if (item.HouseholdId.HasValue)
        {
            categories = await _context.Categories
                .Where(c => c.HouseholdId == item.HouseholdId.Value)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        ViewBag.Categories = categories;

        return View(item);
    }

    // GET: Items/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var item = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.CreatedBy)
            .Include(i => i.Household)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        // Check authorization
        if (!await CanModifyItem(item, userId))
        {
            TempData["ErrorMessage"] = "You don't have permission to delete this item.";
            return RedirectToAction(nameof(Index));
        }

        return View(item);
    }

    // POST: Items/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var item = await _context.Items.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        // Check authorization
        if (!await CanModifyItem(item, userId))
        {
            TempData["ErrorMessage"] = "You don't have permission to delete this item.";
            return RedirectToAction(nameof(Index));
        }

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Items/ExportCsv
    public async Task<IActionResult> ExportCsv(string? filter, int? categoryId, bool? urgent, string? search)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var activeHouseholdId = _householdContext.GetActiveHouseholdId();

        IQueryable<Item> query = _context.Items
            .Include(i => i.Category)
            .Include(i => i.Household);

        // Apply same filters as Index
        if (filter == "personal")
        {
            query = query.Where(i => i.HouseholdId == null && i.CreatedByUserId == userId);
        }
        else if (filter == "household" && activeHouseholdId.HasValue)
        {
            query = query.Where(i => i.HouseholdId == activeHouseholdId.Value);
        }
        else
        {
            if (activeHouseholdId.HasValue)
            {
                query = query.Where(i => 
                    (i.HouseholdId == null && i.CreatedByUserId == userId) || 
                    i.HouseholdId == activeHouseholdId.Value);
            }
            else
            {
                query = query.Where(i => i.HouseholdId == null && i.CreatedByUserId == userId);
            }
        }

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        if (urgent.HasValue)
        {
            query = query.Where(i => i.IsUrgent == urgent.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search));
        }

        var items = await query.OrderByDescending(i => i.IsUrgent)
            .ThenByDescending(i => i.CreatedAt)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Name,Quantity,Urgent,Estimated Price,Category,Vendor,Notes");

        foreach (var item in items)
        {
            csv.AppendLine($"\"{EscapeCsv(item.Name)}\",{item.Quantity},{(item.IsUrgent ? "Yes" : "No")},{item.EstimatedPrice?.ToString("F2") ?? ""},\"{EscapeCsv(item.Category?.Name ?? "")}\",\"{EscapeCsv(item.Vendor ?? "")}\",\"{EscapeCsv(item.Notes ?? "")}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"items_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    // GET: Items/PrintableView
    public async Task<IActionResult> PrintableView(string? filter, int? categoryId, bool? urgent, string? search)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var activeHouseholdId = _householdContext.GetActiveHouseholdId();

        IQueryable<Item> query = _context.Items
            .Include(i => i.Category)
            .Include(i => i.Household);

        // Apply same filters as Index
        if (filter == "personal")
        {
            query = query.Where(i => i.HouseholdId == null && i.CreatedByUserId == userId);
        }
        else if (filter == "household" && activeHouseholdId.HasValue)
        {
            query = query.Where(i => i.HouseholdId == activeHouseholdId.Value);
        }
        else
        {
            if (activeHouseholdId.HasValue)
            {
                query = query.Where(i => 
                    (i.HouseholdId == null && i.CreatedByUserId == userId) || 
                    i.HouseholdId == activeHouseholdId.Value);
            }
            else
            {
                query = query.Where(i => i.HouseholdId == null && i.CreatedByUserId == userId);
            }
        }

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        if (urgent.HasValue)
        {
            query = query.Where(i => i.IsUrgent == urgent.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search));
        }

        var items = await query.OrderByDescending(i => i.IsUrgent)
            .ThenByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(items);
    }

    private bool ItemExists(int id)
    {
        return _context.Items.Any(e => e.Id == id);
    }

    private async Task<bool> CanAccessItem(Item item, string userId)
    {
        // Can access if created by user
        if (item.CreatedByUserId == userId)
        {
            return true;
        }

        // Can access if it's a household item and user is a member
        if (item.HouseholdId.HasValue)
        {
            return await _context.HouseholdMembers
                .AnyAsync(hm => hm.HouseholdId == item.HouseholdId.Value && hm.UserId == userId);
        }

        return false;
    }

    private async Task<bool> CanModifyItem(Item item, string userId)
    {
        // Can modify if created by user
        if (item.CreatedByUserId == userId)
        {
            return true;
        }

        // Can modify if it's a household item and user is a member
        if (item.HouseholdId.HasValue)
        {
            return await _context.HouseholdMembers
                .AnyAsync(hm => hm.HouseholdId == item.HouseholdId.Value && hm.UserId == userId);
        }

        return false;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("\"", "\"\"");
    }
}
