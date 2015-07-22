using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace Simsip.LineRunner.Data.LineRunner
{
    public class PageLinesRepository : IPageLinesRepository
    {
        public List<PageLinesEntity> GetLines(string productId, int pageNumber)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<PageLinesEntity>()
                                  .Where(x => x.ProductId == productId &&
                                              x.PageNumber == pageNumber)
                                  .OrderBy(x => x.LineNumber);

                    return results.ToList();
                }
            }
        }

        public List<PageLinesEntity> GetLines(string productId, int pageNumber, int[] lineNumbers)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = (connection.Table<PageLinesEntity>()
                                 .Where(x => x.ProductId == productId &&
                                             x.PageNumber == pageNumber &&
                                             lineNumbers.Contains(x.LineNumber)))
                                  .OrderBy(x => x.LineNumber);

                    return results.ToList();
                }
            }
        }

        public PageLinesEntity GetLine(string productId, int pageNumber, int lineNumber)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<PageLinesEntity>()
                                 .Where(x => x.ProductId == productId &&
                                             x.PageNumber == pageNumber &&
                                             x.LineNumber == lineNumber))
                                 .FirstOrDefault<PageLinesEntity>();

                    return result;
                }
            }
        }
    }
}


