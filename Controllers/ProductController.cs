using Microsoft.AspNetCore.Mvc;
using MediatR;
using naif_katalog.Core.Features.ProductFeature.Queries;
using System.Dynamic;
using naif_katalog.Core.Features.CategoryFeature.Queries;
using Microsoft.Extensions.Caching.Memory;

namespace naif_katalog.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ProductController : Controller
    {
        private readonly IMediator _mediator;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        public ProductController(IMediator mediator, Microsoft.Extensions.Configuration.IConfiguration configuration, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _mediator = mediator;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<IActionResult> Index(string? code, string? category, decimal? minGram, decimal? maxGram, int? metalTypeId, int? clarityId, int? stoneId, int? stoneTypeId)
        {
            dynamic model = new ExpandoObject();
            model.Products = new List<naif_katalog.Models.Product>();

            var catResponse = await _mediator.Send(new GetAllCategoriesQueryRequest());
            model.Categories = catResponse.isSuccess ? catResponse.data : new List<naif_katalog.Models.CategoryDto>();

            var colorResponse = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllColorsQueryRequest());
            model.Colors = colorResponse.isSuccess ? colorResponse.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.ColorDto>();

            var metalTypeResponse = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllMetalTypesQueryRequest());
            model.MetalTypes = metalTypeResponse.isSuccess ? metalTypeResponse.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.MetalTypeDto>();

            var metalPurityResponse = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllMetalPuritysQueryRequest());
            model.MetalPurities = metalPurityResponse.isSuccess ? metalPurityResponse.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.MetalPurityDto>();

            var stoneResponse = await _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Queries.GetAllStonesQueryRequest());
            model.Stones = stoneResponse.isSuccess ? stoneResponse.data : new List<naif_katalog.Models.StoneDto>();

            var stoneTypeResponse = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneTypesQueryRequest());
            model.StoneTypes = stoneTypeResponse.isSuccess ? stoneTypeResponse.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneTypeDto>();

            var clarityResponse = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneClaritysQueryRequest());
            model.StoneClarities = clarityResponse.isSuccess ? clarityResponse.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto>();

            return View("~/Views/Admin/Products.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> IndexData(
            string? code,
            string? category,
            decimal? minGram,
            decimal? maxGram,
            int? metalTypeId,
            int? clarityId,
            int? stoneId,
            int? stoneTypeId,
            int page = 1,
            int pageSize = 10,
            int? columnIndex = null,
            string? orderBy = null,
            int draw = 1,
            int start = 0,
            int length = 10)
        {
            pageSize = pageSize > 0 ? pageSize : (length > 0 ? length : 10);
            page = page > 0 ? page : (start / pageSize) + 1;
            columnIndex ??= 0;
            orderBy = string.Equals(orderBy, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

            if (Request.Query.TryGetValue("order[0][column]", out var orderColumnValue)
                && int.TryParse(orderColumnValue.ToString(), out var parsedColumn))
            {
                columnIndex = parsedColumn;
            }

            if (Request.Query.TryGetValue("order[0][dir]", out var orderDirectionValue))
            {
                var direction = orderDirectionValue.ToString();
                orderBy = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
            }

            var prodResponse = await _mediator.Send(new GetAllProductsQueryRequest
            {
                Code = code,
                Category = category,
                MinGram = minGram,
                MaxGram = maxGram,
                MetalTypeId = metalTypeId,
                ClarityId = clarityId,
                StoneId = stoneId,
                StoneTypeId = stoneTypeId,
                Page = page,
                PageSize = pageSize,
                ColumnIndex = columnIndex,
                OrderBy = orderBy
            });

            var products = prodResponse != null && prodResponse.isSuccess && prodResponse.data != null
                ? prodResponse.data
                : new List<naif_katalog.Models.Product>();

            foreach (var product in products)
            {
                _cache.Set($"CachedProduct_{product.Id}", product, TimeSpan.FromMinutes(10));
            }

            var count = prodResponse != null && prodResponse.count > 0 ? prodResponse.count : products.Count;

            return Json(new
            {
                draw,
                recordsTotal = count,
                recordsFiltered = count,
                data = products.Select(item => new
                {
                    item.Id,
                    item.Name,
                    item.Code,
                    item.CategoryNames,
                    item.DiamondCarat,
                    item.Gram,
                    item.CalculatedPrice,
                    imageUrl = item.Images != null && item.Images.Count > 0 ? item.Images[0] : null
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try 
            {
                if (_cache.TryGetValue($"CachedProduct_{id}", out naif_katalog.Models.Product cachedProduct))
                {
                    return Json(new { isSuccess = true, data = CreateProductDetailResponse(cachedProduct) });
                }

                if (!_cache.TryGetValue("CachedProducts", out naif_katalog.Models.ResponseDto<List<naif_katalog.Models.Product>> prodResponse))
                {
                    prodResponse = await _mediator.Send(new GetAllProductsQueryRequest { Page = 1, PageSize = 10 });
                    if (prodResponse != null && prodResponse.isSuccess)
                    {
                        _cache.Set("CachedProducts", prodResponse, TimeSpan.FromMinutes(10));
                    }
                }

                if (prodResponse != null && prodResponse.isSuccess)
                {
                    var product = prodResponse.data.FirstOrDefault(p => p.Id == id);
                    if (product == null)
                    {
                        return Json(new { isSuccess = false, errors = new[] { "Ürün bulunamadı." } });
                    }

                    return Json(new { isSuccess = true, data = CreateProductDetailResponse(product) });
                }
                return Json(new { isSuccess = false });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, errors = new[] { ex.Message, ex.StackTrace } });
            }
        }

        private static object CreateProductDetailResponse(naif_katalog.Models.Product product)
        {
            return new
            {
                product.Id,
                product.Code,
                product.Name,
                product.Description,
                product.CategoryIds,
                product.ColorId,
                product.Gram,
                product.MetalPurityName,
                product.LaborMultiplier,
                product.PolishingCost,
                product.LiveGoldPrice,
                product.Images,
                product.ProductStones,
                product.ProductMetals
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetByCode(string code)
        {
            if (!_cache.TryGetValue("CachedProducts", out naif_katalog.Models.ResponseDto<List<naif_katalog.Models.Product>> prodResponse))
            {
                prodResponse = await _mediator.Send(new GetAllProductsQueryRequest());
                if (prodResponse != null && prodResponse.isSuccess)
                {
                    _cache.Set("CachedProducts", prodResponse, TimeSpan.FromMinutes(10));
                }
            }

            if (prodResponse != null && prodResponse.isSuccess)
            {
                var product = prodResponse.data.FirstOrDefault(p => string.Equals(p.Code, code, System.StringComparison.OrdinalIgnoreCase));
                return Json(new { isSuccess = product != null, data = product });
            }
            return Json(new { isSuccess = false });
        }

        [HttpPost]
        public IActionResult ClearCache()
        {
            _cache.Remove("CachedProducts");
            return Json(new { isSuccess = true });
        }
    }
}

