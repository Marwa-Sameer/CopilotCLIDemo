using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseholdNeedsManager.Data;
using HouseholdNeedsManager.Models;
using HouseholdNeedsManager.Services;

namespace HouseholdNeedsManager.Controllers;

[Authorize]
public class HouseholdsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHouseholdContextService _householdContext;

    public HouseholdsController(ApplicationDbContext context, IHouseholdContextService householdContext)
    {
        _context = context;
        _householdContext = householdContext;
    }

    // GET: Households
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var households = await _context.HouseholdMembers
            .Where(hm => hm.UserId == userId)
            .Include(hm => hm.Household)
            .ThenInclude(h => h!.Owner)
            .Include(hm => hm.Household)
            .ThenInclude(h => h!.Members)
            .OrderByDescending(hm => hm.JoinedAt)
            .ToListAsync();

        var activeHouseholdId = _householdContext.GetActiveHouseholdId();
        ViewBag.ActiveHouseholdId = activeHouseholdId;

        return View(households);
    }

    // GET: Households/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Households/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] Household household)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (ModelState.IsValid)
        {
            try
            {
                household.OwnerId = userId;
                household.CreatedAt = DateTime.UtcNow;
                _context.Households.Add(household);
                await _context.SaveChangesAsync();

                // Add the creator as a member with Owner role
                var householdMember = new HouseholdMember
                {
                    UserId = userId,
                    HouseholdId = household.Id,
                    Role = HouseholdRole.Owner,
                    JoinedAt = DateTime.UtcNow
                };
                _context.HouseholdMembers.Add(householdMember);
                await _context.SaveChangesAsync();

                // Set as active household
                _householdContext.SetActiveHouseholdId(household.Id);

                TempData["SuccessMessage"] = $"Household '{household.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while creating the household: {ex.Message}");
            }
        }

        return View(household);
    }

    // GET: Households/Join
    public IActionResult Join()
    {
        return View();
    }

    // POST: Households/Join
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(int householdId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            // Check if household exists
            var household = await _context.Households.FindAsync(householdId);
            if (household == null)
            {
                TempData["ErrorMessage"] = $"Household with ID {householdId} does not exist.";
                return View();
            }

            // Check if already a member
            var existingMember = await _context.HouseholdMembers
                .FirstOrDefaultAsync(hm => hm.UserId == userId && hm.HouseholdId == householdId);
            
            if (existingMember != null)
            {
                TempData["ErrorMessage"] = $"You are already a member of '{household.Name}'.";
                return RedirectToAction(nameof(Index));
            }

            // Add as member
            var householdMember = new HouseholdMember
            {
                UserId = userId,
                HouseholdId = householdId,
                Role = HouseholdRole.Member,
                JoinedAt = DateTime.UtcNow
            };
            _context.HouseholdMembers.Add(householdMember);
            await _context.SaveChangesAsync();

            // Set as active household
            _householdContext.SetActiveHouseholdId(householdId);

            TempData["SuccessMessage"] = $"Successfully joined household '{household.Name}'!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while joining the household: {ex.Message}";
            return View();
        }
    }

    // POST: Households/Switch/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Switch(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        // Verify user is a member of this household
        var membership = await _context.HouseholdMembers
            .Include(hm => hm.Household)
            .FirstOrDefaultAsync(hm => hm.UserId == userId && hm.HouseholdId == id);

        if (membership == null)
        {
            TempData["ErrorMessage"] = "You are not a member of this household.";
            return RedirectToAction(nameof(Index));
        }

        _householdContext.SetActiveHouseholdId(id);
        TempData["SuccessMessage"] = $"Switched to household '{membership.Household?.Name}'.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Households/Leave/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var membership = await _context.HouseholdMembers
            .Include(hm => hm.Household)
            .FirstOrDefaultAsync(hm => hm.UserId == userId && hm.HouseholdId == id);

        if (membership == null)
        {
            TempData["ErrorMessage"] = "You are not a member of this household.";
            return RedirectToAction(nameof(Index));
        }

        // Check if user is the owner
        if (membership.Role == HouseholdRole.Owner)
        {
            TempData["ErrorMessage"] = "You cannot leave a household you own. Delete the household instead.";
            return RedirectToAction(nameof(Index));
        }

        var householdName = membership.Household?.Name;

        _context.HouseholdMembers.Remove(membership);
        await _context.SaveChangesAsync();

        // Clear active household if this was the active one
        var activeHouseholdId = _householdContext.GetActiveHouseholdId();
        if (activeHouseholdId == id)
        {
            _householdContext.ClearActiveHousehold();
        }

        TempData["SuccessMessage"] = $"You have left household '{householdName}'.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Households/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var household = await _context.Households
            .FirstOrDefaultAsync(h => h.Id == id && h.OwnerId == userId);

        if (household == null)
        {
            TempData["ErrorMessage"] = "Household not found or you are not the owner.";
            return RedirectToAction(nameof(Index));
        }

        var householdName = household.Name;

        _context.Households.Remove(household);
        await _context.SaveChangesAsync();

        // Clear active household if this was the active one
        var activeHouseholdId = _householdContext.GetActiveHouseholdId();
        if (activeHouseholdId == id)
        {
            _householdContext.ClearActiveHousehold();
        }

        TempData["SuccessMessage"] = $"Household '{householdName}' deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
