using Simsip.LineRunner.Entities.Scoreoid;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public class ScoreRepository : IScoreRepository
    {
        public void Create(ScoreEntity score)
        {
            // Make sure we have required object/fields
            if (score == null)
            {
                throw new ArgumentException("Score parameter cannot be null");
            }

            // Make sure this is not a duplicate
            var existingScore = GetScore(score.ScoreId);
            if (existingScore != null)
            {
                throw new InvalidOperationException("Cannot create duplicate score. There is an existing score with this score ID.");
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

        public List<ScoreEntity> GetTopScores(int count)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<ScoreEntity>()
                                 .OrderByDescending(x => x.Score)
                                 .Take(count))
                                 .ToList<ScoreEntity>();

                    return result;
                }
            }
        }

        public ScoreEntity GetScore(int scoreId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<ScoreEntity>()
                                 .Where(x => x.ScoreId == scoreId))
                                 .FirstOrDefault<ScoreEntity>();

                    return result;
                }
            }
        }

        public ScoreEntity GetScoreByScore(int score)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<ScoreEntity>()
                                 .Where(x => x.Score == score))
                                 .FirstOrDefault<ScoreEntity>();

                    return result;
                }
            }
        }

        public int GetHighScore()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<ScoreEntity>()
                                .OrderBy(x => x.Score)
                                .FirstOrDefault<ScoreEntity>();

                    if (result != null)
                    {
                        return result.Score;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }


        public void Update(ScoreEntity score)
        {
            // Make sure we have required object/fields
            if (score == null)
            {
                throw new ArgumentException("Score parameter cannot be null");
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

        public void Delete(ScoreEntity score)
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
                        return connection.Table<ScoreEntity>().Count();
                    }
                }
            }
        }
    }
}

