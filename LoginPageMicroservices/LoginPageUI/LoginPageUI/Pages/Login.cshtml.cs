
using System.Net;
using System.Net.Http.Json;
using LoginPageUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPageUI.Pages;

public class LoginModel : PageModel {
    private readonly IHttpClientFactory _clientFactory;
    public LoginModel(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    // For Bootstrap alerts
    public string? AlertMessage { get; set; }
    public string AlertType { get; set; } = "warning"; // success | warning | danger | info

    public void OnGet() {
        // Optionally clear any previous token
        // HttpContext.Session.Remove("AccessToken");
    }

    public async Task<IActionResult> OnPostAsync() {
        // Server-side validation
        if (!ModelState.IsValid) {
            AlertType = "warning";
            AlertMessage = "Please provide valid username and password.";
            return Page();
        }

        try {
            var client = _clientFactory.CreateClient("LoginApi");

            var response = await client.PostAsJsonAsync("/api/auth/login", new {
                username = Input.Username,
                password = Input.Password
            });

            if (!response.IsSuccessStatusCode) {
                // Try to extract API's message if present
                string? apiMessage = null;
                try {
                    var obj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    if (obj != null && obj.TryGetValue("message", out var msg)) apiMessage = msg;
                } catch { /* ignore deserialization errors */ }

                AlertType = "danger";

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    AlertMessage = apiMessage ?? "Invalid username or password.";
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                    AlertMessage = apiMessage ?? "Invalid input. Please check your entries.";
                else
                    AlertMessage = apiMessage ?? $"Login failed (HTTP {(int)response.StatusCode}).";

                ModelState.AddModelError(string.Empty, AlertMessage);
                return Page();
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null || string.IsNullOrWhiteSpace(auth.Token)) {
                AlertType = "danger";
                AlertMessage = "Unexpected response from server.";
                ModelState.AddModelError(string.Empty, AlertMessage);
                return Page();
            }

            // Store token for subsequent calls
            HttpContext.Session.SetString("AccessToken", auth.Token);

            TempData["Success"] = "Login successful.";
            return RedirectToPage("/Secure");
        } catch (HttpRequestException) {
            AlertType = "danger";
            AlertMessage = "Cannot reach server. Check API URL, port, or HTTPS certificate.";
            ModelState.AddModelError(string.Empty, AlertMessage);
            return Page();
        } catch (Exception) {
            AlertType = "danger";
            AlertMessage = "Unexpected error occurred.";
            ModelState.AddModelError(string.Empty, AlertMessage);
            return Page();
        }
    }

    // local DTO for deserialization
    public class AuthResponse {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
