using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.InApp;

namespace Simsip.LineRunner.Data.InApp
{
    public class InAppPurchaseRepository : IInAppPurchaseRepository
    {
        public string PracticeModeProductId { get { return "com.simsip.linerunner.practicemode";  } }

        public void Create(InAppPurchaseEntity purchase)
        {
            // Make sure we have required object/fields
            if (purchase == null)
            {
                throw new ArgumentException("InAppPurchaseEntity parameter cannot be null");
            }

            // Make sure this is not a duplicate
            var existingPurchase = GetPurchaseByOrderId(purchase.OrderId);
            if (existingPurchase != null)
            {
                throw new InvalidOperationException("Cannot create duplicate purchase. There is an existing purchase with this OrderId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(purchase);
                        });
                }
            }
        }

        public InAppPurchaseEntity GetPurchaseByOrderId(string orderId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<InAppPurchaseEntity>()
                                 .Where(x => x.OrderId == orderId))
                                 .FirstOrDefault<InAppPurchaseEntity>();

                    return result;
                }
            }
        }

        public InAppPurchaseEntity GetPurchaseByProductId(string productId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<InAppPurchaseEntity>()
                                 .Where(x => x.ProductId == productId))
                                 .FirstOrDefault<InAppPurchaseEntity>();

                    return result;
                }
            }
        }

        public IList<InAppPurchaseEntity> GetAllPurchases()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<InAppPurchaseEntity>())
                                 .ToList<InAppPurchaseEntity>();

                    return result;
                }
            }
        }

        public void Update(InAppPurchaseEntity purchase)
        {
             // Make sure we have required object/fields
            if (purchase == null)
            {
                throw new ArgumentException("InAppPurchaseEntity parameter cannot be null");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(purchase);
                        });
                }
            }
        }

        public void Delete(InAppPurchaseEntity purchase)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(purchase);
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
                        return connection.Table<InAppPurchaseEntity>().Count();
                    }
                }
            }
        }
    }
}


