using System;
using  System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Data.Simsip;
// using Simsip.Plugins.MobileServices;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Simsip
{
    public class SimsipProductService : ISimsipProductService
    {
        private const string TRACE_TAG = "SimsipProductService";

        private readonly ISimsipProductRepository _productRepository;
        
        public SimsipProductService(ISimsipProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        #region ISimsipProductService Implementation

        public async Task<string> AddProductAsync(SimsipProductEntity product) 
        {
            throw new NotImplementedException();
        }

        public async Task<IList<SimsipProductEntity>> GetProductsAsync()
        {
            var returnProducts = new List<SimsipProductEntity>();

            try
            {
                /* TODO
                var productTable = App.MobileServiceClient.GetTable(MobileServiceConstants.ProductTableName);
                var response = await productTable.ReadAsync("");

                returnProducts = ParseGetProductsResponse(response);
                */
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception getting simsip products: " + ex, new object[] { });
            }

            return returnProducts;
        }

        public async Task UpdateProductAsync(SimsipProductEntity product) 
        {
            throw new NotImplementedException();
        }

        public async Task DeleteProductAsync(SimsipProductEntity product) 
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper functions

        private List<SimsipProductEntity> ParseGetProductsResponse(JToken response)
        {
            var returnProducts = new List<SimsipProductEntity>();

            try
            {
                var productCount = response.Count();
                for (int i = 0; i < productCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedProduct = response[i];
                        var productModel = new SimsipProductEntity();

                        // Get the simplistic values first
                        productModel.ServerId = parsedProduct["id"].Value<string>();
                        productModel.Name = parsedProduct["name"].Value<string>();
                        productModel.Description = parsedProduct["description"].Value<string>();
                        productModel.ImageUrl = parsedProduct["link"].Value<string>();

                        // Is this a new product?
                        var productOnDevice = _productRepository.GetProductByServerId(productModel.ServerId);
                        if (productOnDevice == null)
                        {
                            _productRepository.Create(productModel);
                        }
                        else
                        {
                            _productRepository.Update(productModel);
                        }

                        returnProducts.Add(productModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual simsip product: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing simsip products: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnProducts;
        }

        #endregion
    }
}