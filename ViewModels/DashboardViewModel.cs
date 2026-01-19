namespace HouseholdNeedsManager.ViewModels;

public class DashboardViewModel
{
    public CurrentWeekStats? PersonalStats { get; set; }
    public CurrentWeekStats? HouseholdStats { get; set; }
    public List<WeeklyHistoricalStats> PersonalHistoricalStats { get; set; } = new();
    public List<WeeklyHistoricalStats> HouseholdHistoricalStats { get; set; } = new();
    public bool HasActiveHousehold { get; set; }
    public string? ActiveHouseholdName { get; set; }
}

public class CurrentWeekStats
{
    public int TotalItems { get; set; }
    public int UrgentItems { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public List<CategoryStats> ItemsByCategory { get; set; } = new();
}

public class CategoryStats
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class WeeklyHistoricalStats
{
    public string WeekLabel { get; set; } = string.Empty;
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int TotalItems { get; set; }
    public int UrgentItems { get; set; }
    public decimal TotalEstimatedCost { get; set; }
}
