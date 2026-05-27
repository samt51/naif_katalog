using Microsoft.AspNetCore.Mvc;
using MediatR;
using naif_katalog.Core.Features.ProductFeature.Queries;
using System.Dynamic;
using naif_katalog.Core.Features.CategoryFeature.Queries;

namespace naif_katalog.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ProductController : Controller
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            dynamic model = new ExpandoObject();
            
            var prodResponse = await _mediator.Send(new GetAllProductsQueryRequest());
            model.Products = prodResponse.isSuccess ? prodResponse.data : new List<naif_katalog.Models.Product>();

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

            var clarityResponse = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneClaritysQueryRequest());
            model.StoneClarities = clarityResponse.isSuccess ? clarityResponse.data : new List<naif_katalog.Core.Features.DefinitionFeature.Queries.StoneClarityDto>();

            return View("~/Views/Admin/Products.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var prodResponse = await _mediator.Send(new GetAllProductsQueryRequest());
            if (prodResponse.isSuccess)
            {
                var product = prodResponse.data.FirstOrDefault(p => p.Id == id);
                return Json(new { isSuccess = true, data = product });
            }
            return Json(new { isSuccess = false });
        }
    }
}

