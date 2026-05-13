using Microsoft.AspNetCore.Mvc;

namespace HaberPortali.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _http;
        public HomeController(IHttpClientFactory http) => _http = http;

        public async Task<IActionResult> Index(string? search = null)
        {
            var client = _http.CreateClient("API");
            var url = string.IsNullOrEmpty(search) ? "api/news" : $"api/news?search={search}";
            var res = await client.GetStringAsync(url);
            ViewBag.News = res;
            ViewBag.Search = search;
            return View();
        }

        public async Task<IActionResult> Detail(int id)
        {
            var client = _http.CreateClient("API");
            var news = await client.GetStringAsync($"api/news/{id}");
            var comments = await client.GetStringAsync($"api/comment/news/{id}");
            ViewBag.News = news;
            ViewBag.Comments = comments;
            ViewBag.NewsId = id;
            return View();
        }

        public async Task<IActionResult> Category(int id)
        {
            var client = _http.CreateClient("API");
            var res = await client.GetStringAsync($"api/news/category/{id}");
            ViewBag.News = res;
            return View();
        }
    }
}
