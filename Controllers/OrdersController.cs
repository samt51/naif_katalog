using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;

namespace naif_katalog.Controllers;

[Authorize]
public class OrdersController(IOrderStore orders, IOrderEmailService emails) : Controller
{
    private const int PageSize = 30;

    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Index(OrderListFilter filter, int page = 1)
    {
        page = Math.Max(1, page);
        filter ??= new OrderListFilter();
        filter.AccountId = null;
        var result = await orders.ListAsync(page, PageSize, filter);
        ViewBag.Page = page;
        ViewBag.Filter = filter;
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(result.Total / (double)PageSize));
        ViewBag.Total = result.Total;
        return View(result.Items.Select(x => x.ToSummary(false)).ToList());
    }

    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var order = await orders.GetAsync(id);
        if (order == null) return NotFound();
        return View(order.ToSummary());
    }

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

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "Admin")]
    public async Task<IActionResult> RetryEmail(Guid id)
    {
        var order = await orders.GetAsync(id);
        if (order == null) return NotFound();
        var sent = await emails.SendNewOrderAsync(order);
        var updated = await orders.UpdateEmailStatusAsync(id, sent) ?? order;
        TempData["OrderMessage"] = string.Equals(updated.EmailStatus, "Sent", StringComparison.OrdinalIgnoreCase)
            ? "Sipariş maili gönderildi."
            : "Mail gönderilemedi. SMTP ayarlarını kontrol edin.";
        TempData["OrderMessageOk"] = string.Equals(updated.EmailStatus, "Sent", StringComparison.OrdinalIgnoreCase);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, string status, string? returnUrl = null)
    {
        if (!OrderStatuses.IsValid(status))
            return BadRequest();
        var updated = await orders.UpdateStatusAsync(id, status);
        if (updated == null) return NotFound();
        TempData["OrderMessage"] = updated.OrderNumber + " durumu: " + OrderStatuses.Label(updated.Status);
        TempData["OrderMessageOk"] = true;
        if (string.Equals(returnUrl, "index", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Index));
        return RedirectToAction(nameof(Detail), new { id });
    }

    private int CurrentAccountId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type == "id" || c.Type == "userId" || c.Type == "sub" || c.Type == "nameid")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private bool IsAdmin() =>
        User.Claims.Any(claim =>
            (claim.Type == ClaimTypes.Role
             || claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase)
             || claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase)
             || claim.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))
            && (claim.Value == "1" || claim.Value == "2"));

    private bool CanAccess(int accountId)
    {
        if (IsAdmin()) return true;
        return CurrentAccountId() == accountId;
    }
}
