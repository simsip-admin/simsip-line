using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPageLinesRepository
    {
        List<PageLinesEntity> GetLines(int pageNumber);
        PageLinesEntity GetLine(int pageNumber, int lineNumber);
    }
}
