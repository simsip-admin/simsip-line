using Simsip.LineRunner.Entities.Groupme;
using System.Collections.Generic;

namespace Simsip.LineRunner.Data.Groupme
{
    public interface IGroupmeMessageAttachmentRepository
    {
        void Create(GroupmeMessageAttachmentEntity attachment);
        GroupmeMessageAttachmentEntity ReadByServerId(string serverId);
        List<GroupmeMessageAttachmentEntity> ReadByMessageId(string messageId);
        void Update(GroupmeMessageAttachmentEntity attachment);
        void Delete(GroupmeMessageAttachmentEntity attachment);
        int Count { get; }
    }
}
