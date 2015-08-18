using Simsip.LineRunner.Services.Inapp;
using System.Collections.Generic;
using System.Linq;


namespace Simsip.LineRunner.Utils
{
    public static class InAppUtils
    {
        private static IInappService _inAppService;

        private static InAppUtils()
        {
            InAppUtils._inAppService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService)); 
        }

        public static string GetPackProductIdSuffix(string productId)
        {
            return productId.Replace(InAppUtils._inAppService.LinerunnerPackPrefix, "");
        }

        public static string GetPackProductId(string productIdSuffix)
        {
            return InAppUtils._inAppService.LinerunnerPackPrefix + productIdSuffix;
        }

    }
}