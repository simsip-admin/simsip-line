using Simsip.LineRunner.Entities.Scoreoid;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public interface IPlayerRepository
    {
        void Create(PlayerEntity player);
        PlayerEntity GetPlayer();
        PlayerEntity GetPlayer(int playerId);
        void Update(PlayerEntity player);
        void Delete(PlayerEntity player);
        int Count { get; }
    }
}
