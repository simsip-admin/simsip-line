using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;
#elif IOS
using StoreKit;
using Foundation;
using MonoTouch;
#endif


namespace Simsip.LineRunner.Services.Inapp
{
#if ANDROID

    public class InappService : GameComponent, IInappService
    {
        private InAppBillingServiceConnection _serviceConnection;
        private Product _selectedProduct;
        private IList<Product> _products;

        public InappService(Game game)
            : base(game)
        {
            // Register as service
            game.Services.AddService(typeof(IInappService), this);
        }

        #region GameComponent Overrides

        /// <summary>
        /// Initializes the in app service.
        /// </summary>
        public override void Initialize()
        {
            // Initialize the list of available items
            this._products = new List<Product>();
        }

        #endregion

        #region IInappService implementation

        /// <summary>
        /// Starts the setup of this Android application by connection to the Google Play Service
        /// to handle In-App purchases.
        /// </summary>
        public void StartSetup()
        {
            // A Licensing and In-App Billing public key is required before an app can communicate with
            // Google Play, however you DON'T want to store the key in plain text with the application.
            // The Unify command provides a simply way to obfuscate the key by breaking it into two or
            // or more parts, specifying the order to reassemlbe those parts and optionally providing
            // a set of key/value pairs to replace in the final string. 
            string value = Security.Unify(
                new string[] { "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAtpopeFYhDOOsufNhYe2PY97azBQBJoqGSP/XxsgzQgj3M0MQQ0WE0WDwPKoxlo/MIuoadVR5q2ZRs3rTl", 
					"UsH9vYUPr/z0/O8kn5anQHshQkHkeHsA8wbyJGGmwcXqvZr1fnzFOFyHL46s47vOJBLc5x30oieJ02dNxdBcy0oFEJtJS5Ng5sm6YUv9ifgxYOqMr/K61HNw6tP0j", 
					"vi6vLv2ro/KXO0ADVxcDxEg+Pk16xVNKgnei8M09PJCgA9bWNyeSsD+85Jj9+OZGsS/DN7O7nrGuKx8oAg/lE6a2eCQHh9lXxSPlAFAMH2FB8aNxUeJxkByW+6l/S", 
					"yqvVlOeYxAwIDAQAB" },
                new int[] { 0, 1, 2, 3 });

            // Create a new connection to the Google Play Service
            _serviceConnection = new InAppBillingServiceConnection(Program.SharedProgram, value);
            _serviceConnection.OnConnected += () =>
            {
                // Attach to the various error handlers to report issues
                _serviceConnection.BillingHandler.OnGetProductsError += (int responseCode, Bundle ownedItems) =>
                {
                    System.Diagnostics.Debug.WriteLine("Error getting products");
                };

                _serviceConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += (Bundle ownedItems) =>
                {
                    System.Diagnostics.Debug.WriteLine("Invalid owned items bundle returned");
                };

                _serviceConnection.BillingHandler.OnProductPurchasedError += (int responseCode, string sku) =>
                {
                    System.Diagnostics.Debug.WriteLine("Error purchasing item {0}", sku);
                };

                _serviceConnection.BillingHandler.OnPurchaseConsumedError += (int responseCode, string token) =>
                {
                    System.Diagnostics.Debug.WriteLine("Error consuming previous purchase");
                };

                _serviceConnection.BillingHandler.InAppBillingProcesingError += (message) =>
                {
                    System.Diagnostics.Debug.WriteLine("In app billing processing error {0}", message);
                };

                // Load inventory or available products
                GetInventory();

                // Load any items already purchased
                LoadPurchasedItems();
            };

            // Attempt to connect to the service
            _serviceConnection.Connect();

        }

        /// <summary>
        /// Perform any final cleanup before an activity is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            // Are we attached to the Google Play Service?
            if (this._serviceConnection != null)
            {
                // Yes, disconnect
                this._serviceConnection.Disconnect();
            }
        }

        public void HandleActivityResult(int requestCode, Result resultCode, Intent data)
        {
            this._serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);

