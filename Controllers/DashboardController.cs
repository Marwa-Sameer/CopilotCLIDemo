using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HouseholdNeedsManager.Data;
using HouseholdNeedsManager.Services;
using HouseholdNeedsManager.ViewModels;

namespace HouseholdNeedsManager.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHouseholdContextService _householdContext;

    public DashboardController(ApplicationDbContext context, IHouseholdContextService householdContext)
    {
        _context = context;
        _householdContext = householdContext;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var activeHouseholdId = _householdContext.GetActiveHouseholdId();
        var viewModel = new DashboardViewModel
        {
            HasActiveHousehold = activeHouseholdId.HasValue
        };

        if (activeHouseholdId.HasValue)
        {
            var household = await _context.Households
                .FirstOrDefaultAsync(h => h.Id == activeHouseholdId.Value);
            viewModel.ActiveHouseholdName = household?.Name;
        }

        var currentWeekStart = GetStartOfWeek(DateTime.UtcNow);
        var currentWeekEnd = currentWeekStart.AddDays(7);

        viewModel.PersonalStats = await GetCurrentWeekStats(userId, null, currentWeekStart, currentWeekEnd);
        viewModel.PersonalHistoricalStats = await GetHistoricalStats(userId, null);

        if (activeHouseholdId.HasValue)
        {
            viewModel.HouseholdStats = await GetCurrentWeekStats(userId, activeHouseholdId.Value, currentWeekStart, currentWeekEnd);
            viewModel.HouseholdHistoricalStats = await GetHistoricalStats(userId, activeHouseholdId.Value);
        }

        return View(viewModel);
    }

    private async Task<CurrentWeekStats> GetCurrentWeekStats(string userId, int? householdId, DateTime weekStart, DateTime weekEnd)
    {
        var query = _context.Items
            .Where(i => i.CreatedAt >= weekStart && i.CreatedAt < weekEnd);

        if (householdId.HasValue)
        {
            query = query.Where(i => i.HouseholdId == householdId.Value);
        }
        else
        {
            query = query.Where(i => i.HouseholdId == null && i.CreatedByUserId == userId);
        }

        var items = await query
            .Include(i => i.Category)
            .ToListAsync();

        var stats = new CurrentWeekStats
        {
            TotalItems = items.Count,
            UrgentItems = items.Count(i => i.IsUrgent),
            TotalEstimatedCost = items.Sum(i => i.EstimatedPrice ?? 0)
        };

        var categoryGroups = items
            .GroupBy(i => i.Category?.Name ?? "Uncategorized")
            .Select((g, index) => new CategoryStats
            {
                CategoryName = g.Key,
                Count = g.Count(),
                Color = GetChartColor(index)
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        stats.ItemsByCategory = categoryGroups;

        return stats;
    }

    private async Task<List<WeeklyHistoricalStats>> GetHistoricalStats(string userId, int? householdId)
    {
        var weeklyStats = new List<WeeklyHistoricalStats>();
        var currentDate = DateTime.UtcNow;

        for (int i = 0; i < 4; i++)
        {
            var weekStart = GetStartOfWeek(currentDate.AddDays(-7 * i));
            var weekEnd = weekStart.AddDays(7);

            var query = _context.Items
                .Where(i => i.CreatedAt >= weekStart && i.CreatedAt < weekEnd);

            if (householdId.HasValue)
            {
                query = query.Where(i => i.HouseholdId == householdId.Value);
            }
            else
            {
                query = query.Where(i => i.HouseholdId == null && i.CreatedByUserId == userId);
            }

            var items = await query.ToListAsync();

            weeklyStats.Add(new WeeklyHistoricalStats
            {
                WeekLabel = GetWeekLabel(weekStart),
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                TotalItems = items.Count,
                UrgentItems = items.Count(i => i.IsUrgent),
                TotalEstimatedCost = items.Sum(i => i.EstimatedPrice ?? 0)
            });
        }

        weeklyStats.Reverse();
        return weeklyStats;
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private string GetWeekLabel(DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(6);
        return $"{weekStart:MMM dd} - {weekEnd:MMM dd}";
    }

    private string GetChartColor(int index)
    {
        var colors = new[]
        {
            "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
            "#FF9F40", "#FF6384", "#C9CBCF", "#4BC0C0", "#FF6384"
        };
        return colors[index % colors.Length];
    }
}
