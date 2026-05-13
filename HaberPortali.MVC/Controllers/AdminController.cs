using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace HaberPortali.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Token'ý Session'dan al ve yetkilendirilmiþ HttpClient oluþtur
        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("API");
            var token = HttpContext.Session.GetString("token");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync("api/news");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var newsList = JsonSerializer.Deserialize<List<NewsDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ViewBag.News = newsList ?? new List<NewsDto>();
            }
            else
            {
                ViewBag.News = new List<NewsDto>();
                TempData["Error"] = "Haberler yüklenirken bir hata oluþtu.";
            }
            return View();
        }

        // GET: Admin/NewsCreate
        public async Task<IActionResult> NewsCreate()
        {
            await LoadCategoriesToViewBag();
            return View();
        }

        // POST: Admin/NewsCreate
        [HttpPost]
        public async Task<IActionResult> NewsCreate(NewsDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesToViewBag();
                return View(model);
            }

            var client = CreateAuthorizedClient();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/news", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Haber baþarýyla eklendi.";
                return RedirectToAction("Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Haber eklenirken hata oluþtu: {error}";
                await LoadCategoriesToViewBag();
                return View(model);
            }
        }

        // GET: Admin/NewsEdit/{id}
        public async Task<IActionResult> NewsEdit(int id)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"api/news/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Haber bulunamadý.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var news = JsonSerializer.Deserialize<NewsDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            await LoadCategoriesToViewBag();
            return View(news);
        }

        // POST: Admin/NewsEdit/{id}
        [HttpPost]
        public async Task<IActionResult> NewsEdit(int id, NewsDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesToViewBag();
                return View(model);
            }

            model.Id = id;
            var client = CreateAuthorizedClient();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/news", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Haber güncellendi.";
                return RedirectToAction("Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Güncelleme hatasý: {error}";
                await LoadCategoriesToViewBag();
                return View(model);
            }
        }

        // GET: Admin/NewsDelete/{id}
        public async Task<IActionResult> NewsDelete(int id)
        {
            var client = CreateAuthorizedClient();
            var response = await client.DeleteAsync($"api/news/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Haber silindi.";
            else
                TempData["Error"] = "Haber silinemedi.";
            return RedirectToAction("Index");
        }

        // GET: Admin/CategoryList
        public async Task<IActionResult> CategoryList()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync("api/category");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<CategoryDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ViewBag.Categories = categories ?? new List<CategoryDto>();
            }
            else
            {
                ViewBag.Categories = new List<CategoryDto>();
                TempData["Error"] = "Kategoriler yüklenemedi.";
            }
            return View();
        }

        // POST: Admin/CategoryCreate
        [HttpPost]
        public async Task<IActionResult> CategoryCreate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Kategori adý boþ olamaz.";
                return RedirectToAction("CategoryList");
            }

            var client = CreateAuthorizedClient();
            var categoryDto = new { Name = name };
            var content = new StringContent(JsonSerializer.Serialize(categoryDto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/category", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Kategori eklendi.";
            else
                TempData["Error"] = "Kategori eklenemedi.";
            return RedirectToAction("CategoryList");
        }

        // GET: Admin/CategoryDelete/{id}
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var client = CreateAuthorizedClient();
            var response = await client.DeleteAsync($"api/category/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Kategori silindi.";
            else
                TempData["Error"] = "Kategori silinemedi. (Ýliþkili haberler olabilir)";
            return RedirectToAction("CategoryList");
        }

        private async Task LoadCategoriesToViewBag()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync("api/category");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<CategoryDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ViewBag.Categories = categories ?? new List<CategoryDto>();
            }
            else
            {
                ViewBag.Categories = new List<CategoryDto>();
            }
        }
    }

    public class NewsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ViewCount { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}