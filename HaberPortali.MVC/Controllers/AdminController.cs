using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HaberPortali.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _http;
        public AdminController(IHttpClientFactory http) => _http = http;

        private HttpClient GetAuthorizedClient()
        {
            var client = _http.CreateClient("API");
            var token = HttpContext.Session.GetString("token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index()
        {
            var client = GetAuthorizedClient();
            var news = await client.GetStringAsync("api/news");
            ViewBag.News = news;
            return View();
        }

        public async Task<IActionResult> NewsCreate()
        {
            var client = GetAuthorizedClient();
            var cats = await client.GetStringAsync("api/category");
            ViewBag.Categories = cats;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewsCreate(string title, string content, string imagePath, int categoryId)
        {
            var client = GetAuthorizedClient();

            // API'nin model doðrulamasýna takýlmamak için payload'u tam oluþturuyoruz (id = 0)
            var payload = new
            {
                id = 0,
                title = title,
                content = content,
                imagePath = imagePath,
                categoryId = categoryId
            };

            var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // API'ye isteði atýyoruz
            var response = await client.PostAsync("api/news", body);

            // Eðer iþlem GERÇEKTEN baþarýlýysa listeye dön
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            // EÐER BAÞARISIZSA: API'nin neden kaydetmediðini yakalayýp ekrana yansýtýyoruz
            var errorMessage = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Haber eklenemedi! API Hatasý: {response.StatusCode} - {errorMessage}";

            // Hatalý durumda tekrar ekleme formuna geri döndürüyoruz
            return RedirectToAction("NewsCreate");
        }

        public async Task<IActionResult> NewsEdit(int id)
        {
            var client = GetAuthorizedClient();
            var news = await client.GetStringAsync($"api/news/{id}");
            var cats = await client.GetStringAsync("api/category");
            ViewBag.News = news;
            ViewBag.Categories = cats;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewsEdit(int id, string title, string content, string imagePath, int categoryId)
        {
            var client = GetAuthorizedClient();
            var body = new StringContent(JsonSerializer.Serialize(new { id, title, content, imagePath, categoryId }), Encoding.UTF8, "application/json");
            await client.PutAsync("api/news", body);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> NewsDelete(int id)
        {
            var client = GetAuthorizedClient();
            await client.DeleteAsync($"api/news/{id}");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CategoryList()
        {
            var client = GetAuthorizedClient();
            var cats = await client.GetStringAsync("api/category");
            ViewBag.Categories = cats;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CategoryCreate(string name)
        {
            var client = GetAuthorizedClient();
            var body = new StringContent(JsonSerializer.Serialize(new { id = 0, name }), Encoding.UTF8, "application/json");
            await client.PostAsync("api/category", body);
            return RedirectToAction("CategoryList");
        }

        public async Task<IActionResult> CategoryDelete(int id)
        {
            var client = GetAuthorizedClient();
            await client.DeleteAsync($"api/category/{id}");
            return RedirectToAction("CategoryList");
        }
    }
}
