using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Simsip.LineRunner.Data.InApp;
using Simsip.LineRunner.Entities.InApp;

#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;
using System;
#elif IOS
using StoreKit;
using Foundation;
using MonoTouch;
using System;
#endif


namespace Simsip.LineRunner.Services.Inapp
{
#if ANDROID

    public class InappService : IInappService
    {
        private InAppBillingServiceConnection _serviceConnection;

        public InappService()
        {
            // Register as service
            // TheGame.SharedGame.Services.AddService(typeof(IInappService), this);
        }
        
        #region IInappService implementation


        // public string PracticeModeProductId { get { return "com.simsip.linerunner.practicemode"; } }
        // Testing
        public string PracticeModeProductId { get { return ReservedTestProductIDs.Purchased; } }

        public void Initialize()
        {
            // A Licensing and In-App Billing public key is required before an app can communicate with
            // Google Play, however you DON'T want to store the key in plain text with the application.
            // The Unify command provides a simply way to obfuscate the key by breaking it into two or
            // or more parts, specifying the order to reassemlbe those parts and optionally providing
            // a set of key/value pairs to replace in the final string. 
            string value = Security.Unify(
                new string[] { 
                    "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAhKyKc37WFTHmYoBQkxCRcKz6RfDXTNV59OqAYhATPUdZ4Y2at3X2dECM+G6wCMfnM+Z7TayYGHeuctWFh",
                    "SJzZR3CkGL2ueXuoKKdViJX1ti8btY8Htu9n4t841F9wJVE+YW+A7VJ49cyj+7oc1t6q/6uFDrEdZU+FuAweTQuFjR12RNxxckf2w6HhLQwzNOWdv8agaWI2sK8kaF",
                    "rA5kX6kAtjAflyWdDOnk0dAxXFji4qADmsquL668tYVFryk8XXn7EnE4kjxrWeWf6GBPNbfRZulUgvs4RBQ0mj8WFD+cDw36TbrQCAk0JePskFwWxtnA0pEL63+duO",
                    "HPZ7ndRZQIDAQAB" },
                new int[] { 0, 1, 2, 3 });

            // Create a new connection to the Google Play Service
            _serviceConnection = new InAppBillingServiceConnection(Program.SharedProgram /*TheGame.Activity*/, value);
            _serviceConnection.OnConnected += () =>
            {
                this._serviceConnection.BillingHandler.OnProductPurchased += (int response, Purchase purchase, string purchaseData, string purchaseSignature) =>
                {
                    // Record what we purchased
                    var inAppPurchaseRepository = new InAppPurchaseRepository();
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var purchaseTime = epoch.AddMilliseconds(purchase.PurchaseTime);

                    var existingPurchase = inAppPurchaseRepository.GetPurchaseByProductId(purchase.ProductId);

                    if (existingPurchase == null)
                    {
                        var newPurchase = new InAppPurchaseEntity
                        {
                            OrderId = purchase.OrderId,
                            ProductId = purchase.ProductId,
                            PurchaseTime = purchaseTime
                        };

                        inAppPurchaseRepository.Create(newPurchase);
                    }
                    else
                    {
                        existingPurchase.OrderId = purchase.OrderId;
                        existingPurchase.ProductId = purchase.ProductId;
                        existingPurchase.PurchaseTime = purchaseTime;

                        inAppPurchaseRepository.Update(existingPurchase);
                    }

                    // Let anyone know who is interested that purchase has completed
                    if (this.OnPurchaseProduct != null)
                    {
                        this.OnPurchaseProduct();
                    }
                };
                this._serviceConnection.BillingHandler.QueryInventoryError += (int responseCode, Bundle skuDetails) =>
                {
                    if (this.OnQueryInventoryError != null)
                    {
                        this.OnQueryInventoryError(responseCode, null);
                    }
                };
                this._serviceConnection.BillingHandler.BuyProductError += (int responseCode, string sku) =>
                {
                    // Note, BillingResult.ItemAlreadyOwned, etc. can be used to determine error

                    if (this.OnPurchaseProductError != null)
                    {
                        this.OnPurchaseProductError(responseCode, sku);
                    }
                };
                this._serviceConnection.BillingHandler.InAppBillingProcesingError += (string message) =>
                {
                    if (this.OnInAppBillingProcesingError != null)
                    {
                        this.OnInAppBillingProcesingError(message);
                    }
                };
                this._serviceConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += (Bundle ownedItems) =>
                {
                    if (this.OnInvalidOwnedItemsBundleReturned != null)
                    {
                        this.OnInvalidOwnedItemsBundleReturned(null);
                    }
                };
                this._serviceConnection.BillingHandler.OnProductPurchasedError += (int responseCode, string sku) =>
                {
                    if (this.OnPurchaseProductError != null)
                    {
                        this.OnPurchaseProductError(responseCode, sku);
                    }
                };
                this._serviceConnection.BillingHandler.OnPurchaseFailedValidation += (Purchase purchase, string purchaseData, string purchaseSignature) =>
                {
                    if (this.OnPurchaseFailedValidation != null)
                    {
                        this.OnPurchaseFailedValidation(null, purchaseData, purchaseSignature);
                    }
                };
                this._serviceConnection.BillingHandler.OnUserCanceled += () =>
                {
                    if (this.OnUserCanceled != null)
                    {
                        this.OnUserCanceled();
                    }
                };

                // Load inventory or available products
                this.QueryInventory();
            };

            // Attempt to connect to the service
            _serviceConnection.Connect();

        }

