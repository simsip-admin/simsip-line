using System.Collections.Generic;
using Simsip.LineRunner.Entities.Upgrade;

namespace Simsip.LineRunner.Data.Upgrade
{
    public interface IV2_PageObstaclesRepository
    {
        List<V2_PageObstaclesEntity> GetObstacles();
    }
}
