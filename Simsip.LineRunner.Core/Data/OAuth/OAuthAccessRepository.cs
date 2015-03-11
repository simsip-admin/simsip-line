using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.OAuth;

namespace Simsip.LineRunner.Data.OAuth
{
    public class OAuthAccessRepository : IOAuthAccessRepository
    {

        public void Create(OAuthAccessEntity access)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            // Remove any previous records so we mainntain only one record
                            // for a particular type
                            var existingModels = connection.Table<OAuthAccessEntity>()
                                                 .Where(x => x.Type == access.Type)
                                                 .ToList();
                            foreach (var existingModel in existingModels)
                            {
                                connection.Delete(existingModel);
                            }

                            // Ok, good to go insert our one record for this type
                            connection.Insert(access);
                        });
                }
            }
        }

        public OAuthAccessEntity Read(OAuthType type)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    return connection.Table<OAuthAccessEntity>()
                           .FirstOrDefault(x => x.Type == type);
                }
            }
        }

        public void Update(OAuthAccessEntity access)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(access);
                        });
                }
            }
        }

        public void Delete(OAuthAccessEntity access)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(access);
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
                        return connection.Table<OAuthAccessEntity>().Count();
                    }
                }
            }
        }
    }
}


