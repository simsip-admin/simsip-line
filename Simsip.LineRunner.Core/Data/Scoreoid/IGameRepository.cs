using Simsip.LineRunner.Entities.Scoreoid;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public interface IGameRepository
    {
        void Create(GameEntity game);
        GameEntity GetGame(string gameId);
        void Update(GameEntity game);
        void Delete(GameEntity game);
        int Count { get; }
    }
}
