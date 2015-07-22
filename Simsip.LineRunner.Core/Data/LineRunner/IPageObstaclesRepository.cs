using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPageObstaclesRepository
    {
        List<PageObstaclesEntity> GetObstacles(string productId);
        List<PageObstaclesEntity> GetObstacles(string productId, int pageNumber);
        List<PageObstaclesEntity> GetObstacles(string productId, int pageNumber, int lineNumber);
        List<PageObstaclesEntity> GetObstacles(string productId, int pageNumber, int[] lineNumbers);

        PageObstaclesEntity GetObstacle(string productId, int pageNumber, int lineNumber, int obstacleNumber);
    }
}
