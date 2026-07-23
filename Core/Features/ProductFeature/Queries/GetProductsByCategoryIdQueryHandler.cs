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
    public class GetProductsByCategoryIdQueryHandler : BaseHandler, IRequestHandler<GetProductsByCategoryIdQueryRequest, ResponseDto<List<GetProductsByCategoryIdQueryResponse>>>
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public GetProductsByCategoryIdQueryHandler(IApiService apiService, IConfiguration configuration, IWebHostEnvironment environment) : base(apiService)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<ResponseDto<List<GetProductsByCategoryIdQueryResponse>>> Handle(GetProductsByCategoryIdQueryRequest request, CancellationToken cancellationToken)
        {
            var localAddress = _configuration["LocalAddress"] ?? "https://localhost:3434/";
            if (!localAddress.EndsWith("/")) localAddress += "/";

            var apiResult = await _apiService.GetAsync<List<ApiProduct>>($"api/Products/category/{request.CategoryId}");

            if (apiResult.isSuccess && apiResult.data != null)
            {
                var products = new List<GetProductsByCategoryIdQueryResponse>();
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

                    products.Add(new GetProductsByCategoryIdQueryResponse
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Name = item.Name,
                        ImageName = ProductImageExists(item.ImageName) ? BuildImageUrl(localAddress, item.ImageName) : "",
                        CategoryNames = item.CategoryNames,
                        CategoryIds = item.CategoryIds,
                        Description = item.Description,
                        Gram = item.Gram,
                        Karat = item.DiamondCarat > 0 ? item.DiamondCarat.ToString("N2") + " ct" : "-",
                        MetalPurityName = item.MetalPurityName,
                        DiamondCarat = item.DiamondCarat,
                        ColorId = item.ColorId,
                        ColorName = item.ColorName,
                        LiveGoldPrice = item.LiveGoldPrice,
                        CalculatedPrice = item.CalculatedPrice,
                        Images = imageUrls
                    });
                }
                return new ResponseDto<List<GetProductsByCategoryIdQueryResponse>>().Success(products);
            }

            var err = apiResult.errors != null && apiResult.errors.Count > 0 ? string.Join(", ", apiResult.errors) : "Hata";
            return new ResponseDto<List<GetProductsByCategoryIdQueryResponse>>().Fail(err);
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
}
