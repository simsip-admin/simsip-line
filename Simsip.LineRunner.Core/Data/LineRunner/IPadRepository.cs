using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPadRepository
    {
        IList<PadEntity> GetPads(string productId);

        PadEntity GetPad(string productId, string modelName);
    }
}
