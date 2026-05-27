using Microsoft.AspNetCore.Mvc;
using MediatR;
using naif_katalog.Core.Features.DefinitionFeature.Queries;
using naif_katalog.Core.Features.DefinitionFeature.Commands;
using System.Dynamic;

namespace naif_katalog.Controllers
{
 
    public class DefinitionController : Controller
    {
        private readonly IMediator _mediator;

        public DefinitionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            dynamic model = new ExpandoObject();

            model.StoneTypes = (await _mediator.Send(new GetAllStoneTypesQueryRequest()))?.data;
            model.StoneCuts = (await _mediator.Send(new GetAllStoneCutsQueryRequest()))?.data;
            model.StoneClarities = (await _mediator.Send(new GetAllStoneClaritysQueryRequest()))?.data;
            model.StoneScales = (await _mediator.Send(new GetAllStoneScalesQueryRequest()))?.data;

            model.MetalTypes = (await _mediator.Send(new GetAllMetalTypesQueryRequest()))?.data;
            model.MetalPurities = (await _mediator.Send(new GetAllMetalPuritysQueryRequest()))?.data;
            model.Colors = (await _mediator.Send(new GetAllColorsQueryRequest()))?.data;
            model.Roles = (await _mediator.Send(new GetAllRolesQueryRequest()))?.data;
            model.Units = (await _mediator.Send(new GetAllUnitsQueryRequest()))?.data;
            model.StoneSettings = (await _mediator.Send(new naif_katalog.Core.Features.StoneSettingFeature.Queries.GetAllStoneSettingQueryRequest()))?.data;

            // Fetch Polishing Costs directly from API for simplicity
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new System.Uri("https://localhost:3434/");
                try {
                    var pcResp = client.GetAsync("api/PolishingCost").Result;
                    if (pcResp.IsSuccessStatusCode) {
                        var pcStr = pcResp.Content.ReadAsStringAsync().Result;
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = System.Text.Json.JsonSerializer.Deserialize<PolishingCostResponse>(pcStr, options);
                        model.PolishingCosts = result?.PolishingCosts;
                    }
                    
                    var catResp = client.GetAsync("api/Category").Result;
                    if (catResp.IsSuccessStatusCode) {
                        var catStr = catResp.Content.ReadAsStringAsync().Result;
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var catResult = System.Text.Json.JsonSerializer.Deserialize<CategoryResponse>(catStr, options);
                        model.Categories = catResult?.Data;
                    }
                } catch {}
            }

            return View("~/Views/Admin/Definitions.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string type, string name, decimal? discountRate = 0)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest("İsim boş olamaz");

            object response = null;

            switch (type)
            {
                case "Color": response = await _mediator.Send(new CreateColorsCommandRequest { Name = name }); break;
                case "MetalPurity": response = await _mediator.Send(new CreateMetalPuritysCommandRequest { Name = name }); break;
                case "MetalType": response = await _mediator.Send(new CreateMetalTypesCommandRequest { Name = name }); break;
                case "Role": response = await _mediator.Send(new CreateRolesCommandRequest { Name = name }); break;
                case "StoneClarity": response = await _mediator.Send(new CreateStoneClaritysCommandRequest { Name = name }); break;
                case "StoneCut": response = await _mediator.Send(new CreateStoneCutsCommandRequest { Name = name }); break;
                case "StoneType": response = await _mediator.Send(new CreateStoneTypesCommandRequest { Name = name }); break;
                case "StoneScale": response = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Commands.CreateStoneScalesCommandRequest { Name = name }); break;
                case "Unit": response = await _mediator.Send(new CreateUnitsCommandRequest { Name = name }); break;
            }

            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string type, int id, string name, decimal? discountRate = 0)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest("İsim boş olamaz");

            object response = null;

            switch (type)
            {
                case "Color": response = await _mediator.Send(new UpdateColorsCommandRequest { Id = id, Name = name }); break;
                case "MetalPurity": response = await _mediator.Send(new UpdateMetalPuritysCommandRequest { Id = id, Name = name }); break;
                case "MetalType": response = await _mediator.Send(new UpdateMetalTypesCommandRequest { Id = id, Name = name }); break;
                case "Role": response = await _mediator.Send(new UpdateRolesCommandRequest { Id = id, Name = name }); break;
                case "StoneClarity": response = await _mediator.Send(new UpdateStoneClaritysCommandRequest { Id = id, Name = name }); break;
                case "StoneCut": response = await _mediator.Send(new UpdateStoneCutsCommandRequest { Id = id, Name = name }); break;
                case "StoneType": response = await _mediator.Send(new UpdateStoneTypesCommandRequest { Id = id, Name = name }); break;
                case "StoneScale": response = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Commands.UpdateStoneScalesCommandRequest { Id = id, Name = name }); break;
                case "Unit": response = await _mediator.Send(new UpdateUnitsCommandRequest { Id = id, Name = name }); break;
            }

            return Json(response);
        }
    }
    public class PolishingCostResponse { public System.Collections.Generic.List<PolishingCostItem> PolishingCosts { get; set; } }
    public class PolishingCostItem { public int Id { get; set; } public int CategoryId { get; set; } public string CategoryName { get; set; } public decimal CostPrice { get; set; } }
    public class CategoryResponse { public System.Collections.Generic.List<CategoryItem> Data { get; set; } }
    public class CategoryItem { public int Id { get; set; } public string Name { get; set; } }
}

