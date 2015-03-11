using Simsip.LineRunner.Entities.Scoreoid;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public interface IPlayerDataRepository
    {
        void Create(PlayerDataEntity playerData);
        PlayerDataEntity GetPlayerData(string key);
        void Update(PlayerDataEntity playerData);
        void Delete(PlayerDataEntity playerData);
        int Count { get; }
    }
}
