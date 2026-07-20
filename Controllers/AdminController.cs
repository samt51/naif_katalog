using Microsoft.AspNetCore.Mvc;
using naif_katalog.Core.Features.StoneFeature.Queries;
using naif_katalog.Core.Features.CategoryFeature.Queries;
using MediatR;
using naif_katalog.Core.Features.ProductFeature.Queries;
using naif_katalog.Core.Features.CategoryFeature.Queries;
using naif_katalog.Core.Features.UsersFeature.Queries;
using Microsoft.Extensions.Caching.Memory;
using naif_katalog.Core.Features.DefinitionFeature.Queries;
using naif_katalog.Core.Features.DefinitionFeature.Commands;
using System.Dynamic;

namespace naif_katalog.Controllers
{
   
    public class AdminController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        public AdminController(IMediator mediator, IConfiguration configuration, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _mediator = mediator;
            _configuration = configuration;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> ExportCustomerCatalog(int customerId, string format = "excel", string lang = "tr")
        {
            if (customerId <= 0) return BadRequest("Geçersiz müşteri.");
            format = string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase) ? "pdf" : "excel";
            lang = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "tr";
            using var client = new HttpClient();
            var apiAddress = _configuration["ApiAdress"] ?? "https://apib2b.naifjewellery.com/";
            if (!apiAddress.EndsWith("/")) apiAddress += "/";
            var response = await client.GetAsync($"{apiAddress}api/customers/{customerId}/catalog-export/{format}?lang={lang}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var contentType = response.Content.Headers.ContentType?.ToString()
                ?? (format == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? $"musteri_{customerId}_ozel_katalog.{(format == "pdf" ? "pdf" : "xlsx")}";
            return File(await response.Content.ReadAsByteArrayAsync(), contentType, fileName);
        }

        

        
            
            

        public async Task<IActionResult> Dashboard()
        {
            // Products
            if (!_cache.TryGetValue("CachedProducts", out naif_katalog.Models.ResponseDto<List<naif_katalog.Models.Product>> prodResponse))
            {
                prodResponse = await _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Queries.GetAllProductsQueryRequest());
                if (prodResponse != null && prodResponse.isSuccess)
                {
                    _cache.Set("CachedProducts", prodResponse, TimeSpan.FromMinutes(10));
                }
            }
            int productCount = prodResponse?.data?.Count ?? 0;
            var recentProducts = prodResponse?.data?.OrderByDescending(p => p.Id).Take(5).ToList() ?? new List<naif_katalog.Models.Product>();

            // Users
            var usersResponse = await _mediator.Send(new GetAllUsersQueryRequest());
            int userCount = usersResponse?.data?.Count ?? 0;

            // Categories
            var categoriesResponse = await _mediator.Send(new GetAllCategoriesQueryRequest());
            int categoryCount = categoriesResponse?.data?.Count ?? 0;

            ViewBag.ProductCount = productCount;
            ViewBag.UserCount = userCount;
            ViewBag.CategoryCount = categoryCount;
            ViewBag.RecentProducts = recentProducts;

            return View();
        }


        
        public async Task<IActionResult> Customers()
        {
            var response = await _mediator.Send(new GetAllUsersQueryRequest());
            var stones = await _mediator.Send(new GetAllStoneQueryRequest());
            var categories = await _mediator.Send(new GetAllCategoriesQueryRequest());
            var clarities = await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneClaritysQueryRequest());
            var stoneTypes = await _mediator.Send(new GetAllStoneTypesQueryRequest());
            var units = await _mediator.Send(new GetAllUnitsQueryRequest());
            
            ViewBag.Stones = stones?.data;
            ViewBag.Categories = categories?.data;
            ViewBag.Clarities = clarities?.data;
            ViewBag.StoneTypes = stoneTypes?.data;
            ViewBag.Units = units?.data;

            var settingsResponse = await _mediator.Send(new naif_katalog.Core.Features.StoneSettingFeature.Queries.GetAllStoneSettingQueryRequest());
            ViewBag.Settings = settingsResponse?.data;


            using (var client = new System.Net.Http.HttpClient())
            {
                var apiAddress = _configuration["ApiAdress"] ?? "https://apib2b.naifjewellery.com/";
                if (!apiAddress.EndsWith("/")) apiAddress += "/";
                client.BaseAddress = new System.Uri(apiAddress);
                try {
                    var pcResp = client.GetAsync("api/PolishingCost").Result;
                    if (pcResp.IsSuccessStatusCode) {
                        var pcStr = pcResp.Content.ReadAsStringAsync().Result;
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = System.Text.Json.JsonSerializer.Deserialize<naif_katalog.Controllers.PolishingCostResponse>(pcStr, options);
                        ViewBag.PolishingCosts = result?.PolishingCosts;
                    }
                } catch { }
            }

            if (response != null && response.isSuccess && response.data != null)
            {
                return View(response.data);
            }
            return View(new System.Collections.Generic.List<naif_katalog.Models.UsersDto>());
        }

