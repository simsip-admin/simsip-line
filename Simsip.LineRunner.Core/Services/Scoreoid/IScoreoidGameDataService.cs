using Simsip.LineRunner.Entities.Scoreoid;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Simsip.LineRunner.Services.Scoreoid
{
    public interface IScoreoidGameDataService
    {
        Task<GameDataEntity> GetGameDataAsync(string key);

        Task SetGameDataAsync(GameDataEntity gameData);

        Task UnsetGameDataAsync(GameDataEntity gameData);
    }
}
