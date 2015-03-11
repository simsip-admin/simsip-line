using Simsip.LineRunner.Entities.OAuth;
using System.Collections.Generic;

namespace Simsip.LineRunner.Data.OAuth
{
    public interface IOAuthResultRepository
    {
        void Create(OAuthResultEntity result);
        OAuthResultEntity Read(OAuthType type);
        void Update(OAuthResultEntity result);
        void Delete(OAuthResultEntity result);
        int Count { get; }
    }
}
