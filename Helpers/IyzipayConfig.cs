using Iyzipay;
using Microsoft.Extensions.Configuration;

namespace NaturalShop.API.Helpers
{
    public static class IyzipayConfig
    {
        public static Options GetOptions(IConfiguration configuration)
        {
            return new Options
            {
                ApiKey = configuration["Iyzipay:ApiKey"],
                SecretKey = configuration["Iyzipay:SecretKey"],
                BaseUrl = configuration["Iyzipay:BaseUrl"]
            };
        }
    }
}
