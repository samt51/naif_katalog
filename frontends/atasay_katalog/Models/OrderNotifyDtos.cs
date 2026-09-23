using System.ComponentModel.DataAnnotations;
namespace atasay_katalog.Models;

public sealed class ConfirmOrderRequest
{
    public Guid RequestId { get; set; }
    [Required, StringLength(200)] public string CompanyName { get; set; } = "";
    [Required, StringLength(100)] public string FirstName { get; set; } = "";
    [Required, StringLength(100)] public string LastName { get; set; } = "";
    [Required, StringLength(40), RegularExpression(@"\+?[0-9\s().\-]{7,40}")] public string PhoneNumber { get; set; } = "";
    [Required, MinLength(1), MaxLength(200)] public List<ConfirmOrderItem> Items { get; set; } = new();
    [Required, MinLength(20)] public string PdfBase64 { get; set; } = "";
}

public sealed class ConfirmOrderItem
{
    public int? ProductId { get; set; }
    [Required, StringLength(100)] public string Code { get; set; } = "";
    [StringLength(300)] public string? ProductName { get; set; }
    [StringLength(200)] public string? Category { get; set; }
    [StringLength(100)] public string? Price { get; set; }
    [StringLength(100)] public string? Ayar { get; set; }
    [StringLength(100)] public string? Renk { get; set; }
    [StringLength(100)] public string? Gram { get; set; }
    [Range(1, 999)] public int Quantity { get; set; } = 1;
    [StringLength(2000)] public string? Note { get; set; }
    [StringLength(500)] public string? ImageUrl { get; set; }
    [MaxLength(100)] public List<ConfirmOrderStone> Stones { get; set; } = new();
}

public sealed class ConfirmOrderStone
{
    [StringLength(100)] public string? Type { get; set; }
    [StringLength(100)] public string? Clarity { get; set; }
    [StringLength(100)] public string? Color { get; set; }
    [StringLength(100)] public string? Quantity { get; set; }
    [StringLength(100)] public string? TotalCarat { get; set; }
}

public sealed class OrderAccountSnapshot
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Company { get; set; } = "";
}

public class OrderSummary
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = "";
    public string AccountEmail { get; set; } = "";
    public string AccountCompany { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string EmailStatus { get; set; } = "";
    public DateTime? EmailSentUtc { get; set; }
    public string Status { get; set; } = OrderStatuses.Pending;
    public DateTime? StatusChangedUtc { get; set; }
    public int ItemCount { get; set; }
    public int TotalQuantity { get; set; }
    public bool HasPdf { get; set; }
    public string? PreviewImageUrl { get; set; }
    public List<ConfirmOrderItem> Items { get; set; } = new();
}

public sealed class OrderListFilter
{
    public string? Search { get; set; }
    public string? OrderNumber { get; set; }
    public string? Account { get; set; }
    public string? Customer { get; set; }
    public string? Phone { get; set; }
    public string? ProductCode { get; set; }
    public string? EmailStatus { get; set; }
    public int? AccountId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public bool HasAny =>
        AccountId is > 0 ||
        !string.IsNullOrWhiteSpace(Search) ||
        !string.IsNullOrWhiteSpace(OrderNumber) ||
        !string.IsNullOrWhiteSpace(Account) ||
        !string.IsNullOrWhiteSpace(Customer) ||
        !string.IsNullOrWhiteSpace(Phone) ||
        !string.IsNullOrWhiteSpace(ProductCode) ||
        !string.IsNullOrWhiteSpace(EmailStatus) ||
        !string.IsNullOrWhiteSpace(Status) ||
        FromDate.HasValue || ToDate.HasValue;

    public string Query(int page = 1)
    {
        var parts = new List<string> { "page=" + page };
        void Add(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                parts.Add(key + "=" + Uri.EscapeDataString(value.Trim()));
        }
        Add("search", Search);
        Add("orderNumber", OrderNumber);
        Add("account", Account);
        Add("customer", Customer);
        Add("phone", Phone);
        Add("productCode", ProductCode);
        Add("emailStatus", EmailStatus);
        Add("status", Status);
        if (FromDate.HasValue) parts.Add("fromDate=" + FromDate.Value.ToString("yyyy-MM-dd"));
        if (ToDate.HasValue) parts.Add("toDate=" + ToDate.Value.ToString("yyyy-MM-dd"));
        return string.Join("&", parts);
    }
}

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Cancelled = "Cancelled";

    public static bool IsValid(string? status) =>
        status is Pending or Accepted or Cancelled;

    public static string Label(string? status) => status switch
    {
        Accepted => "Kabul",
        Cancelled => "İptal",
        _ => "Beklemede"
    };
}

public sealed class OrderRecord : OrderSummary
{
    public Guid RequestId { get; set; }

    public OrderSummary ToSummary(bool includeItems = true) => new()
    {
        Id = Id,
        OrderNumber = OrderNumber,
        CreatedUtc = CreatedUtc,
        AccountId = AccountId,
        AccountName = AccountName,
        AccountEmail = AccountEmail,
        AccountCompany = AccountCompany,
        CompanyName = CompanyName,
        FirstName = FirstName,
        LastName = LastName,
        PhoneNumber = PhoneNumber,
        EmailStatus = EmailStatus,
        EmailSentUtc = EmailSentUtc,
        Status = Status,
        StatusChangedUtc = StatusChangedUtc,
        ItemCount = ItemCount,
        TotalQuantity = TotalQuantity,
        HasPdf = HasPdf,
        PreviewImageUrl = PreviewImageUrl,
        Items = includeItems ? Items : []
    };
}
