using naif_katalog.Models;

namespace naif_katalog.Services.Abstract
{
    public interface IOrderEmailService
    {
        Task<bool> SendNewOrderAsync(OrderRecord order, CancellationToken cancellationToken = default);
    }
}
