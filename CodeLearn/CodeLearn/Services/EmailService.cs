using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Polly;
using Polly.Retry;

namespace CodeLearn.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration          _config;
        private readonly ILogger<EmailService>   _logger;

        private static readonly ResiliencePipeline _retry = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2, Delay = TimeSpan.FromSeconds(2) })
            .Build();

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public Task<bool> SendVerificationCodeAsync(string toEmail, string code) =>
            SendAsync(toEmail, "CodeLearn: код подтверждения", $"Ваш код: {code}", false);

        public Task<bool> SendPasswordResetAsync(string toEmail, string resetLink) =>
            SendAsync(toEmail, "CodeLearn: сброс пароля",
                "<p>Вы запросили сброс пароля в CodeLearn.</p>" +
                $"<p><a href=\"{resetLink}\">Нажмите здесь, чтобы задать новый пароль</a></p>" +
                "<p>Ссылка действует ограниченное время. Если вы не запрашивали сброс — проигнорируйте письмо.</p>",
                true);

        private async Task<bool> SendAsync(string toEmail, string subject, string body, bool isHtml)
        {
            var host = _config["Email:SmtpHost"];
            var user = _config["Email:User"];
            var pass = _config["Email:AppPassword"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                _logger.LogWarning("SMTP не настроен — письмо «{Subject}» для {Email} не отправлено", subject, toEmail);
                return false;
            }

            int.TryParse(_config["Email:SmtpPort"], out var port);
            if (port == 0) port = 587;
            bool.TryParse(_config["Email:UseSsl"], out var useSsl);

            var password  = pass.Replace(" ", "").Trim();
            var fromValue = _config["Email:From"] ?? user;

            try
            {
                await _retry.ExecuteAsync(async ct =>
                {
                    using var client = new SmtpClient(host, port)
                    {
                        EnableSsl   = useSsl,
                        Credentials = new NetworkCredential(user, password)
                    };
                    using var message = new MailMessage
                    {
                        From       = new MailAddress(fromValue),
                        Subject    = subject,
                        Body       = body,
                        IsBodyHtml = isHtml
                    };
                    message.To.Add(toEmail);
                    await client.SendMailAsync(message, ct);
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки письма «{Subject}» на {Email}", subject, toEmail);
                return false;
            }
        }
    }
}
