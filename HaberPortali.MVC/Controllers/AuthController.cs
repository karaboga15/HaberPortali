using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HaberPortali.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _http;
        public AuthController(IHttpClientFactory http) => _http = http;

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var client = _http.CreateClient("API");
            var body = new StringContent(JsonSerializer.Serialize(new { userName, password }), Encoding.UTF8, "application/json");
            var res = await client.PostAsync("api/auth/login", body);
            if (!res.IsSuccessStatusCode) { ViewBag.Error = "Kullanıcı adı veya şifre hatalı."; return View(); }
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            var token = data.GetProperty("data").GetString();
            HttpContext.Session.SetString("token", token!);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string userName, string password)
        {
            var client = _http.CreateClient("API");
            var body = new StringContent(JsonSerializer.Serialize(new { userName, password }), Encoding.UTF8, "application/json");
            var res = await client.PostAsync("api/auth/register", body);
            if (!res.IsSuccessStatusCode) { ViewBag.Error = "Kayıt başarısız."; return View(); }
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
