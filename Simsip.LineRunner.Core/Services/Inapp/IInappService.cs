using System.Collections.Generic;

#if ANDROID
using Android.App;
using Android.Content;
using Simsip.LineRunner.Entities.InApp;
using System.Threading.Tasks;
#elif IOS
using StoreKit;
#endif


namespace Simsip.LineRunner.Services.Inapp
{
#if ANDROID
    public delegate void BuyProductErrorDelegate(int responseCode, string sku);

    public delegate void InAppBillingProcessingErrorDelegate(string message);

    public delegate void OnGetProductsErrorDelegate(int responseCode, IDictionary<string, object> ownedItems);

    public delegate void OnInvalidOwnedItemsBundleReturnedDelegate(IDictionary<string, object> ownedItems);

    public delegate void OnProductPurchasedDelegate(int response, InAppPurchaseEntity purchase, string purchaseData, string purchaseSignature);

    public delegate void OnProductPurchaseErrorDelegate(int responseCode, string sku);

    public delegate void OnPurchaseConsumedDelegate(string token);

    public delegate void OnPurchaseConsumedErrorDelegate(int responseCode, string token);

    public delegate void OnPurchaseFailedValidationDelegate(InAppPurchaseEntity purchase, string purchaseData, string purchaseSignature);

    public delegate void OnUserCanceledDelegate();

    public delegate void QueryInventoryErrorDelegate(int responseCode, IDictionary<string, object> skuDetails);
#endif

    public interface IInappService
    {

#if ANDROID

        void StartSetup();

        void OnDestroy();

        void HandleActivityResult(int requestCode, Result resultCode, Intent data);

        /// <summary>
        /// Buys the given Xamarin.Android.InAppBilling.Product
        /// 
        /// This method automatically generates a unique GUID and attaches it as the
        /// developer payload for this purchase.
        /// </summary>
        /// <param name="product">The Xamarin.Android.InAppBilling.Product representing the item the users
        /// wants to purchase.</param>
        void BuyProduct(InAppSkuEntity product);

        /// <summary>
        /// Buys the given Xamarin.Android.InAppBilling.Product and attaches the given
        /// developer payload to the purchase.
        /// </summary>
        /// <param name="product">The Xamarin.Android.InAppBilling.Product representing the item the users
        /// wants to purchase.</param>
        /// <param name="payload">The developer payload to attach to the purchase.</param>
        void BuyProduct(InAppSkuEntity product, string payload);
        
        /// <summary>
        /// Buys a product based on the given product SKU and Item Type attaching the
        /// given payload
        /// </summary>
        /// <param name="sku">The SKU of the item to purchase.</param>
        /// <param name="itemType">The type of the item to purchase.</param>
        /// <param name="payload">The developer payload to attach to the purchase.</param>
        void BuyProduct(string sku, string itemType, string payload);
        
        /// <summary>
        /// Consumes the purchased item.
        /// </summary>
        /// <param name="purchase">The purchase receipt of the item to consume.</param>
        /// <returns>true if the purchase is successfully consumed else returns false.</returns>
        bool ConsumePurchase(InAppPurchaseEntity purchase);
        
        /// <summary>
        /// Consumes the purchased item
        /// </summary>
        /// <param name="token">The purchase token of the purchase to consume.</param>
        /// <returns>true if the purchase is successfully consumed else returns false.</returns>
        bool ConsumePurchase(string token);
        
        /// <summary>
        /// Gets a list of all products of a given item type purchased by the current
        /// user.
        /// </summary>
        /// <param name="itemType">Item type (product or subs)</param>
        /// <returns>A list of Xamarin.InAppBilling.Products purchased by the current user.</returns>
        IList<InAppPurchaseEntity> GetPurchases(string itemType);
        
        /// <summary>
        /// Queries the inventory asynchronously and returns a list of Xamarin.Android.InAppBilling.Products
        /// matching the given list of SKU numbers.
        /// </summary>
        /// <param name="skuList">Sku list.</param>
        /// <param name="itemType">The Xamarin.Android.InAppBilling.ItemType of product being queried.</param>
        /// <returns>List of Xamarin.Android.InAppBilling.Products matching the given list of SKUs.
        /// </returns>
        Task<IList<InAppSkuEntity>> QueryInventoryAsync(IList<string> skuList, string itemType);

        /// <summary>
        /// Occurs when the user attempts to buy a product and there is an error.
        /// </summary>
        event BuyProductErrorDelegate BuyProductError;
        
        /// <summary>
        /// Occurs when there is an in app billing procesing error.
        /// </summary>
        event InAppBillingProcessingErrorDelegate InAppBillingProcesingError;
        
        /// <summary>
        /// Raised where there is an error getting previously purchased products from
        /// the Google Play Services.
        /// </summary>
        event OnGetProductsErrorDelegate OnGetProductsError;
        
        /// <summary>
        /// Raised when Google Play Services returns an invalid bundle from previously
        /// purchased items
        /// </summary>
        event OnInvalidOwnedItemsBundleReturnedDelegate OnInvalidOwnedItemsBundleReturned;
        
        /// <summary>
        /// Occurs after a product has been successfully purchased Google Play.
        /// 
        /// This event is fired after a OnProductPurchased which is raised when the user
        /// successfully logs an intent to purchase with Google Play.
        /// </summary>
        event OnProductPurchasedDelegate OnProductPurchased;

        /// <summary>
        /// Occurs when the is an error on a product purchase attempt.
        /// </summary>
        event OnProductPurchaseErrorDelegate OnProductPurchasedError;
        
        /// <summary>
        /// Occurs when on product consumed.
        /// </summary>
        event OnPurchaseConsumedDelegate OnPurchaseConsumed;

        /// <summary>
        /// Occurs when there is an error consuming a product.
        /// </summary>
        event OnPurchaseConsumedErrorDelegate OnPurchaseConsumedError;
        
        /// <summary>
        /// Occurs when a previously purchased product fails to validate.
        /// </summary>
        event OnPurchaseFailedValidationDelegate OnPurchaseFailedValidation;

        /// <summary>
        /// Occurs when on user canceled.
        /// </summary>
        event OnUserCanceledDelegate OnUserCanceled;
        
        /// <summary>
        /// Occurs when there is an error querying inventory from Google Play Services.
        /// </summary>
        event QueryInventoryErrorDelegate QueryInventoryError;

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