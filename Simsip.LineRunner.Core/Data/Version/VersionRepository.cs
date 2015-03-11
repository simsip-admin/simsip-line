using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Version;

namespace Simsip.LineRunner.Data.Version
{
    public class VersionRepository : IVersionRepository
    {

        public VersionEntity GetVersion()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<VersionEntity>()
                                  .FirstOrDefault();

                    return result;
                }
            }
        }

        public void Update(VersionEntity version)
        {
            // Make sure we have required object/fields
            if (version == null)
            {
                throw new ArgumentException("VersionEntity parameter cannot be null");
            }
            if (version.VersionId != 1)
            {
                throw new ArgumentException("VersionId must be 1");
            }
            if (version.Version <= 1)
            {
                throw new ArgumentException("Version must be greater than 1");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(version);
                        });
                }
            }
        }
    }
}


