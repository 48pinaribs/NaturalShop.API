namespace NaturalShop.API.Services
{
    public interface ISmsService
    {
        Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
    }
}

