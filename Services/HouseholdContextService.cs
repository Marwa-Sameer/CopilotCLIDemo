namespace HouseholdNeedsManager.Services;

/// <summary>
/// Service to track the active household context for the current user session
/// </summary>
public interface IHouseholdContextService
{
    /// <summary>
    /// Gets the active household ID for the current user
    /// </summary>
    int? GetActiveHouseholdId();

    /// <summary>
    /// Sets the active household ID for the current user
    /// </summary>
    void SetActiveHouseholdId(int? householdId);

    /// <summary>
    /// Clears the active household ID
    /// </summary>
    void ClearActiveHousehold();
}

/// <summary>
/// Implementation of household context service using session storage
/// </summary>
public class HouseholdContextService : IHouseholdContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string ActiveHouseholdKey = "ActiveHouseholdId";

    public HouseholdContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetActiveHouseholdId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session == null)
            return null;

        var householdIdStr = context.Session.GetString(ActiveHouseholdKey);
        if (int.TryParse(householdIdStr, out int householdId))
            return householdId;

        return null;
    }

    public void SetActiveHouseholdId(int? householdId)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session == null)
            return;

        if (householdId.HasValue)
            context.Session.SetString(ActiveHouseholdKey, householdId.Value.ToString());
        else
            context.Session.Remove(ActiveHouseholdKey);
    }

    public void ClearActiveHousehold()
    {
        var context = _httpContextAccessor.HttpContext;
        context?.Session?.Remove(ActiveHouseholdKey);
    }
}
