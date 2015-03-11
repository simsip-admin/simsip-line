using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Simsip
{
    public interface ISimsipPurchaseService
    {
        Task<SimsipPurchaseEntity> AddPurchaseAsync(SimsipPurchaseEntity purchase);

        Task<IList<SimsipPurchaseEntity>> GetPurchasesAsync(SimsipUserEntity simsipUser);

        Task<SimsipPurchaseEntity> UpdatePurchaseAsync(SimsipPurchaseEntity purchase); 

        Task DeletePurchaseAsync(SimsipPurchaseEntity purchase); 
    }
}
