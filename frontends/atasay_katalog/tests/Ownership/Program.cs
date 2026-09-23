using System.Security.Claims;
using atasay_katalog.Controllers;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

var emails = new NoEmail();
var otherOrder = new OrderRecord { AccountId = 101 };
var controller = new HomeController(null!, new MemoryCache(new MemoryCacheOptions()), emails)
{
    ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "102") }, "Test"))
        }
    }
};
var result = await controller.ConfirmOrder(new ConfirmOrderRequest { RequestId = Guid.NewGuid() }, new ReplayStore(otherOrder));
if (result is not ForbidResult || emails.Count != 0)
    throw new Exception("Replaying another customer's request ID must not disclose the order or send an email.");
Console.WriteLine("PASS: cross-customer order replay is forbidden before notification or response; no database accessed.");

var categoryImages = new atasay_katalog.Services.Concrete.HomeContentStore(null!);
foreach (var (name, image) in new[] { ("BİLEKLİK", "bracelet"), ("GERDANLIK", "collar"), ("KELEPÇE", "bangle"), ("KOLYE", "necklace"), ("KÜPE", "earring"), ("YÜZÜK", "ring"), ("SET", "set") })
{
    if (categoryImages.CategoryImage(999, name) != $"/images/brand/category-{image}.jpg")
        throw new Exception($"Incorrect fixed category image: {name}");
}
Console.WriteLine("PASS: all seven uppercase Turkish categories map to distinct fixed Atasay images.");

var categoryLabel = new atasay_katalog.TagHelpers.CategoryLabelTagHelper { Name = "set" };
var tagContext = new Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext(new(), new Dictionary<object, object>(), "category-test");
var tagOutput = new Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput("category-label", new(), (_, _) => Task.FromResult<Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContent>(new Microsoft.AspNetCore.Razor.TagHelpers.DefaultTagHelperContent()));
categoryLabel.Process(tagContext, tagOutput);
if (tagOutput.Content.GetContent() != "SET" || tagOutput.Attributes["translate"].Value.ToString() != "no" || tagOutput.Attributes["class"].Value.ToString() != "notranslate")
    throw new Exception("SET must be protected from automatic translation.");
Console.WriteLine("PASS: SET label is preserved and excluded from automatic translation.");

sealed class NoEmail : IOrderEmailService
{
    public int Count;
    public Task<bool> SendNewOrderAsync(OrderRecord order, CancellationToken token = default) { Count++; return Task.FromResult(true); }
}
sealed class ReplayStore(OrderRecord existing) : IOrderStore
{
    public Task<OrderRecord> CreateOrGetAsync(ConfirmOrderRequest request, OrderAccountSnapshot account, CancellationToken token = default) => Task.FromResult(existing);
    public Task<(List<OrderRecord> Items, int Total)> ListAsync(int page, int pageSize, OrderListFilter? filter, CancellationToken token = default) => throw new NotSupportedException();
    public Task<OrderRecord?> GetAsync(Guid id, CancellationToken token = default) => throw new NotSupportedException();
    public Task<byte[]?> GetPdfAsync(Guid id, CancellationToken token = default) => throw new NotSupportedException();
    public Task<OrderRecord?> UpdateEmailStatusAsync(Guid id, bool sent, CancellationToken token = default) => throw new Exception("Must not change another customer's order");
    public Task<OrderRecord?> UpdateStatusAsync(Guid id, string status, CancellationToken token = default) => throw new NotSupportedException();
}
