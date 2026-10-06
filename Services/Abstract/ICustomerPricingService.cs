namespace naif_katalog.Services.Abstract;

public interface ICustomerPricingService
{
    Task<CustomerPricing> GetAsync();
}

public sealed class CustomerPricing
{
    public decimal SalesMultiplier { get; init; } = 1m;
    public decimal B2CMultiplier { get; init; } = 1m;
}
