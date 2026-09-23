using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;

namespace atasay_katalog.Controllers;

[Authorize]
public class OrdersController(IOrderStore orders) : Controller
{
    private const int PageSize = 30;

    public async Task<IActionResult> Mine(string? status = null, int page = 1)
    {
        var accountId = CurrentAccountId();
        if (accountId <= 0) return Unauthorized();
        page = Math.Max(1, page);
        var filter = new OrderListFilter
        {
            AccountId = accountId,
            Status = OrderStatuses.IsValid(status) ? status : null
        };
        var result = await orders.ListAsync(page, PageSize, filter);
        ViewBag.Page = page;
        ViewBag.Status = filter.Status;
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(result.Total / (double)PageSize));
        ViewBag.Total = result.Total;
        return View(result.Items.Select(x => x.ToSummary(false)).ToList());
    }

    public async Task<IActionResult> MineDetail(Guid id)
    {
        var order = await orders.GetAsync(id);
        if (order == null) return NotFound();
        if (!CanAccess(order.AccountId)) return Forbid();
        return View(order.ToSummary());
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(Guid id)
    {
        var order = await orders.GetAsync(id);
        if (order == null) return NotFound();
        if (!CanAccess(order.AccountId)) return Forbid();
        var pdf = await orders.GetPdfAsync(id);
        if (pdf == null) return NotFound();
        return File(pdf, "application/pdf", $"{order.OrderNumber}.pdf");
    }

    private int CurrentAccountId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type == "id" || c.Type == "userId" || c.Type == "sub" || c.Type == "nameid")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private bool CanAccess(int accountId) => CurrentAccountId() == accountId;
}
