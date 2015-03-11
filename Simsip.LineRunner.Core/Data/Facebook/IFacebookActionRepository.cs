using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookActionRepository
    {
        void Create(FacebookActionEntity action);
        IList<FacebookActionEntity> Read(int skip, int pageSize);
        FacebookActionEntity ReadByServerId(string serverId);
        void Update(FacebookActionEntity action);
        void Delete(FacebookActionEntity action);
        int Count { get; }
    }
}
