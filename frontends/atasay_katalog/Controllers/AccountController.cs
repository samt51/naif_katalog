using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using atasay_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace atasay_katalog.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Welcome", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var request = new { Email = email, Password = password };
            var response = await _apiService.PostAsync<object, string>("api/auth/login", request);

            if (response != null && response.isSuccess && !string.IsNullOrEmpty(response.data))
            {
                var token = response.data;
                
                // Store JWT token in cookie or local storage, usually inside a secure HttpOnly cookie
                Response.Cookies.Append("Atasay.ApiToken", token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                });

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims.Select(claim =>
                    claim.Type is "role" or "roles"
                        ? new Claim(ClaimTypes.Role, claim.Value)
                        : claim).ToList();
                if (!claims.Any(c => c.Type == ClaimTypes.Role))
                {
                    var rawRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                    if (!string.IsNullOrWhiteSpace(rawRole))
                        claims.Add(new Claim(ClaimTypes.Role, rawRole));
                }
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies", ClaimTypes.Name, ClaimTypes.Role);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("Cookies", claimsPrincipal);
                
                try {
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid" || c.Type == "sub" || c.Type == "id" || c.Type == "userId")?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int uid))
                    {
                        var _mediator = HttpContext.RequestServices.GetService(typeof(MediatR.IMediator)) as MediatR.IMediator;
                        if (_mediator != null) {
                            await _mediator.Send(new atasay_katalog.Core.Features.UserActionLogFeature.Commands.Create.CreateUserActionLogCommandRequest
                            {
                                UserId = uid,
                                ActionType = "Login",
                                ProductId = null,
                                Details = "Sisteme giriş yapıldı.",
                                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                            });
                        }
                    }
                } catch {}

                return RedirectToAction("Welcome", "Home");
            }

            ViewBag.Error = response?.errors != null ? string.Join(", ", response.errors) : "Giriş başarısız.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            Response.Cookies.Delete("Atasay.ApiToken");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction(nameof(Login));
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            ViewBag.HomeUrl = "/Home/Welcome";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCurrentPassword([FromBody] VerifyPasswordRequest request)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request?.Password))
                return BadRequest(new { isSuccess = false, message = "Şifre gereklidir." });
            var response = await _apiService.PostAsync<object, string>("api/auth/login", new { Email = email, request.Password });
            return Json(new { isSuccess = response?.isSuccess == true, message = response?.isSuccess == true ? "OK" : "Şifre hatalı." });
        }
    }
}

public sealed class VerifyPasswordRequest { public string Password { get; set; } = string.Empty; }
