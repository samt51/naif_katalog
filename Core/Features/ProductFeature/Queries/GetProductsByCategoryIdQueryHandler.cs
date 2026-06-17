using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace naif_katalog.Core.Features.ProductFeature.Queries
{
    public class GetProductsByCategoryIdQueryHandler : BaseHandler, IRequestHandler<GetProductsByCategoryIdQueryRequest, ResponseDto<List<GetProductsByCategoryIdQueryResponse>>>
    {
        private readonly IConfiguration _configuration;

        public GetProductsByCategoryIdQueryHandler(IApiService apiService, IConfiguration configuration) : base(apiService)
        {
            _configuration = configuration;
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
                    
                    if (!string.IsNullOrEmpty(item.ImageName))
                    {
                        imageUrls.Add($"{localAddress}images/katalog/" + item.ImageName);
                    }

                    if (item.Images != null)
                    {
                        foreach(var img in item.Images)
                        {
                            var fullPath = $"{localAddress}images/katalog/" + img;
                            if (!imageUrls.Contains(fullPath))
                                imageUrls.Add(fullPath);
                        }
                    }

                    products.Add(new GetProductsByCategoryIdQueryResponse
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Name = item.Name,
                        ImageName = !string.IsNullOrEmpty(item.ImageName) ? $"{localAddress}images/katalog/" + item.ImageName : "",
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
    }
}
