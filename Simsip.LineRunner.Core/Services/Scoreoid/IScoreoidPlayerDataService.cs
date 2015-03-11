using Simsip.LineRunner.Entities.Scoreoid;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Simsip.LineRunner.Services.Scoreoid
{
    public interface IScoreoidPlayerDataService
    {
        Task<PlayerDataEntity> GetPlayerDataAsync(string key);

        Task SetPlayerDataAsync(PlayerDataEntity playerData);

        Task UnsetGameDataAsync(PlayerDataEntity playerData);
    }
}
