using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookScoreRepository : IFacebookScoreRepository
    {
        public void Create(FacebookScoreEntity score)
        {
            // Make sure we have required object/fields
            if (score == null)
            {
                throw new ArgumentException("FacebookScoreEntity parameter cannot be null");
            }
            if (score.Score <= 0)
            {
                throw new ArgumentException("FacebookScoreEntity Score parameter must be greater than 0");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(score);
                        });
                }
            }
        }

        public List<FacebookScoreEntity> GetPlayersScores(int skip = -1, int pageSize = -1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookScoreEntity>()
                                  .OrderByDescending(x => x.Score)
                                  .ThenBy(x => x.ScoreTime);

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

        public FacebookScoreEntity GetTopScoreForPlayer()
        {
            try
            {
                lock (Database.DATABASE_LOCK)
                {
                    using (var connection = new SQLiteConnection(Database.DatabasePath()))
                    {
                        var result = connection.Table<FacebookScoreEntity>()
                                      .OrderByDescending(x => x.Score)
                                      .ThenBy(x => x.ScoreTime)
                                      .FirstOrDefault();

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in GetTopScoreForPlayer: " + ex);
                return null;
            }
        }

        public List<FacebookScoreEntity> GetTopScoresForPlayer(int count)
        {
            try
            {
                lock (Database.DATABASE_LOCK)
                {
                    using (var connection = new SQLiteConnection(Database.DatabasePath()))
                    {
                        var results = connection.Table<FacebookScoreEntity>()
                                      .OrderByDescending(x => x.Score)
                                      .ThenBy(x => x.ScoreTime)
                                      .Take(count);

                        return results.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in GetTopScoresForPlayer: " + ex);
                return null;
            }
        }

        public FacebookScoreEntity GetScoreForUserAndAppId(string userId, string appId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<FacebookScoreEntity>()
                                  .Where( x => x.PlayerId == userId &&
                                               x.AppId == appId)
                                  .FirstOrDefault();

                    return result;
                }
            }

        }

        public void Update(FacebookScoreEntity score)
        {
            // Make sure we have required object/fields
            if (score == null)
            {
                throw new ArgumentException("FacebookScoreEntity parameter cannot be null");
            }
            if (score.Score <= 0)
            {
                throw new ArgumentException("FacebookScoreEntity Score parameter must be greater than 0");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(score);
                        });
                }
            }
        }

        public void Delete(FacebookScoreEntity score)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(score);
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
                        return connection.Table<FacebookScoreEntity>().Count();
                    }
                }
            }
        }
    }
}


