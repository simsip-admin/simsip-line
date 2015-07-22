using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPageLinesRepository
    {
        List<PageLinesEntity> GetLines(string productId, int pageNumber);
        List<PageLinesEntity> GetLines(string productId, int pageNumber, int[] lineNumbers);

        PageLinesEntity GetLine(string productId, int pageNumber, int lineNumber);
    }
}
