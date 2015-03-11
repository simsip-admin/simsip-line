using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Data.Simsip
{
    public interface ISimsipUserRepository
    {
        void Create(SimsipUserEntity user);
        SimsipUserEntity ReadUser();
        void Update(SimsipUserEntity user);
        void Delete(SimsipUserEntity user);
        int Count { get; }
    }
}
