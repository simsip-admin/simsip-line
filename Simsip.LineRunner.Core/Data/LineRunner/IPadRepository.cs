using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPadRepository
    {
        IList<PadEntity> GetPads();

        PadEntity GetPad(string modelName);
    }
}
