using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IRandomObstaclesRepository
    {
        List<RandomObstaclesEntity> GetAllRandomObstacles(string productId);

        List<RandomObstaclesEntity> GetRandomObstacleSet(string productId, string randomObstacleSet);
    }
}
