using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Data.Groupme
{
    public interface IGroupmeUserRepository
    {
        void Create(GroupmeUserEntity user);
        GroupmeUserEntity ReadByServerId(string serverId);
        GroupmeUserEntity ReadUser();
        void Update(GroupmeUserEntity user);
        void Delete(GroupmeUserEntity user);
        int Count { get; }
    }
}
