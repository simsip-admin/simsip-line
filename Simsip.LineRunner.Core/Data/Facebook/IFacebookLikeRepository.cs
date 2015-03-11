using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookLikeRepository
    {
        void Create(FacebookLikeEntity like);
        FacebookLikeEntity ReadByServerId(string serverId);
        List<FacebookLikeEntity> ReadByForeignKey(string foreignKey, int skip, int pageSize);
        void Update(FacebookLikeEntity like);
        void Delete(FacebookLikeEntity like);
        int Count { get; }
    }
}
