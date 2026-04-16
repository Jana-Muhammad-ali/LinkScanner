using checker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace checker.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Check(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                ViewBag.Result = "Please enter a link";
                return View("Index");
            }

            // لو المستخدم كتب بدون https
            if (!url.StartsWith("http"))
            {
                url = "https://" + url;
            }

            try
            {
                var uri = new Uri(url);
                string domain = uri.Host.ToLower();

                // 🔥 Step 1: كشف الأرقام (زي go0gle)
                if (domain.Contains("0") || domain.Contains("1"))
                {
                    ViewBag.Result = "⚠️ Suspicious link (numbers inside domain)";
                    return View("Index");
                }

                // 🔥 Step 2: مقارنة بمواقع مشهورة
                string[] popularSites = { "google.com", "facebook.com", "youtube.com" };

                foreach (var site in popularSites)
                {
                    int distance = LevenshteinDistance(domain, site);

                    if (distance <= 2) // قريب جدًا = خطر
                    {
                        ViewBag.Result = $"⚠️ Suspicious! Did you mean {site}?";
                        return View("Index");
                    }
                }

                // 🔥 Step 3: check الموقع بيرد ولا لا
                var request = WebRequest.Create(url);
                request.Method = "HEAD";

                using (var response = await request.GetResponseAsync())
                {
                    ViewBag.Result = "✅ Valid and reachable website";
                }
            }
            catch
            {
                ViewBag.Result = "❌ Invalid link or website not found";
            }

            return View("Index");
        }
        private int LevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= t.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[s.Length, t.Length];
        }
    }
}