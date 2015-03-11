using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Data.Groupme
{
    public class GroupmeMessageAttachmentRepository : IGroupmeMessageAttachmentRepository
    {

        public void Create(GroupmeMessageAttachmentEntity attachment)
        {
            // Make sure we have required object/fields
            if (attachment == null)
            {
                throw new ArgumentException("MessageAttachmentModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(attachment.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(attachment.MessageId))
            {
                throw new ArgumentException("MessageId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingAttachment = ReadByServerId(attachment.ServerId);
            if (existingAttachment != null)
            {
                throw new InvalidOperationException("Cannot create duplicate attachment. There is an existing attachment with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(attachment);
                        });
                }
            }
        }

        public GroupmeMessageAttachmentEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<GroupmeMessageAttachmentEntity>()
                                 .Where(x => x.ServerId == serverId)
                                 .FirstOrDefault();

                    return result;
                }
            }
        }

        public List<GroupmeMessageAttachmentEntity> ReadByMessageId(string messageId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<GroupmeMessageAttachmentEntity>()
                                  .Where(x => x.MessageId == messageId)
                                  .ToList();

                    return results;
                }
            }
        }

        public void Update(GroupmeMessageAttachmentEntity attachment)
        {
            // Make sure we have required object/fields
            if (attachment == null)
            {
                throw new ArgumentException("MessageAttachmentModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(attachment.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(attachment.MessageId))
            {
                throw new ArgumentException("MessageId cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(attachment);
                        });
                }
            }
        }

        public void Delete(GroupmeMessageAttachmentEntity attachment)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(attachment);
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
                        return connection.Table<GroupmeMessageAttachmentEntity>().Count();
                    }
                }
            }
        }
    }
}