            //TODO: Use a call back to update the purchased items
            UpdatePurchasedItems();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Loads the purchased items.
        /// </summary>
        private void LoadPurchasedItems()
        {
            // Ask the open connection's billing handler to get any purchases
            var purchases = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);
        }

        /// <summary>
        /// Updates the purchased items.
        /// </summary>
        private void UpdatePurchasedItems()
        {
            // Ask the open connection's billing handler to get any purchases
            var purchases = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);
        }

        /// <summary>
        /// Connects to the Google Play Service and gets a list of products that are available
        /// for purchase.
        /// </summary>
        /// <returns>The inventory.</returns>
        private async Task GetInventory()
        {
            // Ask the open connection's billing handler to return a list of avilable products for the 
            // given list of items.
            // NOTE: We are asking for the Reserved Test Product IDs that allow you to test In-App
            // Billing without actually making a purchase.
            _products = await _serviceConnection.BillingHandler.QueryInventoryAsync(new List<string> {
				"appra_01_test",
				"appra_02_sub",
				ReservedTestProductIDs.Purchased
			}, ItemType.Product);

            // Were any products returned?
            if (_products == null)
            {
                // No, abort
                return;
            }

            // Populate list of available products
            var items = _products.Select(p => p.Title).ToList();
        }

        #endregion
    }

