using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Simsip
{
    public interface ISimsipFeedService
    {
        Task<IList<FacebookHybridEntity>> GetFeedAsync();
    }
}
