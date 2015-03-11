using Simsip.LineRunner.Entities.Youtube;

namespace Simsip.LineRunner.Data.Youtube
{
    public interface IYoutubeProfileRepository
    {
        void Create(YoutubeProfileEntity profile);
        YoutubeProfileEntity ReadByServerId(string serverId);
        YoutubeProfileEntity ReadProfile();
        void Update(YoutubeProfileEntity profile);
        void Delete(YoutubeProfileEntity profile);
        int Count { get; }
    }
}
