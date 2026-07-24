using Microsoft.AspNetCore.Mvc;
using MediatR;
using naif_katalog.Core.Features.CategoryFeature.Commands;
using naif_katalog.Core.Features.CategoryFeature.Queries;
using naif_katalog.Core.Features.ProductFeature.Queries;

namespace naif_katalog.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class CategoryController : Controller
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _mediator.Send(new GetAllCategoriesQueryRequest());
            if (response.isSuccess)
            {
                return View("~/Views/Admin/Categories.cshtml", response.data);
            }
            return View("~/Views/Admin/Categories.cshtml", null);
        }

        [HttpGet]
        public async Task<IActionResult> Products(int categoryId)
        {
            if (categoryId <= 0) return BadRequest(new { isSuccess = false, message = "Geçersiz kategori." });

            var response = await _mediator.Send(new GetProductsByCategoryIdQueryRequest { CategoryId = categoryId });
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string Name, int ParentId = 0)
        {
            if (string.IsNullOrEmpty(Name)) return BadRequest("Kategori adı boş olamaz");
            var response = await _mediator.Send(new CreateCategoryCommandRequest { Name = Name, ParentId = ParentId });
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int Id, string Name, int ParentId = 0)
        {
            if (string.IsNullOrEmpty(Name)) return BadRequest("Kategori adı boş olamaz");
            var response = await _mediator.Send(new UpdateCategoryCommandRequest { Id = Id, Name = Name, ParentId = ParentId });
            return Json(response);
        }
    }
}