#elif IOS

    public class InappService : SKProductsRequestDelegate, IInappService
    {
        public static NSString InAppPurchaseManagerProductsFetchedNotification = new NSString("InAppPurchaseManagerProductsFetchedNotification");
		public static NSString InAppPurchaseManagerTransactionFailedNotification = new NSString("InAppPurchaseManagerTransactionFailedNotification");
		public static NSString InAppPurchaseManagerTransactionSucceededNotification = new NSString("InAppPurchaseManagerTransactionSucceededNotification");
		public static NSString InAppPurchaseManagerRequestFailedNotification = new NSString("InAppPurchaseManagerRequestFailedNotification");

		SKProductsRequest productsRequest;
		CustomPaymentObserver theObserver;

        public InappService(Game game)
        {
            // Register as service
            game.Services.AddService(typeof(IInappService), this);  
        }

    #region Properties

        // Faking a GameComponent property for IOS
        public bool Enabled { get; set; }

        // TODO
        // public static NSAction Done { get; set; }

        #endregion

    #region GameComponent Overrides

        /// <summary>
        /// Initializes the in app service.
        /// </summary>
        public void Initialize()
        {
            theObserver = new CustomPaymentObserver(this);
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(theObserver);
        }

         #endregion

    #region SKProductsRequestDelegate overrides

        // received response to RequestProductData - with price,title,description info
		public override void ReceivedResponse (SKProductsRequest request, SKProductsResponse response)
		{
			SKProduct[] products = response.Products;

			NSDictionary userInfo = null;
			if (products.Length > 0) {
				NSObject[] productIdsArray = new NSObject[response.Products.Length];
				NSObject[] productsArray = new NSObject[response.Products.Length];
				for (int i = 0; i < response.Products.Length; i++) {
					productIdsArray[i] = new NSString(response.Products[i].ProductIdentifier);
					productsArray[i] = response.Products[i];
				}
				userInfo = NSDictionary.FromObjectsAndKeys (productsArray, productIdsArray);
			}
			NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerProductsFetchedNotification,this,userInfo);

			foreach (string invalidProductId in response.InvalidProducts) {
				Debug.WriteLine("Invalid product id: " + invalidProductId );
			}
		}

        /// <summary>
        /// Probably could not connect to the App Store (network unavailable?)
        /// </summary>
        public override void RequestFailed(SKRequest request, NSError error)
        {
            Debug.WriteLine(" ** InAppPurchaseManager RequestFailed() " + error.LocalizedDescription);
            using (var pool = new NSAutoreleasePool())
            {
                NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { error }, new NSObject[] { new NSString("error") });
                // send out a notification for the failed transaction
                NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerRequestFailedNotification, this, userInfo);
            }
        }

        #endregion

    #region IInappService implementation

        // Verify that the iTunes account can make this purchase for this application
        public bool CanMakePayments()
        {
            return SKPaymentQueue.CanMakePayments;
        }

        // request multiple products at once
        public void RequestProductData(List<string> productIds)
        {
            var array = new NSString[productIds.Count];
            for (var i = 0; i < productIds.Count; i++)
            {
                array[i] = new NSString(productIds[i]);
            }
            NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array);

            //set up product request for in-app purchase
            productsRequest = new SKProductsRequest(productIdentifiers);
            productsRequest.Delegate = this; // SKProductsRequestDelegate.ReceivedResponse
            productsRequest.Start();
        }

        public void PurchaseProduct(string appStoreProductId)
        {
            Debug.WriteLine("PurchaseProduct " + appStoreProductId);
            SKPayment payment = SKPayment.PaymentWithProduct(appStoreProductId);
            SKPaymentQueue.DefaultQueue.AddPayment(payment);
        }

        public void CompleteTransaction(SKPaymentTransaction transaction)
        {
            Debug.WriteLine("CompleteTransaction " + transaction.TransactionIdentifier);
            var productId = transaction.Payment.ProductIdentifier;
            // Register the purchase, so it is remembered for next time
            // PhotoFilterManager.Purchase(productId);
            FinishTransaction(transaction, true);
            /* Was commented out in sample as well
                        if (ReceiptValidation.VerificationController.SharedInstance.VerifyPurchase (transaction)) {
                            Console.WriteLine ("Verified!");
                            // Register the purchase, so it is remembered for next time
                            PhotoFilterManager.Purchase(productId);
                            FinishTransaction (transaction, true);
                        } else {
                            Console.WriteLine ("NOT Verified :(");
                            FinishTransaction (transaction, false);
                        }
            */
        }

        public void RestoreTransaction(SKPaymentTransaction transaction)
        {
            // Restored Transactions always have an 'original transaction' attached
            Debug.WriteLine("RestoreTransaction " + transaction.TransactionIdentifier + "; OriginalTransaction " + transaction.OriginalTransaction.TransactionIdentifier);
            var productId = transaction.OriginalTransaction.Payment.ProductIdentifier;
            // Register the purchase, so it is remembered for next time
            // PhotoFilterManager.Purchase(productId); // it's as though it was purchased again
            FinishTransaction(transaction, true);
        }

        public void FailedTransaction(SKPaymentTransaction transaction)
        {
            //SKErrorPaymentCancelled == 2
            if (transaction.Error.Code == 2) // user cancelled
                Debug.WriteLine("User CANCELLED FailedTransaction Code=" + transaction.Error.Code + " " + transaction.Error.LocalizedDescription);
            else // error!
                Debug.WriteLine("FailedTransaction Code=" + transaction.Error.Code + " " + transaction.Error.LocalizedDescription);

            FinishTransaction(transaction, false);
        }

        public void FinishTransaction(SKPaymentTransaction transaction, bool wasSuccessful)
        {
            Debug.WriteLine("FinishTransaction " + wasSuccessful);
            // remove the transaction from the payment queue.
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);		// THIS IS IMPORTANT - LET'S APPLE KNOW WE'RE DONE !!!!

            using (var pool = new NSAutoreleasePool())
            {
                NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { transaction }, new NSObject[] { new NSString("transaction") });
                if (wasSuccessful)
                {
                    // send out a notification that we’ve finished the transaction
                    NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerTransactionSucceededNotification, this, userInfo);
                }
                else
                {
                    // send out a notification for the failed transaction
                    NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerTransactionFailedNotification, this, userInfo);
                }
            }
        }

        /// <summary>
        /// Restore any transactions that occurred for this Apple ID, either on 
        /// this device or any other logged in with that account.
        /// </summary>
        public void Restore()
        {
            Debug.WriteLine(" ** InAppPurchaseManager Restore()");
            // theObserver will be notified of when the restored transactions start arriving <- AppStore
            SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
        }

        #endregion

    }
    internal class CustomPaymentObserver : SKPaymentTransactionObserver
    {
        private IInappService theManager;

        public CustomPaymentObserver(InappService manager)
        {
            theManager = manager;
        }

        // called when the transaction status is updated
        public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
        {
            Debug.WriteLine("UpdatedTransactions");
            foreach (SKPaymentTransaction transaction in transactions)
            {
                switch (transaction.TransactionState)
                {
                    case SKPaymentTransactionState.Purchased:
                        theManager.CompleteTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Failed:
                        theManager.FailedTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Restored:
                        theManager.RestoreTransaction(transaction);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void PaymentQueueRestoreCompletedTransactionsFinished(SKPaymentQueue queue)
        {
            // Restore succeeded
            Debug.WriteLine(" ** RESTORE PaymentQueueRestoreCompletedTransactionsFinished ");
        }

        public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error)
        {
            // Restore failed somewhere...
            Debug.WriteLine(" ** RESTORE RestoreCompletedTransactionsFailedWithError " + error.LocalizedDescription);
        }
    }

#elif DESKTOP
    
    public class InappService : GameComponent, IInappService
    {
        public InappService(Game game)
            : base(game)
        {
        }

    }

#elif WINDOWS_PHONE

    public class InappService : GameComponent, IInappService
    {
        public InappService(Game game)
            : base(game)
        {
        }

    }

#elif NETFX_CORE

    public class InappService : GameComponent, IInappService
    {
        public InappService(Game game)
            : base(game)
        {
        }

    }

#endif

}