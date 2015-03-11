using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookAlbumRepository
    {
        void Create(FacebookAlbumEntity album);
        List<FacebookAlbumEntity> GetAlbums(int skip, int pageSize);
        FacebookAlbumEntity GetAlbumByServerId(string serverId);
        List<FacebookAlbumEntity> ReadByUser(FacebookUserEntity user, int skip, int pageSize);
        void Update(FacebookAlbumEntity album);
        void Delete(FacebookAlbumEntity album);
        int Count { get; }
    }
}
