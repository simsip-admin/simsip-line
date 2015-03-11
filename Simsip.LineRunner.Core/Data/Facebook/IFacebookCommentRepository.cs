using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookCommentRepository
    {
        void Create(FacebookCommentEntity comment);
        FacebookCommentEntity ReadByServerId(string serverId);
        List<FacebookCommentEntity> ReadByForeignKey(string foreignKey, int skip, int pageSize);
        void Update(FacebookCommentEntity comment);
        void Delete(FacebookCommentEntity comment);
        int Count { get; }
    }
}
