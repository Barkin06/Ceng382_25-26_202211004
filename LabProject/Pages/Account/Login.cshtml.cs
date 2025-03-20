using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string? Username { get; set; }

        [BindProperty]
        public string? PhoneNumber { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? ConfirmPassword { get; set; }

        [TempData]
        public string? Message { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(PhoneNumber) 
                || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) 
                || string.IsNullOrEmpty(ConfirmPassword))
            {
                ViewData["Message"] = "Please fill all the informations.";
                return Page();
            }

            if (!Email.Contains("@"))
            {
                ViewData["Message"] = "Email address is wrong.";
                return Page();
            }

           
            if (Password != ConfirmPassword)
            {
                ViewData["Message"] = "Passwords are not matching.";
                return Page();
            }

            
            TempData["Message"] = "Registration is successful!";
            return RedirectToPage("/Success");
        }
    }
}
