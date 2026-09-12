using naif_katalog.Models;

namespace naif_katalog.Services.Abstract;

public interface IOrderStore
{
    Task<(List<OrderRecord> Items, int Total)> ListAsync(int page, int pageSize, OrderListFilter? filter, CancellationToken cancellationToken = default);
    Task<OrderRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<byte[]?> GetPdfAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderRecord> CreateOrGetAsync(ConfirmOrderRequest request, OrderAccountSnapshot account, CancellationToken cancellationToken = default);
    Task<OrderRecord?> UpdateEmailStatusAsync(Guid id, bool sent, CancellationToken cancellationToken = default);
    Task<OrderRecord?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
}
