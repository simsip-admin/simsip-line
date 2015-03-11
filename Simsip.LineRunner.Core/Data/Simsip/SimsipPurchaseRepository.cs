using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Data.Simsip
{
    public class SimsipPurchaseRepository : ISimsipPurchaseRepository
    {
        public void Create(SimsipPurchaseEntity purchase)
        {
            // Make sure we have required object/fields
            if (purchase == null)
            {
                throw new ArgumentException("SimsipPurchaseEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(purchase.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(purchase.ProductId))
            {
                throw new ArgumentException("ProductId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingPurchase = GetPurchaseByServerId(purchase.ServerId);
            if (existingPurchase != null)
            {
                throw new InvalidOperationException("Cannot create duplicate purchase. There is an existing purchase with this ServerId");
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

        public SimsipPurchaseEntity GetPurchaseByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<SimsipPurchaseEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<SimsipPurchaseEntity>();

                    return result;
                }
            }
        }

        public IList<SimsipPurchaseEntity> GetAllPurchases()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<SimsipPurchaseEntity>())
                                 .ToList<SimsipPurchaseEntity>();

                    return result;
                }
            }
        }

        public void Update(SimsipPurchaseEntity purchase)
        {
             // Make sure we have required object/fields
            if (purchase == null)
            {
                throw new ArgumentException("SimsipPurchaseEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(purchase.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(purchase.ProductId))
            {
                throw new ArgumentException("ProductId cannot be empty");
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

        public void Delete(SimsipPurchaseEntity purchase)
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
                        return connection.Table<SimsipPurchaseEntity>().Count();
                    }
                }
            }
        }
    }
}


