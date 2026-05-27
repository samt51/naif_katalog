using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace naif_katalog.Controllers
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var request = new { Email = email, Password = password };
            var response = await _apiService.PostAsync<object, string>("api/auth/login", request);

            if (response != null && response.isSuccess && !string.IsNullOrEmpty(response.data))
            {
                var token = response.data;
                
                // Store JWT token in cookie or local storage, usually inside a secure HttpOnly cookie
                Response.Cookies.Append("jwtToken", token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                });

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "Cookies");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("Cookies", claimsPrincipal);

                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
                if (roleClaim == "3")
                {
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Dashboard", "Admin");
            }

            ViewBag.Error = response?.errors != null ? string.Join(", ", response.errors) : "Giriş başarısız.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            Response.Cookies.Delete("jwtToken");
            return RedirectToAction("Login");
        }
    }
}
