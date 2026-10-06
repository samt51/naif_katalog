using System.Globalization;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;

namespace naif_katalog.Services.Concrete;

public class CustomerPricingService : ICustomerPricingService
{
    private readonly IApiService _apiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;

    public CustomerPricingService(IApiService apiService, IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
    {
        _apiService = apiService;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    public async Task<CustomerPricing> GetAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return new CustomerPricing();

        var userId = ReadUserId(user);
        var email = ReadEmail(user);
        var cacheKey = $"customer-pricing:{userId}:{email}";
        if (_cache.TryGetValue(cacheKey, out CustomerPricing? cached) && cached != null)
            return cached;

        var sales = ReadDecimal(user, "salesMultiplier", "SalesMultiplier") ?? 1m;
        var b2c = ReadDecimal(user, "b2cMultiplier", "B2CMultiplier", "b2CMultiplier") ?? 1m;
        var profile = await LoadProfileAsync(userId, email);
        if (profile?.SalesMultiplier is > 0)
            sales = profile.SalesMultiplier.Value;
        if (profile?.B2CMultiplier is > 0)
            b2c = profile.B2CMultiplier.Value;

        var pricing = new CustomerPricing
        {
            SalesMultiplier = sales < 0.01m ? 1m : sales,
            B2CMultiplier = b2c < 1m ? 1m : b2c
        };
        _cache.Set(cacheKey, pricing, TimeSpan.FromSeconds(20));
        return pricing;
    }

    private async Task<UserPricingProfileDto?> LoadProfileAsync(int userId, string? email)
    {
        if (userId > 0)
        {
            var single = await _apiService.GetAsync<UsersDto>($"api/Users/{userId}");
            if (single.isSuccess && single.data?.PricingProfile != null)
                return single.data.PricingProfile;
        }

        var all = await _apiService.GetAsync<List<UsersDto>>("api/Users");
        if (!all.isSuccess || all.data == null)
            return null;

        var match = all.data.FirstOrDefault(item =>
            (userId > 0 && item.Id == userId) ||
            (!string.IsNullOrWhiteSpace(email) && string.Equals(item.Email, email, StringComparison.OrdinalIgnoreCase)));
        return match?.PricingProfile;
    }

    private static int ReadUserId(ClaimsPrincipal user)
    {
        var raw = user.Claims.FirstOrDefault(claim =>
            claim.Type == ClaimTypes.NameIdentifier ||
            claim.Type.EndsWith("/nameidentifier", StringComparison.OrdinalIgnoreCase) ||
            claim.Type.Equals("id", StringComparison.OrdinalIgnoreCase) ||
            claim.Type.Equals("userId", StringComparison.OrdinalIgnoreCase) ||
            claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase) ||
            claim.Type.Equals("nameid", StringComparison.OrdinalIgnoreCase))?.Value;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : 0;
    }

    internal static string? ReadEmail(ClaimsPrincipal user)
    {
        var email = user.Claims.FirstOrDefault(claim =>
            claim.Type == ClaimTypes.Email ||
            claim.Type.EndsWith("/emailaddress", StringComparison.OrdinalIgnoreCase) ||
            claim.Type.Equals("email", StringComparison.OrdinalIgnoreCase))?.Value;
        if (!string.IsNullOrWhiteSpace(email))
            return email.Trim();

        return user.Claims
            .Where(claim =>
                claim.Type.Equals("unique_name", StringComparison.OrdinalIgnoreCase) ||
                claim.Type.Equals("preferred_username", StringComparison.OrdinalIgnoreCase) ||
                claim.Type.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase) ||
                claim.Type.EndsWith("/name", StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value) && value.Contains('@'))
            ?.Trim();
    }

    private static decimal? ReadDecimal(ClaimsPrincipal user, params string[] names)
    {
        var raw = user.Claims.FirstOrDefault(claim =>
            names.Any(name => claim.Type.Equals(name, StringComparison.OrdinalIgnoreCase)))?.Value;
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        return decimal.TryParse(raw.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }
}
