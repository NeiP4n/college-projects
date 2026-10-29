namespace CodeLearn.Services
{
    public record PendingLocalData(string Email, string Password);
    public record PendingGoogleData(string GoogleId, string Email, string AvatarUrl);

    public enum CodeValidationResult { Valid, Invalid, Expired, Missing }

    public interface IRegistrationFlowService
    {
        string               GenerateCode();
        void                 StorePendingLocal(string email, string password, string code);
        void                 StorePendingGoogle(string googleId, string email, string avatarUrl);
        void                 MarkEmailVerified();
        CodeValidationResult ValidateCode(string inputCode);
        string               MaskEmail(string email);
        PendingLocalData?    GetPendingLocal();
        PendingGoogleData?   GetPendingGoogle();
        bool                 IsEmailVerified();
        void                 ClearPendingLocal();
        void                 ClearPendingGoogle();
    }
}
