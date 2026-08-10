namespace NaturalShop.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationCodeAsync(string email, string code, int expiresInMinutes);
    }
}
