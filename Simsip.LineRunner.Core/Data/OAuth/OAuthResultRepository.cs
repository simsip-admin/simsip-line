using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.OAuth;

namespace Simsip.LineRunner.Data.OAuth
{
    public class OAuthResultRepository : IOAuthResultRepository
    {
        public void Create(OAuthResultEntity result)
        {
            // Ok, good to go insert our one record for this type
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            // Remove any previous records so we mainntain only one record
                            // for a particular type
                            var existingModels = connection.Table<OAuthResultEntity>()
                                                 .Where(x => x.Type == result.Type)
                                                 .ToList();
                            foreach (var existingModel in existingModels)
                            {
                                connection.Delete(existingModel);
                            }

                            connection.Insert(result);
                        });
                }
            }
        }

        public OAuthResultEntity Read(OAuthType type)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    return connection.Table<OAuthResultEntity>()
                           .FirstOrDefault(x => x.Type == type);
                }
            }
        }

        public void Update(OAuthResultEntity result)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(result);
                        });
                }
            }

        }

        public void Delete(OAuthResultEntity result)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(result);
                        });
                }
            }
        }

        public int Count
        {
            get
            {
                lock (Database.DATABASE_LOCK)
                {
                    using (var connection = new SQLiteConnection(Database.DatabasePath()))
                    {
                        return connection.Table<OAuthResultEntity>().Count();
                    }
                }
            }
        }
    }
}


