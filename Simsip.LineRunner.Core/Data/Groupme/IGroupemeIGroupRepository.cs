using System.Collections.Generic;
using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Data.Groupme
{
    public interface IGroupemeIGroupRepository
    {
        void Create(GroupmeGroupEntity group);
        List<GroupmeGroupEntity> ReadInbox(int skip, int pageSize);
        GroupmeGroupEntity ReadByServerId(string serverId);
        void Update(GroupmeGroupEntity group);
        void Delete(GroupmeGroupEntity group);
        int Count { get; }
    }
}
