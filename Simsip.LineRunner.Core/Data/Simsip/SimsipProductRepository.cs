using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Data.Simsip
{
    public class SimsipProductRepository : ISimsipProductRepository
    {
        public void Create(SimsipProductEntity product)
        {
            // Make sure we have required object/fields
            if (product == null)
            {
                throw new ArgumentException("SimsipProductEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(product.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(product.Name))
            {
                throw new ArgumentException("Name cannot be empty");
            }
            if (string.IsNullOrEmpty(product.Description))
            {
                throw new ArgumentException("Description cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingProduct = GetProductByServerId(product.ServerId);
            if (existingProduct != null)
            {
                throw new InvalidOperationException("Cannot create duplicate product. There is an existing product with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(product);
                        });
                }
            }
        }

        public SimsipProductEntity GetProductByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<SimsipProductEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<SimsipProductEntity>();

                    return result;
                }
            }

        }

        public IList<SimsipProductEntity> GetProducts()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<SimsipProductEntity>())
                                 .ToList<SimsipProductEntity>();

                    return result;
                }
            }
        }

        public void Update(SimsipProductEntity product)
        {
             // Make sure we have required object/fields
            if (product == null)
            {
                throw new ArgumentException("SimsipProductEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(product.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(product.Name))
            {
                throw new ArgumentException("Name cannot be empty");
            }
            if (string.IsNullOrEmpty(product.Description))
            {
                throw new ArgumentException("Description cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(product);
                        });
                }
            }

        }

        public void Delete(SimsipProductEntity product)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(product);
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
                        return connection.Table<SimsipProductEntity>().Count();
                    }
                }
            }
        }
    }
}


