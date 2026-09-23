using Microsoft.AspNetCore.Mvc;
using MediatR;
using atasay_katalog.Core.Features.ProductFeature.Queries;
using System.Dynamic;
using atasay_katalog.Core.Features.CategoryFeature.Queries;
using Microsoft.Extensions.Caching.Memory;

namespace atasay_katalog.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ProductController : Controller
    {
    private string CustomerCacheKey(string key)
    {
        var token = Request.Cookies["Atasay.ApiToken"] ?? "";
        var scope = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
        return $"Atasay:{scope}:{key}";
    }

        private readonly IMediator _mediator;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private readonly IWebHostEnvironment _environment;

        public ProductController(IMediator mediator, Microsoft.Extensions.Configuration.IConfiguration configuration, Microsoft.Extensions.Caching.Memory.IMemoryCache cache, IWebHostEnvironment environment)
        {
            _mediator = mediator;
            _configuration = configuration;
            _cache = cache;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try 
            {
                if (_cache.TryGetValue(CustomerCacheKey($"Product_{id}"), out atasay_katalog.Models.Product cachedProduct))
                {
                    return Json(new { isSuccess = true, data = CreateProductDetailResponse(cachedProduct) });
                }

                if (!_cache.TryGetValue(CustomerCacheKey("Products"), out atasay_katalog.Models.ResponseDto<List<atasay_katalog.Models.Product>> prodResponse))
                {
                    prodResponse = await _mediator.Send(new GetAllProductsQueryRequest { Page = 1, PageSize = 10 });
                    if (prodResponse != null && prodResponse.isSuccess)
                    {
                        _cache.Set(CustomerCacheKey("Products"), prodResponse, TimeSpan.FromMinutes(10));
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

        private static object CreateProductDetailResponse(atasay_katalog.Models.Product product)
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
            if (!_cache.TryGetValue(CustomerCacheKey("Products"), out atasay_katalog.Models.ResponseDto<List<atasay_katalog.Models.Product>> prodResponse))
            {
                prodResponse = await _mediator.Send(new GetAllProductsQueryRequest());
                if (prodResponse != null && prodResponse.isSuccess)
                {
                    _cache.Set(CustomerCacheKey("Products"), prodResponse, TimeSpan.FromMinutes(10));
                }
            }

            if (prodResponse != null && prodResponse.isSuccess)
            {
                var product = prodResponse.data.FirstOrDefault(p => string.Equals(p.Code, code, System.StringComparison.OrdinalIgnoreCase));
                return Json(new { isSuccess = product != null, data = product });
            }
            return Json(new { isSuccess = false });
        }

    }
}

