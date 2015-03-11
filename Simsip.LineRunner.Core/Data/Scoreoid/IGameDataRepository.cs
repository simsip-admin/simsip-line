using Simsip.LineRunner.Entities.Scoreoid;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public interface IGameDataRepository
    {
        void Create(GameDataEntity gameData);
        GameDataEntity GetGameData(string key);
        void Update(GameDataEntity gameData);
        void Delete(GameDataEntity gameData);
        int Count { get; }
    }
}
