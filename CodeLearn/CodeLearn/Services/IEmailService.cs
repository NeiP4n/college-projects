namespace CodeLearn.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationCodeAsync(string toEmail, string code);
        Task<bool> SendPasswordResetAsync(string toEmail, string resetLink);
    }
}
