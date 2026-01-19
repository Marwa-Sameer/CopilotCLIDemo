using HouseholdNeedsManager.Services;

namespace HouseholdNeedsManager.Middleware;

/// <summary>
/// Middleware to inject household context into HttpContext
/// </summary>
public class HouseholdContextMiddleware
{
    private readonly RequestDelegate _next;

    public HouseholdContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IHouseholdContextService householdContextService)
    {
        // Store household ID in HttpContext.Items for easy access in controllers
        var householdId = householdContextService.GetActiveHouseholdId();
        if (householdId.HasValue)
        {
            context.Items["ActiveHouseholdId"] = householdId.Value;
        }

        await _next(context);
    }
}
