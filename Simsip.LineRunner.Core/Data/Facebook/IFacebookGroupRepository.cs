using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookGroupRepository
    {
        void Create(FacebookGroupEntity group);
        IList<FacebookGroupEntity> GetGroups(int skip, int pageSize);
        FacebookGroupEntity GetGroupByServerId(string serverId);
        void Update(FacebookGroupEntity group);
        void Delete(FacebookGroupEntity group);
        int Count { get; }
    }
}
