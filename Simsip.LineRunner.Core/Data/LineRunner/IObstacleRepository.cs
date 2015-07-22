using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IObstacleRepository
    {
        ObstacleEntity GetObstacle(string productId, string modelName);
    }
}
