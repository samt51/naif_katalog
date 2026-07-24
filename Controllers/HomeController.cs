using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using naif_katalog.Models;
using MediatR;
using naif_katalog.Core.Features.ProductFeature.Queries;

namespace naif_katalog.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;
    private readonly IMemoryCache _memoryCache;

    public HomeController(IMediator mediator, IMemoryCache memoryCache)
    {
        _mediator = mediator;
        _memoryCache = memoryCache;
    }

    public async Task<IActionResult> Detail(int id)
    {
        var detailResult = await FindProductForDetail(id);
        var product = detailResult.Product;
        var productPool = detailResult.Products;

        if (product != null)
        {
            // Log View Product Activity
            try
            {
                var allClaims = string.Join(", ", User.Claims.Select(c => c.Type + ":" + c.Value));
                System.Console.WriteLine("TÃ¼m Claimler: " + allClaims);
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "id" || c.Type == "userId" || c.Type == "sub" || c.Type == "nameid")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int uid))
                {
                    await _mediator.Send(new naif_katalog.Core.Features.UserActionLogFeature.Commands.Create.CreateUserActionLogCommandRequest
                    {
                        UserId = uid,
                        ActionType = "ViewProduct",
                        ProductId = id,
                        Details = product.Name + " Ã¼rÃ¼nÃ¼ incelendi. GÃ¶rÃ¼len Fiyat: $" + product.CalculatedPrice.ToString("N2"),
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                        UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                    });
                }
            }
            catch (System.Exception ex) { 
                System.Console.WriteLine("UserActionLog HATA: " + ex.Message + " | " + ex.StackTrace);
            }

            try {
                ViewBag.RelatedProducts = productPool
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

        return RedirectToAction("Index");
    }

    private async Task<(Product? Product, List<Product> Products)> FindProductForDetail(int id)
    {
        if (_memoryCache.TryGetValue($"CachedProduct_{id}", out Product? cachedProduct) && cachedProduct != null)
        {
            return (cachedProduct, new List<Product> { cachedProduct });
        }

        const int pageSize = 100;
        var loadedProducts = new List<Product>();

        for (var page = 1; page <= 100; page++)
        {
            var prodResponse = await _mediator.Send(new GetAllProductsQueryRequest
            {
                Page = page,
                PageSize = pageSize
            });

            if (prodResponse == null || !prodResponse.isSuccess || prodResponse.data == null || prodResponse.data.Count == 0)
            {
                break;
            }

            foreach (var product in prodResponse.data)
            {
                _memoryCache.Set($"CachedProduct_{product.Id}", product, TimeSpan.FromMinutes(10));
            }

            loadedProducts.AddRange(prodResponse.data);

            var found = prodResponse.data.FirstOrDefault(x => x.Id == id);
            if (found != null)
            {
                return (found, loadedProducts);
            }

            var totalCount = prodResponse.count > 0 ? prodResponse.count : loadedProducts.Count;
            if (loadedProducts.Count >= totalCount)
            {
                break;
            }
        }

        return (null, loadedProducts);
    }

    public async Task<IActionResult> Index(int? categoryId = null, string? search = null, decimal? minPrice = null, decimal? maxPrice = null, string? sortOrder = null, int page = 1)
    {
        if (HttpContext != null)
        {
            const int homePageSize = 12;
            if (page < 1) page = 1;

            var columnIndex = sortOrder switch
            {
                "price_asc" => 6,
                "price_desc" => 6,
                _ => 0
            };

            var orderBy = sortOrder == "price_desc" ? "desc" : "asc";

            var productsTask = _mediator.Send(new GetAllProductsQueryRequest
            {
                Code = search,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Page = page,
                PageSize = homePageSize,
                ColumnIndex = columnIndex,
                OrderBy = orderBy
            });
            var categoriesTask = _mediator.Send(new naif_katalog.Core.Features.CategoryFeature.Queries.GetAllCategoriesQueryRequest());
            await Task.WhenAll(productsTask, categoriesTask);
            var productsResponsePaged = await productsTask;
            var categoriesResponsePaged = await categoriesTask;
            var categoriesListPaged = categoriesResponsePaged.isSuccess ? categoriesResponsePaged.data : new List<naif_katalog.Models.CategoryDto>();
            var currentCategoryObjPaged = categoryId.HasValue ? categoriesListPaged.FirstOrDefault(c => c.Id == categoryId.Value) : null;

            var pagedProductsFromBackend = productsResponsePaged != null && productsResponsePaged.isSuccess && productsResponsePaged.data != null
                ? productsResponsePaged.data
                : new List<Product>();

            foreach (var product in pagedProductsFromBackend)
            {
                _memoryCache.Set($"CachedProduct_{product.Id}", product, TimeSpan.FromMinutes(10));
            }

            var totalProductsPaged = productsResponsePaged != null && productsResponsePaged.count > 0 ? productsResponsePaged.count : pagedProductsFromBackend.Count;
            var totalPagesPaged = (int)Math.Ceiling(totalProductsPaged / (double)homePageSize);

            ViewBag.Categories = categoriesListPaged;
            ViewBag.CurrentCategory = currentCategoryObjPaged?.Name;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.SearchQuery = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPagesPaged;
            ViewBag.TotalCount = totalProductsPaged;

            try {
                var colorsTask = _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllMetalTypesQueryRequest());
                var claritiesTask = _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneClaritysQueryRequest());
                var stoneTypesTask = _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneTypesQueryRequest());
                var stonesTask = _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Queries.GetAllStonesQueryRequest());
                await Task.WhenAll(colorsTask, claritiesTask, stoneTypesTask, stonesTask);

                var colorsResp = await colorsTask;
                ViewBag.MetalTypes = (colorsResp != null && colorsResp.isSuccess) ? colorsResp.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.MetalTypeDto>();

                var claritiesResp = await claritiesTask;
                ViewBag.Clarities = (claritiesResp != null && claritiesResp.isSuccess) ? claritiesResp.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto>();

                var stoneTypesResp = await stoneTypesTask;
                ViewBag.StoneTypes = (stoneTypesResp != null && stoneTypesResp.isSuccess) ? stoneTypesResp.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto>();

                var stoneResponse = await stonesTask;
                ViewBag.Stones = (stoneResponse != null && stoneResponse.isSuccess) ? stoneResponse.data : new List<naif_katalog.Models.StoneDto>();
            } catch {
                ViewBag.MetalTypes = new List<naif_katalog.Core.Features.DefinitionFeature.Queries.MetalTypeDto>();
                ViewBag.Clarities = new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto>();
                ViewBag.StoneTypes = new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto>();
                ViewBag.Stones = new List<naif_katalog.Models.StoneDto>();
            }

            return View(pagedProductsFromBackend);
        }

        var products = new List<Product>();
        
        // --- Cache Logic START ---
        var cacheKey = "CachedProducts";
        List<Product>? fetchedProducts = null;

        if (!_memoryCache.TryGetValue(cacheKey, out fetchedProducts))
        {
            fetchedProducts = new List<Product>();
            
            var allResponse = await _mediator.Send(new GetAllProductsQueryRequest());
            if (allResponse.isSuccess && allResponse.data != null)
                fetchedProducts = allResponse.data;
            else
                fetchedProducts.Add(new Product { Code = "API_ERROR", CategoryNames = new List<string> { "HATA" }, Images = new List<string>() });
            
            if (fetchedProducts != null && !fetchedProducts.Any(p => p.Code == "API_ERROR"))
            {
                var cacheEntryOptions = new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(24));
                _memoryCache.Set(cacheKey, fetchedProducts, cacheEntryOptions);
            }
        }
        // --- Cache Logic END ---
        
        // In-memory Category Filtering
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            fetchedProducts = fetchedProducts.Where(x => x.CategoryIds != null && x.CategoryIds.Contains(categoryId.Value)).ToList();
        }

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
                .SelectMany(p => p.ProductMetals ?? new List<naif_katalog.Core.Features.ProductFeature.Queries.ApiProductMetal>())
                .Where(pm => pm != null && pm.MetalTypeId > 0 && !string.IsNullOrEmpty(pm.MetalTypeName))
                .GroupBy(m => m.MetalTypeId)
                .Select(g => new naif_katalog.Core.Features.DefinitionFeature.Queries.MetalTypeDto { Id = g.Key, Name = g.First().MetalTypeName })
                .ToList();
        }

        var clarityList = ViewBag.Clarities as List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto>;
        if (clarityList == null || !clarityList.Any())
        {
            ViewBag.Clarities = fetchedProducts
                .SelectMany(p => p.ProductStones ?? new List<naif_katalog.Core.Features.ProductFeature.Queries.ApiProductStone>())
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
        
        // --- Pagination Logic START ---
        int pageSize = 12;
        int totalProducts = products.Count;
        int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;
        
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalProducts;
        
        var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        // --- Pagination Logic END ---

        return View(pagedProducts);
    }
}

