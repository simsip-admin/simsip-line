using System.Collections.Generic;
using Simsip.LineRunner.Entities.InApp;


namespace Simsip.LineRunner.Data.InApp
{
    public interface IInAppSkuRepository
    {
        void Create(InAppSkuEntity sku);
        InAppSkuEntity GetSkuByProductId(string productId);
        IList<InAppSkuEntity> GetAllSkus();
        void Update(InAppSkuEntity sku);
        void Delete(InAppSkuEntity sku);
        int Count { get; }
    }
}
