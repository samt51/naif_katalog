using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using atasay_katalog.Services.Abstract;

namespace atasay_katalog.Controllers;

[Authorize]
public sealed class CustomerDataController(IApiService api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Translation(string entityType, int entityId, string languageCode = "en")
    {
        if (entityId <= 0 || languageCode is not ("en" or "tr") ||
            !new[] { "Category", "MetalPurity", "MetalType", "StoneClarity", "StoneType", "Color" }.Contains(entityType))
            return BadRequest();
        var result = await api.GetAsync<string>($"api/definition-translations?entityType={Uri.EscapeDataString(entityType)}&entityId={entityId}&languageCode={languageCode}");
        return Json(result);
    }
}
