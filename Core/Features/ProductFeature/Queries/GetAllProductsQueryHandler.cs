using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace naif_katalog.Core.Features.ProductFeature.Queries
{
    public class GetAllProductsQueryHandler : BaseHandler, IRequestHandler<GetAllProductsQueryRequest, ResponseDto<List<Product>>>
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public GetAllProductsQueryHandler(IApiService apiService, IConfiguration configuration, IWebHostEnvironment environment) : base(apiService)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<ResponseDto<List<Product>>> Handle(GetAllProductsQueryRequest request, CancellationToken cancellationToken)
        {
            try
            {

       
            var localAddress = _configuration["LocalAddress"] ?? "https://localhost:3434/";
            if (!localAddress.EndsWith("/")) localAddress += "/";

            var qs = new List<string>();
            if (!string.IsNullOrEmpty(request.Code)) qs.Add($"Code={System.Net.WebUtility.UrlEncode(request.Code)}");
            if (!string.IsNullOrEmpty(request.Category)) qs.Add($"Category={System.Net.WebUtility.UrlEncode(request.Category)}");
            if (request.MinGram.HasValue) qs.Add($"MinGram={request.MinGram.Value}");
            if (request.MaxGram.HasValue) qs.Add($"MaxGram={request.MaxGram.Value}");
            if (request.MinPrice.HasValue) qs.Add($"MinPrice={request.MinPrice.Value}");
            if (request.MaxPrice.HasValue) qs.Add($"MaxPrice={request.MaxPrice.Value}");
            if (request.MetalTypeId.HasValue) qs.Add($"MetalTypeId={request.MetalTypeId.Value}");
            if (request.ClarityId.HasValue) qs.Add($"ClarityId={request.ClarityId.Value}");
            if (request.StoneId.HasValue) qs.Add($"StoneId={request.StoneId.Value}");
            if (request.StoneTypeId.HasValue) qs.Add($"StoneTypeId={request.StoneTypeId.Value}");
            qs.Add($"page={request.Page}");
            qs.Add($"pageSize={request.PageSize}");
            if (request.ColumnIndex.HasValue) qs.Add($"columnIndex={request.ColumnIndex.Value}");
            if (!string.IsNullOrEmpty(request.OrderBy)) qs.Add($"orderBy={System.Net.WebUtility.UrlEncode(request.OrderBy)}");
            qs.Add($"applyCustomerPricing={request.ApplyCustomerPricing.ToString().ToLowerInvariant()}");
            
            var url = "api/Products" + (qs.Any() ? "?" + string.Join("&", qs) : "");

            var apiResult = await _apiService.GetAsync<List<ApiProduct>>(url);
            
            if (apiResult.isSuccess && apiResult.data != null)
            {
                var products = new List<Product>();
                foreach (var item in apiResult.data)
                {
                    var imageUrls = new List<string>();
                    
                    if (ProductImageExists(item.ImageName))
                    {
                        imageUrls.Add(BuildImageUrl(localAddress, item.ImageName));
                    }

                    if (item.Images != null)
                    {
                        foreach(var img in item.Images)
                        {
                            var fullPath = BuildImageUrl(localAddress, img);
                            if (ProductImageExists(img) && !imageUrls.Contains(fullPath))
                                imageUrls.Add(fullPath);
                        }
                    }

                    products.Add(new Product
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Name = item.Name,
                        CategoryNames = item.CategoryNames,
                        CategoryIds = item.CategoryIds,
                        Description = item.Description,
                        Gram = item.Gram,
                        Karat = item.DiamondCarat > 0 ? item.DiamondCarat.ToString("N2") + " ct" : "-",
                        MetalPurityName = item.MetalPurityName,
                        DiamondCarat = item.DiamondCarat,
                        ColorId = item.ColorId,
                        ColorName = item.ColorName,
                        CalculatedPrice = item.CalculatedPrice,
                        LiveGoldPrice = item.LiveGoldPrice,
                        LaborMultiplier = item.LaborMultiplier,
                        PolishingCost = item.PolishingCost,
                        Images = imageUrls,
                        ProductStones = item.ProductStones ?? new List<ApiProductStone>(),
                        ProductMetals = item.ProductMetals ?? new List<ApiProductMetal>()
                    });
                }
                var response = new ResponseDto<List<Product>>().Success(products);
                response.count = apiResult.count > 0 ? apiResult.count : products.Count;
                return response;
            }

            var err = apiResult.errors != null && apiResult.errors.Count > 0 ? string.Join(", ", apiResult.errors) : "Hata";
            return new ResponseDto<List<Product>>().Fail(err);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private bool ProductImageExists(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)) return false;

            var relativePath = imageName.Replace('\\', '/').TrimStart('/');
            var catalogRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "images", "katalog"));
            var fullPath = Path.GetFullPath(Path.Combine(catalogRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

            return fullPath.StartsWith(catalogRoot + Path.DirectorySeparatorChar, System.StringComparison.OrdinalIgnoreCase)
                && File.Exists(fullPath);
        }

        private string BuildImageUrl(string localAddress, string imageName)
        {
            var relativePath = imageName.Replace('\\', '/').TrimStart('/');
            var fullPath = Path.GetFullPath(Path.Combine(
                _environment.WebRootPath,
                "images",
                "katalog",
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
            var version = File.GetLastWriteTimeUtc(fullPath).Ticks;
            return $"{localAddress}images/katalog/{relativePath}?v={version}";
        }
    }

    public class ApiProduct
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string ImageName { get; set; }
        public string Name { get; set; }
        public List<string> CategoryNames { get; set; }
        public List<int> CategoryIds { get; set; }
        public string Description { get; set; }
        public decimal Gram { get; set; }
        public string Karat { get; set; }
        public string MetalPurityName { get; set; }
        public decimal DiamondCarat { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public decimal CalculatedPrice { get; set; }
        public decimal LiveGoldPrice { get; set; }
        public decimal LaborMultiplier { get; set; }
        public decimal PolishingCost { get; set; }
        public List<string> Images { get; set; }
        public List<ApiProductStone> ProductStones { get; set; }
        public List<ApiProductMetal> ProductMetals { get; set; }
    }

    public class ApiProductStone
    {
        public int Id { get; set; }
        public int StoneId { get; set; }
        public string StoneName { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public int? ClarityId { get; set; }
        public string ClarityName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Carat { get; set; }
        public decimal TotalCarat { get; set; }
    }

    public class ApiProductMetal
    {
        public int Id { get; set; }
        public int MetalTypeId { get; set; }
        public string MetalTypeName { get; set; }
        public int? MetalPurityId { get; set; }
        public string MetalPurityName { get; set; }
        public decimal Gram { get; set; }
    }
}
