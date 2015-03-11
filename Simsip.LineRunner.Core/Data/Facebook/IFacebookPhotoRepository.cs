using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookPhotoRepository
    {
        void Create(FacebookPhotoEntity photo);
        FacebookPhotoEntity GetPhotoByServerId(string serverId);
        List<FacebookPhotoEntity> GetPhotosByAlbumId(string albumId, int skip=-1, int pageSize=-1);
        void Update(FacebookPhotoEntity photo);
        void Delete(FacebookPhotoEntity photo);
        int Count { get; }
    }
}
