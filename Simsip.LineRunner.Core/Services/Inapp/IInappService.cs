using System.Collections.Generic;

#if ANDROID
using Android.App;
using Android.Content;
#elif IOS
using StoreKit;
#endif


namespace Simsip.LineRunner.Services.Inapp
{
    public interface IInappService
    {

#if ANDROID

        void StartSetup();

        void OnDestroy();

        void HandleActivityResult(int requestCode, Result resultCode, Intent data);
#elif IOS

        bool CanMakePayments();

        void RequestProductData(List<string> productIds);

        void PurchaseProduct(string appStoreProductId);

        void CompleteTransaction(SKPaymentTransaction transaction);

        void RestoreTransaction(SKPaymentTransaction transaction);

        void FailedTransaction(SKPaymentTransaction transaction);

        void FinishTransaction(SKPaymentTransaction transaction, bool wasSuccessful);

        void Restore();

#elif DESKTOP
#elif WINDOWS_PHONE
#elif NETFX_CORE
#endif

    }
}