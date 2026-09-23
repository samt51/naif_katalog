using atasay_katalog.Models;

namespace atasay_katalog.Services.Abstract
{
    public interface IOrderEmailService
    {
        Task<bool> SendNewOrderAsync(OrderRecord order, CancellationToken cancellationToken = default);
    }
}
