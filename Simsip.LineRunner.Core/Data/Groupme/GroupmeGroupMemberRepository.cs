using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Data.Groupme
{
    public class GroupmeGroupMemberRepository : IGroupmeGroupMemberRepository
    {
        public void Create(GroupmeGroupMemberEntity member)
        {
            // Make sure we have required object/fields
            if (member == null)
            {
                throw new ArgumentException("GroupMemberModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(member.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(member.GroupId))
            {
                throw new ArgumentException("GroupId cannot be empty");
            }
            if (string.IsNullOrEmpty(member.UserId))
            {
                throw new ArgumentException("UserId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingMember = ReadByServerId(member.ServerId);
            if (existingMember != null)
            {
                throw new InvalidOperationException("Cannot create duplicate member. There is an existing member with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(member);
                        });
                }
            }
        }

        public List<GroupmeGroupMemberEntity> ReadByGroupId(string groupId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    return connection.Table<GroupmeGroupMemberEntity>()
                                      .Where(x => x.GroupId == groupId)
                                      .OrderBy(x => x.Nickname)
                                      .ToList();
                }
            }
        }

        public GroupmeGroupMemberEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    return connection.Table<GroupmeGroupMemberEntity>()
                                        .Where(x => x.ServerId == serverId)
                                        .FirstOrDefault();
                }
            }
        }

        public void Update(GroupmeGroupMemberEntity member)
        {
            // Make sure we have required object/fields
            if (member == null)
            {
                throw new ArgumentException("GroupMemberModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(member.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(member.GroupId))
            {
                throw new ArgumentException("GroupId cannot be empty");
            }
            if (string.IsNullOrEmpty(member.UserId))
            {
                throw new ArgumentException("UserId cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(member);
                        });
                }
            }
        }

        public void Delete(GroupmeGroupMemberEntity member)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(member);
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
                        return connection.Table<GroupmeGroupMemberEntity>().Count();
                    }
                }
            }
        }
    }
}