        public void HandleActivityResult(int requestCode, Result resultCode, Intent data)
        {
            this._serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
        }

        public async void QueryInventory()
        {
            var products = await this._serviceConnection.BillingHandler.QueryInventoryAsync(
                                new List<string>() 
                                    { 
                                        this.PracticeModeProductId
                                        // ReservedTestProductIDs.Canceled
                                    }, 
                                    ItemType.Product);

            // Update inventory
            var inAppSkuRepository = new InAppSkuRepository();
            foreach (var product in products)
            {
                var existingProduct = inAppSkuRepository.GetSkuByProductId(product.ProductId);
                if (existingProduct != null)
                {
                    existingProduct.Type = ItemType.Product;
                    existingProduct.Price = product.Price;
                    existingProduct.Title = product.Title;
                    existingProduct.Description = product.Description;
                    existingProduct.PriceCurrencyCode = product.Price_Currency_Code;

                    inAppSkuRepository.Update(existingProduct);
                }
                else
                {
                    var newProduct = new InAppSkuEntity();
                    newProduct.ProductId = product.ProductId;
                    newProduct.Type = ItemType.Product;
                    newProduct.Price = product.Price;
                    newProduct.Title = product.Title;
                    newProduct.Description = product.Description;
                    newProduct.PriceCurrencyCode = product.Price_Currency_Code;

                    inAppSkuRepository.Create(newProduct);
                }
            }

            // If we have any purchases for the inventory, update the purchases
            var inAppPurchaseRepository = new InAppPurchaseRepository();
            foreach (var product in products)
            {
                var existingPurchase = inAppPurchaseRepository.GetPurchaseByProductId(product.ProductId);
                if (existingPurchase != null)
                {
                    // Nothing to do for now
                }
            }

            if (this.OnQueryInventory != null)
            {
                this.OnQueryInventory();
            }
            
            /* Example:
            // Ask the open connection's billing handler to return a list of avilable products for the 
            // given list of items.
            // NOTE: We are asking for the Reserved Test Product IDs that allow you to test In-App
            // Billing without actually making a purchase.
            _products = await _serviceConnection.BillingHandler.QueryInventoryAsync(new List<string> {
                "appra_01_test",
                "appra_02_sub",
                ReservedTestProductIDs.Purchased
            }, ItemType.Product);
            */
        }

        public void PurchaseProduct(string productId)
        {
            // See Initialize() for where we hook up event handler for this
            this._serviceConnection.BillingHandler.BuyProduct(productId, ItemType.Product, "test");
        }

