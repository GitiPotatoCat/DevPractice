
using System.Net;
using System.Net.Http.Json;
using LoginPageUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPageUI.Pages {
    public class RegisterModel : PageModel {
        private readonly IHttpClientFactory _clientFactory;
        public RegisterModel(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

        [BindProperty]
        public RegisterInput Input { get; set; } = new();

        // For Bootstrap alerts
        public string? AlertMessage { get; set; }
        public string AlertType { get; set; } = "warning"; // success | warning | danger | info

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                AlertType = "warning";
                AlertMessage = "Please fix the validation errors.";
                return Page();
            }

            try {
                var client = _clientFactory.CreateClient("LoginApi");

                // Body shape must match API expectations:
                // { "username": "...", "password": "..." }
                var response = await client.PostAsJsonAsync("/api/auth/register", new {
                    username = Input.Username,
                    password = Input.Password
                });

                if (!response.IsSuccessStatusCode) {
                    // Attempt to read API's message if present: { "message": "..." }
                    string? apiMessage = null;
                    try {
                        var obj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        if (obj != null && obj.TryGetValue("message", out var msg))
                            apiMessage = msg;
                    } catch { /* ignore deserialization errors */ }

                    AlertType = "danger";

                    if (response.StatusCode == HttpStatusCode.Conflict)
                        AlertMessage = apiMessage ?? "Username already exists.";
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                        AlertMessage = apiMessage ?? "Invalid input.";
                    else
                        AlertMessage = apiMessage ?? $"Registration failed (HTTP {(int)response.StatusCode}).";

                    ModelState.AddModelError(string.Empty, AlertMessage);
                    return Page();
                }

                // Success (201 Created or 200 OK depending on API)
                TempData["Success"] = "Registration successful. Please login.";
                return RedirectToPage("/Login");
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
    }
}
