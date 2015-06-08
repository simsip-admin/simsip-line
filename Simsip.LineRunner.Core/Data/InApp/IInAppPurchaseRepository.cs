using System.Collections.Generic;
using Simsip.LineRunner.Entities.InApp;


namespace Simsip.LineRunner.Data.InApp
{
    public interface IInAppPurchaseRepository
    {
        void Create(InAppPurchaseEntity product);
        InAppPurchaseEntity GetPurchaseByOrderId(string orderId);
        IList<InAppPurchaseEntity> GetAllPurchases();
        void Update(InAppPurchaseEntity purchase);
        void Delete(InAppPurchaseEntity purchase);
        int Count { get; }
    }
}
