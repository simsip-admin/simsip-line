using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IRandomObstaclesRepository
    {
        List<RandomObstaclesEntity> GetAllRandomObstacles();

        List<RandomObstaclesEntity> GetRandomObstacleSet(string randomObstacleSet);
    }
}
