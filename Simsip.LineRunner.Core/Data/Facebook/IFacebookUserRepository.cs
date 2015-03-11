using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookUserRepository
    {
        void Create(FacebookUserEntity user);
        FacebookUserEntity ReadByServerId(string serverId);
        FacebookUserEntity ReadUser();
        void Update(FacebookUserEntity user);
        void Delete(FacebookUserEntity user);
        int Count { get; }
    }
}
