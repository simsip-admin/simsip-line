using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookPhotoRepository : IFacebookPhotoRepository
    {
        public void Create(FacebookPhotoEntity photo)
        {
            // Make sure we have required object/fields
            if (photo == null)
            {
                throw new ArgumentException("FacebookPhotoEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(photo.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(photo.AlbumId))
            {
                throw new ArgumentException("AlbumId cannot be empty");
            }
            if (string.IsNullOrEmpty(photo.FromId))
            {
                throw new ArgumentException("FromId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingPhoto = GetPhotoByServerId(photo.ServerId);
            if (existingPhoto != null)
            {
                throw new InvalidOperationException("Cannot create duplicate photo. There is an existing photo with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(photo);
                        });
                }
            }
        }

        public FacebookPhotoEntity GetPhotoByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<FacebookPhotoEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<FacebookPhotoEntity>();

                    return result;
                }
            }
        }

        public List<FacebookPhotoEntity> GetPhotosByAlbumId(string albumId, int skip=-1, int pageSize=-1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookPhotoEntity>()
                                  .Where(x => x.AlbumId == albumId)
                                  .OrderBy(x => x.Position);

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

        public void Update(FacebookPhotoEntity photo)
        {
             // Make sure we have required object/fields
            if (photo == null)
            {
                throw new ArgumentException("PhotoModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(photo.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(photo.AlbumId))
            {
                throw new ArgumentException("AlbumId cannot be empty");
            }
            if (string.IsNullOrEmpty(photo.FromId))
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
                            connection.Update(photo);
                        });
                }
            }
        }

        public void Delete(FacebookPhotoEntity photo)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(photo);
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
                        return connection.Table<FacebookPhotoEntity>().Count();
                    }
                }
            }
        }
    }
}