        public void RestoreProducts()
        {
            var purchases = this._serviceConnection.BillingHandler.GetPurchases(ItemType.Product);

            // Record what we restored
            var inAppPurchaseRepository = new InAppPurchaseRepository();
            foreach (var purchase in purchases)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var purchaseTime = epoch.AddMilliseconds(purchase.PurchaseTime);

                var existingPurchase = inAppPurchaseRepository.GetPurchaseByProductId(purchase.ProductId);

                if (existingPurchase == null)
                {
                    var newPurchase = new InAppPurchaseEntity
                    {
                        OrderId = purchase.OrderId,
                        ProductId = purchase.ProductId,
                        PurchaseTime = purchaseTime
                    };

                    inAppPurchaseRepository.Create(newPurchase);
                }
                else
                {
                    existingPurchase.OrderId = purchase.OrderId;
                    existingPurchase.ProductId = purchase.ProductId;
                    existingPurchase.PurchaseTime = purchaseTime;

                    inAppPurchaseRepository.Update(existingPurchase);
                }
            }

            // Notifiy anyone who needs to know products were restored
            if (this.OnRestoreProducts != null)
            {
                this.OnRestoreProducts();
            }
        }

        public void RefundProduct()
        {
            var purchases = this._serviceConnection.BillingHandler.GetPurchases(ItemType.Product);

            this._serviceConnection.BillingHandler.ConsumePurchase(purchases[0].PurchaseToken);
        }

        public void OnDestroy()
        {
            // Are we attached to the Google Play Service?
            if (this._serviceConnection != null)
            {
                // Yes, disconnect
                this._serviceConnection.Disconnect();
            }
        }

        public event OnQueryInventoryDelegate OnQueryInventory;

        public event OnPurchaseProductDelegate OnPurchaseProduct;

        public event OnRestoreProductsDelegate OnRestoreProducts;

        public event OnQueryInventoryErrorDelegate OnQueryInventoryError;

        public event OnPurchaseProductErrorDelegate OnPurchaseProductError;

        public event OnRestoreProductsErrorDelegate OnRestoreProductsError;

        public event OnUserCanceledDelegate OnUserCanceled;

        public event OnInAppBillingProcessingErrorDelegate OnInAppBillingProcesingError;

        public event OnInvalidOwnedItemsBundleReturnedDelegate OnInvalidOwnedItemsBundleReturned;

        public event OnPurchaseFailedValidationDelegate OnPurchaseFailedValidation;

        #endregion

    }

