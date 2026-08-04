namespace MotorsShop.Services.Email;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;
    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) => _logger = logger;

    public Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        _logger.LogInformation("""

            ====================== EMAIL (dev) ======================
            To:      {ToEmail}
            Subject: {Subject}
            ---------------------------------------------------------
            {Body}
            =========================================================

            """, toEmail, subject, htmlBody);

        return Task.CompletedTask;
    }
}