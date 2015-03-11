using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Data.Groupme
{
    public class GroupmeGroupMessageRepository : IGroupemeGroupMessageRepository
    {
        public void Create(GroupmeGroupMessageEntity message)
        {
            // Make sure we have required object/fields
            if (message == null)
            {
                throw new ArgumentException("GroupMessageModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(message.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(message.GroupId))
            {
                throw new ArgumentException("GroupId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingMessage = ReadByServerId(message.ServerId);
            if (existingMessage != null)
            {
                throw new InvalidOperationException("Cannot create duplicate message. There is an existing message with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(message);
                        });
                }
            }
        }

        public List<GroupmeGroupMessageEntity> ReadLatest(string groupId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {

                    var q = from message in connection.Table<GroupmeGroupMessageEntity>()
                            .Where(m => m.GroupId == groupId)
                            join attachment in connection.Table<GroupmeMessageAttachmentEntity>()
                            on message.ServerId equals attachment.MessageId into attachments
                            orderby message.CreatedAt
                            select new GroupmeGroupMessageEntity
                                {
                                    DeviceId = message.DeviceId,
                                    ServerId = message.ServerId,
                                    GroupId = message.GroupId,
                                    SourceGuid = message.SourceGuid,
                                    CreatedAt = message.CreatedAt,
                                    UserId = message.UserId,
                                    Name = message.Name,
                                    AvatarUrl = message.AvatarUrl,
                                    Text = message.Text,
                                    IsSystem = message.IsSystem,
                                    FavoritedBy = message.FavoritedBy,
                                    Attachments = attachments.ToList()
                                };

                    return q.Take(20).ToList();
                }
            }
        }

        public List<GroupmeGroupMessageEntity> ReadPrevious(string groupId, DateTime createdAt)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var q = from message in connection.Table<GroupmeGroupMessageEntity>()
                            .Where(m => m.GroupId == groupId && m.CreatedAt < createdAt)
                            join attachment in connection.Table<GroupmeMessageAttachmentEntity>()
                                on message.ServerId equals attachment.MessageId into attachments
                            orderby message.CreatedAt
                            select new GroupmeGroupMessageEntity
                                {
                                    DeviceId = message.DeviceId,
                                    ServerId = message.ServerId,
                                    GroupId = message.GroupId,
                                    SourceGuid = message.SourceGuid,
                                    CreatedAt = message.CreatedAt,
                                    UserId = message.UserId,
                                    Name = message.Name,
                                    AvatarUrl = message.AvatarUrl,
                                    Text = message.Text,
                                    IsSystem = message.IsSystem,
                                    FavoritedBy = message.FavoritedBy,
                                    Attachments = attachments.ToList()
                                };

                    return q.Take(20).ToList();
                }
            }
        }

        public GroupmeGroupMessageEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    return connection.Table<GroupmeGroupMessageEntity>()
                           .Where(x => x.ServerId == serverId)
                           .FirstOrDefault();
                }
            }

        }

        public void Update(GroupmeGroupMessageEntity message)
        {
            // Make sure we have required object/fields
            if (message == null)
            {
                throw new ArgumentException("GroupMessageModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(message.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(message.GroupId))
            {
                throw new ArgumentException("GroupId cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(message);
                        });
                }
            }
        }

        public void Delete(GroupmeGroupMessageEntity message)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(message);
                            foreach (var attachment in message.Attachments)
                            {
                                connection.Delete(attachment);
                            }
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
                        return connection.Table<GroupmeGroupMessageEntity>().Count();
                    }
                }
            }
        }
    }
}