#elif IOS

    public class InappService : SKProductsRequestDelegate, IInappService
    {
        private CustomPaymentObserver _customPaymentObserver;

        public static NSString InAppQueryInventoryNotification = new NSString("InAppQueryInventoryNotification");
        public static NSString InAppQueryInventoryErrorNotification = new NSString("InAppQueryInventoryErrorNotification");
        public static NSString InAppPurchaseProductNotification = new NSString("InAppPurchaseProductNotification");
        public static NSString InAppPurchaseProductErrorNotification = new NSString("InAppPurchaseProductErrorNotification");
        public static NSString InAppRestoreProductsNotification = new NSString("InAppRestoreProductsNotification");
        public static NSString InAppRestoreProductsErrorNotification = new NSString("InAppRestoreProductsErrorNotification");

        private NSObject _queryInventoryObserver;
        private NSObject _queryInventoryErrorObserver;
        private NSObject _purchaseProductObserver;
        private NSObject _purchaseProductErrorObserver;
        private NSObject _restoreProductsObserver;
        private NSObject _restoreProductsErrorObserver;

        private SKProductsRequest _productsRequest;

        public InappService()
        {
            // Register as service
            TheGame.SharedGame.Services.AddService(typeof(IInappService), this);  
        }

        #region SKProductsRequestDelegate overrides

        // Received response to RequestProductData - with price, title, description info
		public override void ReceivedResponse (SKProductsRequest request, SKProductsResponse response)
		{
			SKProduct[] products = response.Products;

			NSDictionary userInfo = null;
			if (products.Length > 0) 
            {
				NSObject[] productIdsArray = new NSObject[response.Products.Length];
				NSObject[] productsArray = new NSObject[response.Products.Length];
				for (int i = 0; i < response.Products.Length; i++) {
					productIdsArray[i] = new NSString(response.Products[i].ProductIdentifier);
					productsArray[i] = response.Products[i];
				}
				userInfo = NSDictionary.FromObjectsAndKeys (productsArray, productIdsArray);
			}
			NSNotificationCenter.DefaultCenter.PostNotificationName(
                InAppQueryInventoryNotification,
                this,
                userInfo);

			foreach (string invalidProductId in response.InvalidProducts) 
            {
				Debug.WriteLine("Invalid product id: " + invalidProductId );
			}
		}

        // Probably could not connect to the App Store (network unavailable?)
        public override void RequestFailed(SKRequest request, NSError error)
        {
            Debug.WriteLine(" ** InAppPurchaseManager RequestFailed() " + error.LocalizedDescription);
            using (var pool = new NSAutoreleasePool())
            {
                NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { error }, new NSObject[] { new NSString("error") });
                // send out a notification for the failed transaction
                NSNotificationCenter.DefaultCenter.PostNotificationName(InAppQueryInventoryErrorNotification, this, userInfo);
            }
        }

        #endregion

        #region IInappService implementation

        public string PracticeModeProductId { get { return "com.simsip.linerunner.practicemode"; } }

        public void Initialize()
        {
            this._customPaymentObserver = new CustomPaymentObserver(this);
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(this._customPaymentObserver);

            this._queryInventoryObserver = NSNotificationCenter.DefaultCenter.AddObserver(InappService.InAppQueryInventoryNotification,
                (notification) =>
                {
                    var info = notification.UserInfo;
                    var practiceModeProductId = new NSString(this.PracticeModeProductId);

                    var product = (SKProduct)info.ObjectForKey(practiceModeProductId);

                    // Update inventory
                    var inAppSkuRepository = new InAppSkuRepository();
                    var existingProduct = inAppSkuRepository.GetSkuByProductId(this.PracticeModeProductId);
                    if (existingProduct != null)
                    {
                        existingProduct.Type = "inapp";
                        existingProduct.Price = this.LocalizedPrice(product);
                        existingProduct.Title = product.LocalizedTitle;
                        existingProduct.Description = product.LocalizedDescription;

                        inAppSkuRepository.Update(existingProduct);
                    }
                    else
                    {
                        var newProduct = new InAppSkuEntity();
                        newProduct.ProductId = this.PracticeModeProductId;
                        newProduct.Type = "inapp";
                        newProduct.Price = this.LocalizedPrice(product);
                        newProduct.Title = product.LocalizedTitle;
                        newProduct.Description = product.LocalizedDescription;

                        inAppSkuRepository.Create(newProduct);
                    }

                    // If we have any purchases for the inventory, update the purchases
                    var inAppPurchaseRepository = new InAppPurchaseRepository();
                    var existingPurchase = inAppPurchaseRepository.GetPurchaseByProductId(this.PracticeModeProductId);
                    if (existingPurchase != null)
                    {
                        // Nothing to do for now
                    }

                    // Notify anyone who needed to know that our inventory is in
                    if (this.OnQueryInventory != null)
                    {
                        this.OnQueryInventory();
                    }
                });

            this._queryInventoryErrorObserver = NSNotificationCenter.DefaultCenter.AddObserver(InappService.InAppQueryInventoryErrorNotification,
                (notification) =>
                {
                    // Notify anyone who needed to know that there was a query inventory error
                    if (this.OnQueryInventoryError != null)
                    {
                        this.OnQueryInventoryError(0, null);
                    }
                });

            this._purchaseProductObserver = NSNotificationCenter.DefaultCenter.AddObserver(InappService.InAppPurchaseProductNotification,
                (notification) =>
                {
                    // Notify anyone who needed to know that product was purchased
                    if (this.OnPurchaseProduct != null)
                    {
                        this.OnPurchaseProduct();
                    }

                });

            this._purchaseProductErrorObserver = NSNotificationCenter.DefaultCenter.AddObserver(InappService.InAppPurchaseProductErrorNotification,
                (notification) =>
                {
                    // Notify anyone who needed to know that there was a product purchase error
                    if (this.OnPurchaseProductError != null)
                    {
                        this.OnPurchaseProductError(0, string.Empty);
                    }
                });

            this._restoreProductsObserver = NSNotificationCenter.DefaultCenter.AddObserver(InappService.InAppRestoreProductsNotification,
                (notification) =>
                {
                    // Notify anyone who needed to know that products were restored
                    if (this.OnRestoreProducts != null)
                    {
                        this.OnRestoreProducts();
                    }

                });

            this._restoreProductsErrorObserver = NSNotificationCenter.DefaultCenter.AddObserver(InappService.InAppRestoreProductsErrorNotification,
                (notification) =>
                {
                    // Notify anyone who needed to know that there was an error in restoring products
                    if (this.OnRestoreProductsError != null)
                    {
                        this.OnRestoreProductsError(0, null);
                    }
                });

            if (this.CanMakePayments())
            {
                // Async request 
                // StoreKit -> App Store -> ReceivedResponse (see below)
                this.QueryInventory();
            }

        }

        public void QueryInventory()
        {
            var array = new NSString[1];
            array[0] = new NSString(this.PracticeModeProductId);
            NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array);

            // Set up product request for in-app purchase to be handled in
            // SKProductsRequestDelegate.ReceivedResponse (see above)
            this._productsRequest = new SKProductsRequest(productIdentifiers);
            this._productsRequest.Delegate = this; 
            this._productsRequest.Start();
        }

        public void PurchaseProduct(string productId)
        {
            // Construct a payment request
            var payment = SKPayment.PaymentWithProduct(productId);

            // Queue the payment request up
            // Will be handled in:
            // CustomPaymentObserver.UpdatedTransactions -> InAppService.PurchaseTransaction - InAppService.FinishTransaction
            SKPaymentQueue.DefaultQueue.AddPayment(payment);
        }

        /// <summary>
        /// RestoreProducts any transactions that occurred for this Apple ID, either on 
        /// this device or any other logged in with that account.
        /// </summary>
        public void RestoreProducts()
        {
            Debug.WriteLine(" ** InAppPurchaseManager Restore()");
            // theObserver will be notified of when the restored transactions start arriving <- AppStore
            SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
        }

        public bool CanMakePayments()
        {
            return SKPaymentQueue.CanMakePayments;
        }

        public void WillTerminate()
        {
            // Remove the observer when the app exits
            NSNotificationCenter.DefaultCenter.RemoveObserver(this._queryInventoryObserver);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this._queryInventoryErrorObserver);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this._purchaseProductObserver);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this._purchaseProductErrorObserver);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this._restoreProductsObserver);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this._restoreProductsErrorObserver);
        }

        public event OnQueryInventoryDelegate OnQueryInventory;

        public event OnPurchaseProductDelegate OnPurchaseProduct;

        public event OnRestoreProductsDelegate OnRestoreProducts;

        public event OnQueryInventoryErrorDelegate OnQueryInventoryError;

        public event OnPurchaseProductErrorDelegate OnPurchaseProductError;

        public event OnRestoreProductsErrorDelegate OnRestoreProductsError;

        public event OnUserCanceledDelegate OnUserCanceled;

        public event OnInAppBillingProcessingErrorDelegate OnInAppBillingProcesingError;

        public event OnInvalidOwnedItemsBundleReturnedDelegate OnInvalidOwnedItemsBundleReturned;

        public event OnPurchaseFailedValidationDelegate OnPurchaseFailedValidation;

        #endregion

        #region Api

        public void PurchaseTransaction(SKPaymentTransaction transaction)
        {
            var productId = transaction.Payment.ProductIdentifier;

            // Record the purchase
            var inAppPurchaseRepository = new InAppPurchaseRepository();
            var existingPurchase = inAppPurchaseRepository.GetPurchaseByProductId(productId);
            if (existingPurchase == null)
            {
                var newPurchase = new InAppPurchaseEntity
                    {
                        OrderId = transaction.TransactionIdentifier,
                        ProductId = transaction.Payment.ProductIdentifier,
                        PurchaseTime = NSDateToDateTime(transaction.TransactionDate)
                    };

                inAppPurchaseRepository.Create(newPurchase);
            }
            else
            {
                existingPurchase.OrderId = transaction.TransactionIdentifier;
                existingPurchase.ProductId = transaction.Payment.ProductIdentifier;
                existingPurchase.PurchaseTime = NSDateToDateTime(transaction.TransactionDate);

                inAppPurchaseRepository.Update(existingPurchase);
            }

            // Remove the transaction from the payment queue.
            // IMPORTANT: Let's ios know we're done
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);

            // Send out a notification that we’ve finished the transaction
            using (var pool = new NSAutoreleasePool())
            {
                NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { transaction }, new NSObject[] { new NSString("transaction") });
                NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseProductNotification, this, userInfo);
            }
        }

        public void RestoreTransaction(SKPaymentTransaction transaction)
        {
            // Restored Transactions always have an 'original transaction' attached
            var productId = transaction.OriginalTransaction.Payment.ProductIdentifier;

            // Record the restore
            var inAppPurchaseRepository = new InAppPurchaseRepository();
            var existingPurchase = inAppPurchaseRepository.GetPurchaseByProductId(productId);
            if (existingPurchase == null)
            {
                var newPurchase = new InAppPurchaseEntity
                {
                    OrderId = transaction.OriginalTransaction.TransactionIdentifier,
                    ProductId = transaction.OriginalTransaction.Payment.ProductIdentifier,
                    PurchaseTime = NSDateToDateTime(transaction.OriginalTransaction.TransactionDate)
                };

                inAppPurchaseRepository.Create(newPurchase);
            }
            else
            {
                existingPurchase.OrderId = transaction.OriginalTransaction.TransactionIdentifier;
                existingPurchase.ProductId = transaction.OriginalTransaction.Payment.ProductIdentifier;
                existingPurchase.PurchaseTime = NSDateToDateTime(transaction.OriginalTransaction.TransactionDate);

                inAppPurchaseRepository.Update(existingPurchase);
            }

            // Remove the transaction from the payment queue.
            // IMPORTANT: Let's ios know we're done
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);

            // Send out a notification that we’ve finished the transaction
            using (var pool = new NSAutoreleasePool())
            {
                NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { transaction }, new NSObject[] { new NSString("transaction") });
                NSNotificationCenter.DefaultCenter.PostNotificationName(InAppRestoreProductsNotification, this, userInfo);
            }
        }

        public void FailedTransaction(SKPaymentTransaction transaction)
        {
            //SKErrorPaymentCancelled == 2
            if (transaction.Error.Code == 2) // user cancelled
            {
                Debug.WriteLine("User CANCELLED FailedTransaction Code=" + transaction.Error.Code + " " + transaction.Error.LocalizedDescription);
            }
            else // error!
            {
                Debug.WriteLine("FailedTransaction Code=" + transaction.Error.Code + " " + transaction.Error.LocalizedDescription);
            }

            // Remove the transaction from the payment queue.
            // IMPORTANT: Let's ios know we're done
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
            
            // Send out a notification for the failed transaction
            using (var pool = new NSAutoreleasePool())
            {
                NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { transaction }, new NSObject[] { new NSString("transaction") });
                NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseProductErrorNotification, this, userInfo);
            }
        }

        #endregion

        #region Helper methods

        private string LocalizedPrice(SKProduct product)
        {
            var formatter = new NSNumberFormatter();
            formatter.FormatterBehavior = NSNumberFormatterBehavior.Version_10_4;
            formatter.NumberStyle = NSNumberFormatterStyle.Currency;
            formatter.Locale = product.PriceLocale;
            var formattedString = formatter.StringFromNumber(product.Price);
            return formattedString;
        }

        private DateTime NSDateToDateTime(NSDate date)
        {
            // NSDate has a wider range than DateTime, so clip
            // the converted date to DateTime.Min|MaxValue.
            double secs = date.SecondsSinceReferenceDate;
            if (secs < -63113904000)
                return DateTime.MinValue;
            if (secs > 252423993599)
                return DateTime.MaxValue;
            return (DateTime)date;
        }

        #endregion

    }

    internal class CustomPaymentObserver : SKPaymentTransactionObserver
    {
        private InappService _inAppService;

        public CustomPaymentObserver(InappService inAppService)
        {
            _inAppService = inAppService;
        }

        public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
        {
            foreach (SKPaymentTransaction transaction in transactions)
            {
                switch (transaction.TransactionState)
                {
                    case SKPaymentTransactionState.Purchased:
                        this._inAppService.PurchaseTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Failed:
                        this._inAppService.FailedTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Restored:
                        this._inAppService.RestoreTransaction(transaction);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void PaymentQueueRestoreCompletedTransactionsFinished(SKPaymentQueue queue)
        {
            // RestoreProducts succeeded
            Debug.WriteLine(" ** RESTORE PaymentQueueRestoreCompletedTransactionsFinished ");
        }

        public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error)
        {
            // RestoreProducts failed somewhere...
            Debug.WriteLine(" ** RESTORE RestoreCompletedTransactionsFailedWithError " + error.LocalizedDescription);
        }
    }

#endif

}