        public async Task<IActionResult> Definitions()
        {
            dynamic model = new ExpandoObject();

            model.StoneTypes = (await _mediator.Send(new GetAllStoneTypesQueryRequest()))?.data;
            model.StoneCuts = (await _mediator.Send(new GetAllStoneCutsQueryRequest()))?.data;
            model.StoneClarities = (await _mediator.Send(new GetAllStoneClaritysQueryRequest()))?.data;
            model.StoneSettings = (await _mediator.Send(new naif_katalog.Core.Features.StoneSettingFeature.Queries.GetAllStoneSettingQueryRequest()))?.data;
            model.StoneScales = (await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneScalesQueryRequest()))?.data;

            model.MetalTypes = (await _mediator.Send(new GetAllMetalTypesQueryRequest()))?.data;
            model.MetalPurities = (await _mediator.Send(new GetAllMetalPuritysQueryRequest()))?.data;
            model.Colors = (await _mediator.Send(new GetAllColorsQueryRequest()))?.data;

            model.CustomerGroups = (await _mediator.Send(new GetAllCustomerGroupsQueryRequest()))?.data;
            model.Roles = (await _mediator.Send(new GetAllRolesQueryRequest()))?.data;
            model.Units = (await _mediator.Send(new GetAllUnitsQueryRequest()))?.data;
                        model.StoneClarities = (await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneClaritysQueryRequest()))?.data;
            model.StoneSettings = (await _mediator.Send(new naif_katalog.Core.Features.StoneSettingFeature.Queries.GetAllStoneSettingQueryRequest()))?.data;
            model.StoneScales = (await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneScalesQueryRequest()))?.data;

            return View(model);
        }

        public async Task<IActionResult> Logs()
        {
            var response = await _mediator.Send(new naif_katalog.Core.Features.UserActionLogFeature.Queries.GetAllUserActionLogsQueryRequest());
            if (response.isSuccess)
            {
                return View(response.data);
            }
            return View(new List<naif_katalog.Models.UserActionLogDto>());
        }



        [HttpPost]
                [HttpPost]
        public async Task<IActionResult> CreateStoneSetting([FromBody] naif_katalog.Core.Features.StoneSettingFeature.Commands.CreateStoneSettingCommandRequest request)
        {
            var response = await _mediator.Send(request);
            return Json(new { isSuccess = response });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStoneSetting([FromBody] naif_katalog.Core.Features.StoneSettingFeature.Commands.UpdateStoneSettingCommandRequest request)
        {
            var response = await _mediator.Send(request);
            return Json(new { isSuccess = response });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteStoneSetting([FromBody] naif_katalog.Core.Features.StoneSettingFeature.Commands.DeleteStoneSettingCommandRequest request)
        {
            var response = await _mediator.Send(request);
            return Json(new { isSuccess = response });
        }

        public async Task<IActionResult> CreateDefinition(string type, string name)
        {
            if(string.IsNullOrEmpty(name)) return BadRequest("Ä°sim boÅŸ olamaz");

            object response = null;

            switch(type)
            {
                case "Color": response = await _mediator.Send(new CreateColorsCommandRequest { Name = name, Code = name, ColorType = "Stone" }); break;
                case "CustomerGroup": response = await _mediator.Send(new CreateCustomerGroupsCommandRequest { Name = name }); break;
                case "MetalPurity": response = await _mediator.Send(new CreateMetalPuritysCommandRequest { Name = name }); break;
                case "MetalType": response = await _mediator.Send(new CreateMetalTypesCommandRequest { Name = name }); break;
                case "Role": response = await _mediator.Send(new CreateRolesCommandRequest { Name = name }); break;
                case "StoneClarity": response = await _mediator.Send(new CreateStoneClaritysCommandRequest { Name = name }); break;
                case "StoneCut": response = await _mediator.Send(new CreateStoneCutsCommandRequest { Name = name }); break;
                case "StoneType": response = await _mediator.Send(new CreateStoneTypesCommandRequest { Name = name }); break;
                case "Unit": response = await _mediator.Send(new CreateUnitsCommandRequest { Name = name }); break;
            }

            return Json(response);
        }

        [HttpPost]
        
        [HttpPost]
        public async Task<IActionResult> UpdateDefinition(string type, int id, string name)
        {
            if(string.IsNullOrEmpty(name)) return BadRequest("Ä°sim boÅŸ olamaz");

            object response = null;

            switch(type)
            {
                case "Color": response = await _mediator.Send(new UpdateColorsCommandRequest { Id = id, Name = name, Code = name, ColorType = "Stone" }); break;
                case "CustomerGroup": response = await _mediator.Send(new UpdateCustomerGroupsCommandRequest { Id = id, Name = name }); break;
                case "MetalPurity": response = await _mediator.Send(new UpdateMetalPuritysCommandRequest { Id = id, Name = name }); break;
                case "MetalType": response = await _mediator.Send(new UpdateMetalTypesCommandRequest { Id = id, Name = name }); break;
                case "Role": response = await _mediator.Send(new UpdateRolesCommandRequest { Id = id, Name = name }); break;
                case "StoneClarity": response = await _mediator.Send(new UpdateStoneClaritysCommandRequest { Id = id, Name = name }); break;
                case "StoneCut": response = await _mediator.Send(new UpdateStoneCutsCommandRequest { Id = id, Name = name }); break;
                case "StoneType": response = await _mediator.Send(new UpdateStoneTypesCommandRequest { Id = id, Name = name }); break;
                case "Unit": response = await _mediator.Send(new UpdateUnitsCommandRequest { Id = id, Name = name }); break;
            }

            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(naif_katalog.Core.Features.UsersFeature.Commands.CreateUsersCommandRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new naif_katalog.Models.ResponseDto<naif_katalog.Core.Features.UsersFeature.Commands.CreateUsersCommandResponse> { isSuccess = false, errors = errors });
            }
            try
            {
                var response = await _mediator.Send(request);
                return Json(response ?? new naif_katalog.Models.ResponseDto<naif_katalog.Core.Features.UsersFeature.Commands.CreateUsersCommandResponse> { isSuccess = false, errors = new List<string> { "API yanÄ±t vermedi." } });
            }
            catch (Exception ex)
            {
                return Json(new naif_katalog.Models.ResponseDto<naif_katalog.Core.Features.UsersFeature.Commands.CreateUsersCommandResponse> { isSuccess = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(naif_katalog.Core.Features.UsersFeature.Commands.UpdateUsersCommandRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new naif_katalog.Models.ResponseDto<naif_katalog.Core.Features.UsersFeature.Commands.UpdateUsersCommandResponse> { isSuccess = false, errors = errors });
            }
            try
            {
                var response = await _mediator.Send(request);
                return Json(response ?? new naif_katalog.Models.ResponseDto<naif_katalog.Core.Features.UsersFeature.Commands.UpdateUsersCommandResponse> { isSuccess = false, errors = new List<string> { "API yanÄ±t vermedi." } });
            }
            catch (Exception ex)
            {
                return Json(new naif_katalog.Models.ResponseDto<naif_katalog.Core.Features.UsersFeature.Commands.UpdateUsersCommandResponse> { isSuccess = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpGet]
        public async Task<IActionResult> StoneLots()
        {
            var response = await _mediator.Send(new naif_katalog.Core.Features.StoneFeature.Queries.GetAllStoneQueryRequest());
            
            dynamic model = new ExpandoObject();
            model.Stones = response?.data;
            model.StoneTypes = (await _mediator.Send(new GetAllStoneTypesQueryRequest()))?.data;
            model.Units = (await _mediator.Send(new GetAllUnitsQueryRequest()))?.data;
                        model.StoneClarities = (await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneClaritysQueryRequest()))?.data;
            model.StoneSettings = (await _mediator.Send(new naif_katalog.Core.Features.StoneSettingFeature.Queries.GetAllStoneSettingQueryRequest()))?.data;
            model.StoneScales = (await _mediator.Send(new naif_katalog.Core.Features.DefinitionFeature.Queries.GetAllStoneScalesQueryRequest()))?.data;

            return View(model);
        }

        [HttpPost]
        public async Task<naif_katalog.Models.ResponseDto<naif_katalog.Models.CreateStoneCommandResponse>> CreateStone([FromBody] naif_katalog.Core.Features.StoneFeature.Commands.CreateStoneCommandRequest request)
        {
            return await _mediator.Send(request);
        }

        [HttpPut]
        public async Task<naif_katalog.Models.ResponseDto<naif_katalog.Models.UpdateStoneCommandResponse>> UpdateStone([FromBody] naif_katalog.Core.Features.StoneFeature.Commands.UpdateStoneCommandRequest request)
        {
            return await _mediator.Send(request);
        }

        [HttpDelete]
        public async Task<naif_katalog.Models.ResponseDto<naif_katalog.Models.DeleteStoneCommandResponse>> DeleteStone([FromBody] naif_katalog.Core.Features.StoneFeature.Commands.DeleteStoneCommandRequest request)
        {
            return await _mediator.Send(request);
        }

        [HttpPost]
                [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _cache.Remove("CachedProducts");
            var response = await _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Commands.DeleteProductCommandRequest { Id = id });
            return Json(response ?? new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new System.Collections.Generic.List<string> { "API yanï¿½t vermedi." } });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] naif_katalog.Core.Features.ProductFeature.Commands.CreateProductCommandRequest request, [FromForm] string? stonesJson, [FromForm] string? metalsJson, [FromForm] List<IFormFile>? imageFiles)
        {
            _cache.Remove("CachedProducts");
            try
            {
                if (!string.IsNullOrEmpty(stonesJson) && stonesJson != "undefined" && stonesJson != "null")
                    request.ProductStones = System.Text.Json.JsonSerializer.Deserialize<List<naif_katalog.Core.Features.ProductFeature.Commands.CreateProductStoneDto>>(stonesJson);
                
                if (!string.IsNullOrEmpty(metalsJson) && metalsJson != "undefined" && metalsJson != "null")
                    request.ProductMetals = System.Text.Json.JsonSerializer.Deserialize<List<naif_katalog.Core.Features.ProductFeature.Commands.CreateProductMetalDto>>(metalsJson);

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    request.ImageNames ??= new List<string>();
                    
                    var catResponse = await _mediator.Send(new naif_katalog.Core.Features.CategoryFeature.Queries.GetAllCategoriesQueryRequest());
                    string folderName = "DIGER";
                    if (catResponse != null && catResponse.isSuccess)
                    {
                        var cat = request.CategoryIds != null && request.CategoryIds.Any() ? catResponse.data.FirstOrDefault(x => request.CategoryIds.Contains(x.Id)) : null;
                        if (cat != null)
                        {
                            if (cat.ParentId > 0)
                            {
                                var parent = catResponse.data.FirstOrDefault(x => x.Id == cat.ParentId);
                                folderName = parent != null ? parent.Name.ToUpper() : cat.Name.ToUpper();
                            }
                            else
                            {
                                folderName = cat.Name.ToUpper();
                            }
                        }
                    }
                    
                    var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "katalog", folderName);
                    if (!System.IO.Directory.Exists(uploadsFolder))
                        System.IO.Directory.CreateDirectory(uploadsFolder);

                    int i = 1;
                    foreach (var file in imageFiles)
                    {
                        var ext = System.IO.Path.GetExtension(file.FileName);
                        var cleanCode = (request.Code ?? "URUN").ToUpper().Replace(" ", "").Replace("/", "-");
                        var fileName = $"{cleanCode}_{i}{ext}";
                        while(System.IO.File.Exists(System.IO.Path.Combine(uploadsFolder, fileName)))
                        {
                            i++;
                            fileName = $"{cleanCode}_{i}{ext}";
                        }
                        
                        var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        
                        request.ImageNames.Add($"{folderName}/{fileName}");
                        i++;
                    }
                }

                var response = await _mediator.Send(request);
                return Json(response ?? new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "API yanıt vermedi." } });
            }
            catch (System.Exception ex)
            {
                return Json(new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "Sunucu hatası: " + ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromForm] naif_katalog.Core.Features.ProductFeature.Commands.UpdateProductCommandRequest request, [FromForm] string? stonesJson, [FromForm] string? metalsJson, [FromForm] List<IFormFile>? imageFiles)
        {
            _cache.Remove("CachedProducts");
            try
            {
                if (!string.IsNullOrEmpty(stonesJson) && stonesJson != "undefined" && stonesJson != "null")
                    request.ProductStones = System.Text.Json.JsonSerializer.Deserialize<List<naif_katalog.Core.Features.ProductFeature.Commands.UpdateProductStoneDto>>(stonesJson);
                
                if (!string.IsNullOrEmpty(metalsJson) && metalsJson != "undefined" && metalsJson != "null")
                    request.ProductMetals = System.Text.Json.JsonSerializer.Deserialize<List<naif_katalog.Core.Features.ProductFeature.Commands.UpdateProductMetalDto>>(metalsJson);

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    request.ImageNames ??= new List<string>();
                    
                    var catResponse = await _mediator.Send(new naif_katalog.Core.Features.CategoryFeature.Queries.GetAllCategoriesQueryRequest());
                    string folderName = "DIGER";
                    if (catResponse != null && catResponse.isSuccess)
                    {
                        var cat = request.CategoryIds != null && request.CategoryIds.Any() ? catResponse.data.FirstOrDefault(x => request.CategoryIds.Contains(x.Id)) : null;
                        if (cat != null)
                        {
                            if (cat.ParentId > 0)
                            {
                                var parent = catResponse.data.FirstOrDefault(x => x.Id == cat.ParentId);
                                folderName = parent != null ? parent.Name.ToUpper() : cat.Name.ToUpper();
                            }
                            else
                            {
                                folderName = cat.Name.ToUpper();
                            }
                        }
                    }
                    
                    var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "katalog", folderName);
                    if (!System.IO.Directory.Exists(uploadsFolder))
                        System.IO.Directory.CreateDirectory(uploadsFolder);

                    int i = 1;
                    foreach (var file in imageFiles)
                    {
                        var ext = System.IO.Path.GetExtension(file.FileName);
                        var cleanCode = (request.Code ?? "URUN").ToUpper().Replace(" ", "").Replace("/", "-");
                        var fileName = $"{cleanCode}_{i}{ext}";
                        while(System.IO.File.Exists(System.IO.Path.Combine(uploadsFolder, fileName)))
                        {
                            i++;
                            fileName = $"{cleanCode}_{i}{ext}";
                        }
                        
                        var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        
                        request.ImageNames.Add($"{folderName}/{fileName}");
                        i++;
                    }
                }

                var response = await _mediator.Send(request);
                return Json(response ?? new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "API yanıt vermedi." } });
            }
            catch (System.Exception ex)
            {
                return Json(new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "Sunucu hatası: " + ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProductMetal([FromForm] naif_katalog.Core.Features.ProductFeature.Commands.CreateProductMetalCommandRequest request)
        {
            var response = await _mediator.Send(request);
            return Json(response ?? new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "API yanÄ±t vermedi." } });
        }

        [HttpPost]
        public async Task<IActionResult> AddProductStone([FromForm] naif_katalog.Core.Features.ProductFeature.Commands.CreateProductStoneCommandRequest request)
        {
            var response = await _mediator.Send(request);
            return Json(response ?? new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "API yanÄ±t vermedi." } });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductStone(int id)
        {
            var response = await _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Commands.DeleteProductStoneCommandRequest { Id = id });
            return Json(response ?? new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "API yanÄ±t vermedi." } });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductMetal(int id)
        {
            var response = await _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Commands.DeleteProductMetalCommandRequest { Id = id });
            return Json(response ?? new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "API yanÄ±t vermedi." } });
        }

        [HttpPost]
        public async Task<IActionResult> UploadProductImage(Microsoft.AspNetCore.Http.IFormFile file, int productId)
        {
            if (file == null || file.Length == 0)
                return Json(new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "Dosya seÃ§ilmedi." } });

            try
            {
                var handler = new System.Net.Http.HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using (var client = new System.Net.Http.HttpClient(handler))
                {
                    var apiAddress = _configuration["ApiAdress"] ?? "https://apib2b.naifjewellery.com/";
                    if (!apiAddress.EndsWith("/")) apiAddress += "/";
                    client.BaseAddress = new System.Uri(apiAddress);
                    using (var content = new System.Net.Http.MultipartFormDataContent())
                    {
                        var fileContent = new System.Net.Http.StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        content.Add(fileContent, "file", file.FileName);
                        content.Add(new System.Net.Http.StringContent(productId.ToString()), "productId");

                        var response = await client.PostAsync("api/ProductImage/upload", content);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            return Content(json, "application/json");
                        }
                    }
                }
                return Json(new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "Sunucu hatasÄ±: Resim yÃ¼klenemedi." } });
            }
            catch (System.Exception ex)
            {
                return Json(new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductImage(int productId, string imageName)
        {
            _cache.Remove("CachedProducts");

            if (productId <= 0 || string.IsNullOrWhiteSpace(imageName))
            {
                return Json(new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "Silinecek resim bilgisi eksik." } });
            }

            try
            {
                var handler = new System.Net.Http.HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using var client = new System.Net.Http.HttpClient(handler);
                var apiAddress = _configuration["ApiAdress"] ?? "https://apib2b.naifjewellery.com/";
                if (!apiAddress.EndsWith("/")) apiAddress += "/";
                client.BaseAddress = new System.Uri(apiAddress);

                var token = Request.Cookies["jwtToken"];
                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var encodedImageName = System.Net.WebUtility.UrlEncode(imageName);
                var deleteResponse = await client.DeleteAsync($"api/ProductImage?productId={productId}&imageName={encodedImageName}");
                if (!deleteResponse.IsSuccessStatusCode)
                {
                    using var formContent = new System.Net.Http.MultipartFormDataContent();
                    formContent.Add(new System.Net.Http.StringContent(productId.ToString()), "productId");
                    formContent.Add(new System.Net.Http.StringContent(imageName), "imageName");
                    deleteResponse = await client.PostAsync("api/ProductImage/delete", formContent);
                }

                if (!deleteResponse.IsSuccessStatusCode)
                {
                    var updateResponse = await RemoveProductImageThroughProductUpdate(productId, imageName);
                    if (updateResponse == null || !updateResponse.isSuccess)
                    {
                        var error = await deleteResponse.Content.ReadAsStringAsync();
                        return Json(updateResponse ?? new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { string.IsNullOrWhiteSpace(error) ? "Resim silinemedi." : error } });
                    }
                }

                TryDeleteLocalProductImage(imageName);
                return Json(new naif_katalog.Models.ResponseDto<bool> { isSuccess = true, data = true });
            }
            catch (System.Exception ex)
            {
                return Json(new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { ex.Message } });
            }
        }

        private async Task<naif_katalog.Models.ResponseDto<bool>?> RemoveProductImageThroughProductUpdate(int productId, string imageName)
        {
            var productsResponse = await _mediator.Send(new naif_katalog.Core.Features.ProductFeature.Queries.GetAllProductsQueryRequest());
            if (productsResponse == null || !productsResponse.isSuccess || productsResponse.data == null)
            {
                return new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = productsResponse?.errors ?? new List<string> { "Ürün bilgileri alınamadı." } };
            }

            var product = productsResponse.data.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return new naif_katalog.Models.ResponseDto<bool> { isSuccess = false, errors = new List<string> { "Ürün bulunamadı." } };
            }

            var normalizedImageName = NormalizeProductImageName(imageName);
            var remainingImages = (product.Images ?? new List<string>())
                .Select(NormalizeProductImageName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, normalizedImageName, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var updateRequest = new naif_katalog.Core.Features.ProductFeature.Commands.UpdateProductCommandRequest
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                Gram = product.Gram,
                Karat = product.MetalPurityName,
                DiamondCarat = product.DiamondCarat,
                CategoryIds = product.CategoryIds ?? new List<int>(),
                MetalPurityId = product.ProductMetals?.FirstOrDefault()?.MetalPurityId,
                ColorId = product.ColorId,
                LaborMultiplier = product.LaborMultiplier,
                PolishingCost = product.PolishingCost,
                LiveGoldPrice = product.LiveGoldPrice,
                ImageNames = remainingImages,
                ProductStones = (product.ProductStones ?? new List<naif_katalog.Core.Features.ProductFeature.Queries.ApiProductStone>())
                    .Select(s => new naif_katalog.Core.Features.ProductFeature.Commands.UpdateProductStoneDto
                    {
                        Id = s.Id,
                        StoneId = s.StoneId,
                        Quantity = Convert.ToInt32(s.Quantity),
                        Carat = s.Carat,
                        TotalCarat = s.TotalCarat,
                        ClarityId = s.ClarityId,
                        ColorId = s.ColorId
                    })
                    .ToList(),
                ProductMetals = (product.ProductMetals ?? new List<naif_katalog.Core.Features.ProductFeature.Queries.ApiProductMetal>())
                    .Select(m => new naif_katalog.Core.Features.ProductFeature.Commands.UpdateProductMetalDto
                    {
                        Id = m.Id,
                        MetalTypeId = m.MetalTypeId,
                        Weight = m.Gram
                    })
                    .ToList()
            };

            return await _mediator.Send(updateRequest);
        }

        private static string NormalizeProductImageName(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return string.Empty;
            }

            var normalized = imageName.Replace('\\', '/');
            const string marker = "/images/katalog/";
            var markerIndex = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex >= 0)
            {
                normalized = normalized[(markerIndex + marker.Length)..];
            }

            normalized = normalized.TrimStart('/');
            if (normalized.StartsWith("images/katalog/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized["images/katalog/".Length..];
            }

            return Uri.UnescapeDataString(normalized);
        }

        private void TryDeleteLocalProductImage(string imageName)
        {
            try
            {
                var relativePath = NormalizeProductImageName(imageName).Replace('/', System.IO.Path.DirectorySeparatorChar).Replace('\\', System.IO.Path.DirectorySeparatorChar);
                var fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "katalog", relativePath));
                var katalogRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "katalog"));

                if (fullPath.StartsWith(katalogRoot, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch
            {
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> CreateStoneScale([FromBody] naif_katalog.Core.Features.DefinitionFeature.Commands.CreateStoneScalesCommandRequest request)
        {
            var response = await _mediator.Send(request);
            return Json(new { isSuccess = response });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStoneScale([FromBody] naif_katalog.Core.Features.DefinitionFeature.Commands.UpdateStoneScalesCommandRequest request)
        {
            var response = await _mediator.Send(request);
            return Json(new { isSuccess = response });
        }

            
        [HttpPost]
        public async Task<IActionResult> UpdateCategoryOrder([FromBody] naif_katalog.Core.Features.CategoryFeature.Commands.UpdateOrder.UpdateCategoryOrderCommandRequest request)
        {
            var response = await _mediator.Send(request);
            return Json(response);
        }

        public async Task<IActionResult> CategoryAssign(string code = null)
        {
            var categories = await _mediator.Send(new GetAllCategoriesQueryRequest());
            ViewBag.Categories = categories?.data;
            ViewBag.ProductCode = code;
            return View();
        }
    }
}
