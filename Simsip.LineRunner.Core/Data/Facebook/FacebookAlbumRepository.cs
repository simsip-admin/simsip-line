using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookAlbumRepository : IFacebookAlbumRepository
    {
        public void Create(FacebookAlbumEntity album)
        {
            // Make sure we have required object/fields
            if (album == null)
            {
                throw new ArgumentException("AlbumModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(album.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(album.FromId))
            {
                throw new ArgumentException("FromId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingAlbum = GetAlbumByServerId(album.ServerId);
            if (existingAlbum != null)
            {
                throw new InvalidOperationException("Cannot create duplicate album. There is an existing album with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(album);
                        });
                }
            }
        }

        public List<FacebookAlbumEntity> GetAlbums(int skip = -1, int pageSize = -1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookAlbumEntity>()
                                  .OrderBy(x => x.UpdatedTime);

                    if (skip != -1)
                    {
                        results.Skip(skip);
                    }

                    if (pageSize != -1)
                    {
                        results.Take(pageSize);
                    }

                    return results.ToList();
                }
            }
        }

        public FacebookAlbumEntity GetAlbumByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<FacebookAlbumEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<FacebookAlbumEntity>();

                    return result;
                }
            }
        }

        public List<FacebookAlbumEntity> ReadByUser(FacebookUserEntity user, int skip=-1, int pageSize=-1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookAlbumEntity>()
                                  .Where(x => x.FromId == user.ServerId)
                                  .OrderBy(x => x.UpdatedTime);

                    if (skip != -1)
                    {
                        results.Skip(skip);
                    }

                    if (pageSize != -1)
                    {
                        results.Take(pageSize);
                    }

                    return results.ToList();
                }
            }
        }

        public void Update(FacebookAlbumEntity album)
        {
            // Make sure we have required object/fields
            if (album == null)
            {
                throw new ArgumentException("AlbumModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(album.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(album.FromId))
            {
                throw new ArgumentException("FromId cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(album);
                        });
                }
            }
        }

        public void Delete(FacebookAlbumEntity album)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(album);
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
                        return connection.Table<FacebookAlbumEntity>().Count();
                    }
                }
            }
        }
    }
}


