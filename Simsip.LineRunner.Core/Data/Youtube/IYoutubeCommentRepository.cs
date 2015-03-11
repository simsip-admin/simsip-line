using System.Collections.Generic;
using Simsip.LineRunner.Entities.Youtube;

namespace Simsip.LineRunner.Data.Youtube
{
    public interface IYoutubeCommentRepository
    {
        void Create(YoutubeCommentEntity comment);
        YoutubeCommentEntity ReadByServerId(string serverId);
        List<YoutubeCommentEntity> ReadByForeignKey(string foreignKey, int skip, int pageSize);
        void Update(YoutubeCommentEntity comment);
        void Delete(YoutubeCommentEntity comment);
        int Count { get; }
    }
}
