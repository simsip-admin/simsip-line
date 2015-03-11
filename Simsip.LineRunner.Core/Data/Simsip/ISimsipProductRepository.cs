using System.Collections.Generic;
using Simsip.LineRunner.Entities.Simsip;


namespace Simsip.LineRunner.Data.Simsip
{
    public interface ISimsipProductRepository
    {
        void Create(SimsipProductEntity product);
        SimsipProductEntity GetProductByServerId(string serverId);
        IList<SimsipProductEntity> GetProducts();
        void Update(SimsipProductEntity product);
        void Delete(SimsipProductEntity product);
        int Count { get; }
    }
}
