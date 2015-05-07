using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPageObstaclesRepository
    {
        List<PageObstaclesEntity> GetObstacles();
        List<PageObstaclesEntity> GetObstacles(int pageNumber);
        List<PageObstaclesEntity> GetObstacles(int pageNumber, int lineNumber);
        List<PageObstaclesEntity> GetObstacles(int pageNumber, int[] lineNumbers);

        PageObstaclesEntity GetObstacle(int pageNumber, int lineNumber, int obstacleNumber);
    }
}
