using System;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Youtube;

namespace Simsip.LineRunner.Data.Youtube
{
    public class YoutubeProfileRepository : IYoutubeProfileRepository
    {
        public void Create(YoutubeProfileEntity profile)
        {
            // Make sure we have required object/fields
            if (profile == null)
            {
                throw new ArgumentException("ProfileModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(profile.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingProfile = ReadByServerId(profile.ServerId);
            if (existingProfile != null)
            {
                throw new InvalidOperationException("Cannot create duplicate profile. There is an existing profile with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(profile);
                        });
                }
            }
        }

        public YoutubeProfileEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<YoutubeProfileEntity>()
                                 .Where(x => x.ServerId == serverId)
                                 .FirstOrDefault();

                    return result;
                }
            }
        }

        public YoutubeProfileEntity ReadProfile()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<YoutubeProfileEntity>()
                                 .FirstOrDefault<YoutubeProfileEntity>();

                    return result;
                }
            }
        }

        public void Update(YoutubeProfileEntity profile)
        {
            // Make sure we have required object/fields
            if (profile == null)
            {
                throw new ArgumentException("ProfileModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(profile.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(profile);
                        });
                }
            }
        }

        public void Delete(YoutubeProfileEntity profile)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(profile);
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
                        return connection.Table<YoutubeProfileEntity>().Count();
                    }
                }
            }
        }
    }
}


