using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.InApp;

namespace Simsip.LineRunner.Data.InApp
{
    public class InAppSkuRepository : IInAppSkuRepository
    {
        public void Create(InAppSkuEntity sku)
        {
            // Make sure we have required object/fields
            if (sku == null)
            {
                throw new ArgumentException("InAppSkuEntity parameter cannot be null");
            }

            // Make sure this is not a duplicate
            var existingSku = GetSkuByProductId(sku.ProductId);
            if (existingSku != null)
            {
                throw new InvalidOperationException("Cannot create duplicate sku. There is an existing sku with this ProductId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(sku);
                        });
                }
            }
        }

        public InAppSkuEntity GetSkuByProductId(string productId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<InAppSkuEntity>()
                                 .Where(x => x.ProductId == productId))
                                 .FirstOrDefault<InAppSkuEntity>();

                    return result;
                }
            }
        }

        public IList<InAppSkuEntity> GetAllSkus()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<InAppSkuEntity>())
                                 .ToList<InAppSkuEntity>();

                    return result;
                }
            }
        }

        public void Update(InAppSkuEntity sku)
        {
             // Make sure we have required object/fields
            if (sku == null)
            {
                throw new ArgumentException("InAppSkuEntity parameter cannot be null");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(sku);
                        });
                }
            }

        }

        public void Delete(InAppSkuEntity sku)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(sku);
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
                        return connection.Table<InAppSkuEntity>().Count();
                    }
                }
            }
        }
    }
}


