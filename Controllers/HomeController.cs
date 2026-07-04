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

                    var stonesResp = await _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Queries.GetAllStonesQueryRequest());
                    ViewBag.Stones = stonesResp?.data;

                    var stoneTypesResp = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneTypesQueryRequest());
                    ViewBag.StoneTypes = stoneTypesResp?.data;
                } catch { }

                return View(product);

            }
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index(int? categoryId = null, string? search = null, decimal? minPrice = null, decimal? maxPrice = null, string? sortOrder = null)
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

        try {
            var colorsResp = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllMetalTypesQueryRequest());
            ViewBag.MetalTypes = (colorsResp != null && colorsResp.isSuccess) ? colorsResp.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.MetalTypeDto>();

            var claritiesResp = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneClaritysQueryRequest());
            ViewBag.Clarities = (claritiesResp != null && claritiesResp.isSuccess) ? claritiesResp.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto>();

            var stoneTypesResp = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneTypesQueryRequest());
            ViewBag.StoneTypes = (stoneTypesResp != null && stoneTypesResp.isSuccess) ? stoneTypesResp.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto>();
        } catch (Exception ex) {
            Console.WriteLine("Error fetching filters: " + ex.Message);
            ViewBag.MetalTypes = new List<naif_katalog.Core.Features.DefinitionFeature.Queries.MetalTypeDto>();
            ViewBag.Clarities = new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto>();
            ViewBag.StoneTypes = new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto>();
        }

        // Fallback: If API failed (e.g. 401 Unauthorized because user is public), extract from products!
        var metalTypeList = ViewBag.MetalTypes as List<naif_katalog.Core.Features.DefinitionFeature.Queries.MetalTypeDto>;
        if (metalTypeList == null || !metalTypeList.Any())
        {
            ViewBag.MetalTypes = fetchedProducts
                .SelectMany(p => p.ProductMetals)
                .Where(pm => pm != null && pm.MetalTypeId > 0 && !string.IsNullOrEmpty(pm.MetalTypeName))
                .GroupBy(m => m.MetalTypeId)
                .Select(g => new naif_katalog.Core.Features.DefinitionFeature.Queries.MetalTypeDto { Id = g.Key, Name = g.First().MetalTypeName })
                .ToList();
        }

        var clarityList = ViewBag.Clarities as List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto>;
        if (clarityList == null || !clarityList.Any())
        {
            ViewBag.Clarities = fetchedProducts
                .SelectMany(p => p.ProductStones)
                .Where(ps => ps != null && ps.ClarityId.HasValue && !string.IsNullOrEmpty(ps.ClarityName))
                .GroupBy(c => c.ClarityId.Value)
                .Select(g => new naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto { Id = g.Key, Name = g.First().ClarityName })
                .ToList();
        }

        var stoneTypeList = ViewBag.StoneTypes as List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto>;
        if (stoneTypeList == null || !stoneTypeList.Any())
        {
            // Fallback common stone types since we cannot extract from ApiProductStone
            ViewBag.StoneTypes = new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto>
            {
                new naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto { Id = 1, Name = "Pırlanta" },
                new naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto { Id = 2, Name = "Zümrüt" },
                new naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto { Id = 3, Name = "Safir" },
                new naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto { Id = 4, Name = "Yakut" }
            };
        }

        try {
            var stoneResponse = await _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Queries.GetAllStonesQueryRequest());
            ViewBag.Stones = (stoneResponse != null && stoneResponse.isSuccess) ? stoneResponse.data : new List<naif_katalog.Models.StoneDto>();
        } catch {
            ViewBag.Stones = new List<naif_katalog.Models.StoneDto>();
        }

        foreach (var item in fetchedProducts)
        {
            if (item.Code == "API_ERROR")
            {
                products.Add(item);
                continue;
            }

            if (!string.IsNullOrEmpty(search) && !item.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                continue;

            if (minPrice.HasValue && item.CalculatedPrice < minPrice.Value)
                continue;

            if (maxPrice.HasValue && item.CalculatedPrice > maxPrice.Value)
                continue;

            products.Add(item);
        }

        switch (sortOrder)
        {
            case "price_asc":
                products = products.OrderBy(p => p.CalculatedPrice).ToList();
                break;
            case "price_desc":
                products = products.OrderByDescending(p => p.CalculatedPrice).ToList();
                break;
            default:
                // Önerilen
                break;
        }

        ViewBag.CurrentSort = sortOrder;
        return View(products);
    }
}

