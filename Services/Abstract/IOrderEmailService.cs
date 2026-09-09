using naif_katalog.Models;

namespace naif_katalog.Services.Abstract
{
    public interface IOrderEmailService
    {
        Task<bool> SendNewOrderAsync(ConfirmOrderRequest order, string? accountName, string? accountEmail, CancellationToken cancellationToken = default);
    }
}
