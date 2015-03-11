using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Data.Simsip;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Simsip
{
    public class SimsipPurchaseService : ISimsipPurchaseService
    {
        private const string TRACE_TAG = "SimsipPurchaseService";

        private readonly ISimsipPurchaseRepository _purchaseRepository;
        
        public SimsipPurchaseService(ISimsipPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        #region ISimsipPurchaseService Implementation

        public async Task<SimsipPurchaseEntity> AddPurchaseAsync(SimsipPurchaseEntity purchase) 
        {
            SimsipPurchaseEntity returnPurchase = null;

            try
            {
                /* TODO
                var purchaseTable = App.MobileServiceClient.GetTable(MobileServiceConstants.PurchaseTableName);

                var purchaseJson = new JObject();
                purchaseJson.Add("simsip_id", purchase.SimsipUserId);
                purchaseJson.Add("product_id", purchase.ProductId);

                var response = await purchaseTable.InsertAsync(purchaseJson);

                returnPurchase = ParsePurchaseResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding simsip purchase: " + ex);
            }

            return returnPurchase;
        }

        public async Task<IList<SimsipPurchaseEntity>> GetPurchasesAsync(SimsipUserEntity simsipUser) 
        {
            List<SimsipPurchaseEntity> returnPurchases = new List<SimsipPurchaseEntity>();

            try
            {
                /* TODO
                var purchaseTable = App.MobileServiceClient.GetTable(MobileServiceConstants.PurchaseTableName);

                var response = await purchaseTable.ReadAsync("$filter=simsip_id eq " + simsipUser.ServerId);

                returnPurchases = ParseGetPurchasesResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting simsip purchases: " + ex);
            }

            return returnPurchases;
        }

        public async Task<SimsipPurchaseEntity> UpdatePurchaseAsync(SimsipPurchaseEntity purchase) 
        {
            SimsipPurchaseEntity returnPurchase = null;

            try
            {
                /* TODO
                var purchaseTable = App.MobileServiceClient.GetTable(MobileServiceConstants.PurchaseTableName);

                var purchaseJson = new JObject();
                purchaseJson.Add("id", purchase.ServerId);
                purchaseJson.Add("simsip_id", purchase.SimsipUserId);
                purchaseJson.Add("product_id", purchase.ProductId);

                var response = await purchaseTable.UpdateAsync(purchaseJson);

                returnPurchase = ParsePurchaseResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception updating simsip purchase: " + ex);
            }

            return returnPurchase;
        }

        public async Task DeletePurchaseAsync(SimsipPurchaseEntity purchase) 
        {
            try
            {
                /* TODO
                var purchaseTable = App.MobileServiceClient.GetTable(MobileServiceConstants.PurchaseTableName);

                var purchaseJson = new JObject();
                purchaseJson.Add("id", purchase.ServerId);

                await purchaseTable.DeleteAsync(purchaseJson);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception deleting simsip purchase: " + ex);
            }

            return;
        }

        #endregion

        #region Helper functions

        private SimsipPurchaseEntity ParsePurchaseResponse(JToken response)
        {
            SimsipPurchaseEntity returnPurchase = null;

            try
            {
                returnPurchase = new SimsipPurchaseEntity();

                // Get the simplistic values first
                returnPurchase.ServerId = response["id"].Value<string>();
                returnPurchase.SimsipUserId = response["simsip_user_id"].Value<string>();
                returnPurchase.ProductId = response["product_id"].Value<string>();

                // Is this a new purchase?
                var purchaseOnDevice = _purchaseRepository.GetPurchaseByServerId(returnPurchase.ServerId);
                if (purchaseOnDevice == null)
                {
                    _purchaseRepository.Create(returnPurchase);
                }
                else
                {
                    _purchaseRepository.Update(returnPurchase);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing simsip purchase: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnPurchase;
        }

        private List<SimsipPurchaseEntity> ParseGetPurchasesResponse(JToken response)
        {
            var returnPurchases = new List<SimsipPurchaseEntity>();

            try
            {
                var purchaseCount = response.Count();
                for (int i = 0; i < purchaseCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedPurchase = response[i];
                        var purchaseModel = ParsePurchaseResponse(parsedPurchase);

                        returnPurchases.Add(purchaseModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual simsip purchase: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing simsip purchases: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnPurchases;
        }

        #endregion

    }
}