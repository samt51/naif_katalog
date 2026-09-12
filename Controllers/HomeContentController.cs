using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using naif_katalog.Core.Features.CategoryFeature.Queries;
using naif_katalog.Models;
using naif_katalog.Services.Concrete;

namespace naif_katalog.Controllers;

[Authorize(Policy = "Admin")]
public class HomeContentController(IMediator mediator, HomeContentStore store) : Controller
{
    public async Task<IActionResult> Index()
    {
        var response = await mediator.Send(new GetAllCategoriesQueryRequest());
        ViewBag.Subscribers = await store.Subscribers();
        ViewBag.LoadFailed = response?.isSuccess != true;
        return View((response?.data ?? []).Where(c => c.ParentId == 0).OrderBy(c => c.OrderIndex)
            .Select(c => new HomeCategoryCard(c.Id, c.Name, store.CategoryImage(c.Id))).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(9 * 1024 * 1024)]
    public async Task<IActionResult> UploadCategoryImage(int categoryId, IFormFile? file)
    {
        var response = await mediator.Send(new GetAllCategoriesQueryRequest());
        if (response?.data?.Any(c => c.Id == categoryId && c.ParentId == 0) != true) return BadRequest();
        try
        {
            if (file == null) throw new ArgumentException("Bir görsel seçin.");
            await store.SaveCategoryImage(categoryId, file);
            TempData["ContentSuccess"] = "Kategori görseli kaydedildi.";
        }
        catch (ArgumentException ex) { TempData["ContentError"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
