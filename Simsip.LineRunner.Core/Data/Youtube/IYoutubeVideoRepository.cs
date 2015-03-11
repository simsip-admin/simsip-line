using System.Collections.Generic;
using Simsip.LineRunner.Entities.Youtube;

namespace Simsip.LineRunner.Data.Youtube
{
    public interface IYoutubeVideoRepository
    {
        void Create(YoutubeVideoEntity video);
        YoutubeVideoEntity ReadByServerId(string serverId);
        List<YoutubeVideoEntity> Read(int skip, int pageSize);
        void Update(YoutubeVideoEntity video);
        void Delete(YoutubeVideoEntity video);
        int Count { get; }
    }
}
