using System.Collections.Generic;
using Simsip.LineRunner.Entities.Simsip;


namespace Simsip.LineRunner.Data.Simsip
{
    public interface ISimsipPurchaseRepository
    {
        void Create(SimsipPurchaseEntity product);
        SimsipPurchaseEntity GetPurchaseByServerId(string serverId);
        IList<SimsipPurchaseEntity> GetAllPurchases();
        void Update(SimsipPurchaseEntity purchase);
        void Delete(SimsipPurchaseEntity purchase);
        int Count { get; }
    }
}
