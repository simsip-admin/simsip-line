using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;


namespace Simsip.LineRunner.Data.LineRunner
{
    public interface ILineRepository
    {
        LineEntity GetLine(string modelName);
        IList<LineEntity> GetLines();
    }
}
