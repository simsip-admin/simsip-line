using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Simsip
{
    public interface ISimsipProductService
    {
        Task<string> AddProductAsync(SimsipProductEntity product);

        Task<IList<SimsipProductEntity>> GetProductsAsync();

        Task UpdateProductAsync(SimsipProductEntity product);

        Task DeleteProductAsync(SimsipProductEntity product);
    }
}
