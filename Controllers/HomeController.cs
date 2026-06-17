using Microsoft.AspNetCore.Mvc;
using naif_katalog.Models;
using MediatR;
using naif_katalog.Core.Features.ProductFeature.Queries;

namespace naif_katalog.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

        public async Task<IActionResult> Detail(int id)
    {
        var prodResponse = await _mediator.Send(new GetAllProductsQueryRequest());
        if (prodResponse.isSuccess && prodResponse.data != null)
        {
            var product = prodResponse.data.FirstOrDefault(x => x.Id == id);
            if (product != null)
            {
                
                // Log View Product Activity
                try
                {
                    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "id" || c.Type == "userId")?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int uid))
                    {
                        await _mediator.Send(new naif_katalog.Core.Features.UserActionLogFeature.Commands.Create.CreateUserActionLogCommandRequest
                        {
                            UserId = uid,
                            ActionType = "ViewProduct",
                            ProductId = id,
                            Details = product.Name + " ürünü incelendi.",
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                        });
                    }
                }
                catch { }

                try {
                    ViewBag.RelatedProducts = prodResponse.data
                        .Where(x => x.CategoryIds != null && product.CategoryIds != null && x.CategoryIds.Any(c => product.CategoryIds.Contains(c)) && x.Id != product.Id)
                        .Take(10)
                        .ToList();

                    var colorsResp = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllMetalTypesQueryRequest());
                    ViewBag.MetalTypes = colorsResp?.data;

                    var puritiesResp = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllMetalPuritysQueryRequest());
                    ViewBag.Karats = puritiesResp?.data;

                    var claritiesResp = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneClaritysQueryRequest());
                    ViewBag.Clarities = claritiesResp?.data;
                } catch { }

                return View(product);

            }
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index(int? categoryId = null, string? search = null)
    {
        List<Product> fetchedProducts = new List<Product>();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            var catResponse = await _mediator.Send(new GetProductsByCategoryIdQueryRequest { CategoryId = categoryId.Value });
            if (catResponse.isSuccess && catResponse.data != null)
            {
                fetchedProducts = catResponse.data.Select(x => new Product
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CategoryNames = x.CategoryNames,
                    CategoryIds = x.CategoryIds,
                    Description = x.Description,
                    Gram = x.Gram,
                    Karat = x.Karat,
                    DiamondCarat = x.DiamondCarat,
                    ColorId = x.ColorId,
                    ColorName = x.ColorName,
                    CalculatedPrice = x.CalculatedPrice,
                    Images = x.Images
                }).ToList();
            }
            else
            {
                fetchedProducts.Add(new Product { Code = "API_ERROR", CategoryNames = new List<string> { "HATA" }, Images = new List<string>() });
            }
        }
        else
        {
            var allResponse = await _mediator.Send(new GetAllProductsQueryRequest());
            if (allResponse.isSuccess && allResponse.data != null)
                fetchedProducts = allResponse.data;
            else
                fetchedProducts.Add(new Product { Code = "API_ERROR", CategoryNames = new List<string> { "HATA" }, Images = new List<string>() });
        }

        var products = new List<Product>();
        var categoriesResponse = await _mediator.Send(new naif_katalog.Core.Features.CategoryFeature.Queries.GetAllCategoriesQueryRequest());
        var categoriesList = categoriesResponse.isSuccess ? categoriesResponse.data : new List<naif_katalog.Models.CategoryDto>();
        ViewBag.Categories = categoriesList;
        
        var currentCategoryObj = categoryId.HasValue ? categoriesList.FirstOrDefault(c => c.Id == categoryId.Value) : null;
        ViewBag.CurrentCategory = currentCategoryObj?.Name;
        ViewBag.CurrentCategoryId = categoryId;
        ViewBag.SearchQuery = search;

        foreach (var item in fetchedProducts)
        {
            if (item.Code == "API_ERROR")
            {
                products.Add(item);
                continue;
            }

            if (!string.IsNullOrEmpty(search) && !item.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                continue;

            products.Add(item);
        }

        return View(products);
    }
}

