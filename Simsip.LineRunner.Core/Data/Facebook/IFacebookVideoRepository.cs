using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookVideoRepository
    {
        void Create(FacebookVideoEntity video);
        IList<FacebookVideoEntity> GetVideos(int skip, int pageSize);
        FacebookVideoEntity GetVideoByServerId(string serverId);
        IList<FacebookVideoEntity> GetVideosByUser(FacebookUserEntity user, int skip, int pageSize);
        void Update(FacebookVideoEntity video);
        void Delete(FacebookVideoEntity video);
        int Count { get; }
    }
}
