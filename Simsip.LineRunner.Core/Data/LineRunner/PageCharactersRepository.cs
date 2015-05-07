using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System.Collections.Generic;
using System.Linq;


namespace Simsip.LineRunner.Data.LineRunner
{
    public class PageCharactersRepository : IPageCharactersRepository
    {
        public List<PageCharactersEntity> GetCharacters(int pageNumber)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<PageCharactersEntity>()
                                  .Where(x => x.PageNumber == pageNumber)
                                  .OrderBy(x => x.LineNumber)
                                  .ThenBy(x => x.CharacterNumber);

                    return results.ToList();
                }
            }
        }

        public List<PageCharactersEntity> GetCharacters(int pageNumber, int lineNumber)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<PageCharactersEntity>()
                                  .Where(x => x.PageNumber == pageNumber &&
                                              x.LineNumber == lineNumber)
                                  .OrderBy(x => x.CharacterNumber);

                    return results.ToList();
                }
            }
        }

        public List<PageCharactersEntity> GetCharacters(int pageNumber, int[] lineNumbers)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<PageCharactersEntity>()
                                  .Where(x => x.PageNumber == pageNumber &&
                                              lineNumbers.Contains(x.LineNumber))
                                  .OrderBy(x => x.CharacterNumber);

                    return results.ToList();
                }
            }
        }
        public PageCharactersEntity GetCharacter(int pageNumber, int lineNumber, int characterNumber)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<PageCharactersEntity>()
                                  .Where(x => x.PageNumber == pageNumber &&
                                              x.LineNumber == lineNumber &&
                                              x.CharacterNumber == characterNumber))
                                 .FirstOrDefault<PageCharactersEntity>();

                    return result;
                }
            }
        }
    }
}


