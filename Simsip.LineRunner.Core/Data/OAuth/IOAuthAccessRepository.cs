using Simsip.LineRunner.Entities.OAuth;
using System.Collections.Generic;

namespace Simsip.LineRunner.Data.OAuth
{
    public interface IOAuthAccessRepository
    {
        void Create(OAuthAccessEntity access);
        OAuthAccessEntity Read(OAuthType type);
        void Update(OAuthAccessEntity access);
        void Delete(OAuthAccessEntity access);
        int Count { get; }
    }
}
