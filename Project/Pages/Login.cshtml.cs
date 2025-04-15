using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Week2.Models;

namespace Week2.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty] public string Username { get; set; }
        [BindProperty] public string Password { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            var sessionUsername = HttpContext.Session.GetString("username");
            var cookieUsername = Request.Cookies["username"];

            if (!string.IsNullOrEmpty(sessionUsername) && sessionUsername == cookieUsername)
            {
                Response.Redirect("/Index");
            }
        }

        public IActionResult OnPost()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "users.json");
            if (!System.IO.File.Exists(filePath))
            {
                ErrorMessage = "users.json not found!";
                return Page();
            }

            var json = System.IO.File.ReadAllText(filePath);
            var users = JsonSerializer.Deserialize<List<User>>(json);

            var foundUser = users?.FirstOrDefault(u =>
                u.Username == Username &&
                u.Password == Password &&
                u.IsActive);

            if (foundUser == null)
            {
                ErrorMessage = "Hatalı kullanıcı adı veya şifre!";
                return Page();
            }

            var token = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("username", Username);
            HttpContext.Session.SetString("token", token);
            HttpContext.Session.SetString("session_id", HttpContext.Session.Id);

            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(30),
                HttpOnly = true,
                Secure = false, // localhost'ta false, yayında true
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("username", Username, options);
            Response.Cookies.Append("token", token, options);
            Response.Cookies.Append("session_id", HttpContext.Session.Id, options);

            return RedirectToPage("/Index");
        }
    }
}
