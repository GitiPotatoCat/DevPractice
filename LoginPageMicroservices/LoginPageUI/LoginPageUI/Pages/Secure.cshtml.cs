
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPageUI.Pages;

public class SecureModel : PageModel {
    public string? Token { get; set; }
    public IActionResult OnGet() {
        Token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrEmpty(Token)) {
            TempData["Success"] = null;
            TempData["Alert"] = "Please login first.";
            return RedirectToPage("/Login");
        }
        return Page();
    }
}