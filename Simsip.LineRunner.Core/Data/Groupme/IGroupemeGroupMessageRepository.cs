using Simsip.LineRunner.Entities.Groupme;
using System.Collections.Generic;

namespace Simsip.LineRunner.Data.Groupme
{
    public interface IGroupemeGroupMessageRepository
    {
        void Create(GroupmeGroupMessageEntity message);
        List<GroupmeGroupMessageEntity> ReadLatest(string groupId);
        GroupmeGroupMessageEntity ReadByServerId(string serverId);
        void Update(GroupmeGroupMessageEntity message);
        void Delete(GroupmeGroupMessageEntity message);
        int Count { get; }
    }
}
