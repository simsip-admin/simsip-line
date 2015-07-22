using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;


namespace Simsip.LineRunner.Data.LineRunner
{
    public interface ILineRepository
    {
        LineEntity GetLine(string productId, string modelName);
        IList<LineEntity> GetLines(string productId);
    }
}
