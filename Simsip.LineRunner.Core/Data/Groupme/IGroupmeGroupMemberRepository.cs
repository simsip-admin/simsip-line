using Simsip.LineRunner.Entities.Groupme;
using System.Collections.Generic;

namespace Simsip.LineRunner.Data.Groupme
{
    public interface IGroupmeGroupMemberRepository
    {
        void Create(GroupmeGroupMemberEntity member);
        GroupmeGroupMemberEntity ReadByServerId(string serverId);
        List<GroupmeGroupMemberEntity> ReadByGroupId(string groupId);
        void Update(GroupmeGroupMemberEntity member);
        void Delete(GroupmeGroupMemberEntity member);
        int Count { get; }
    }
}
