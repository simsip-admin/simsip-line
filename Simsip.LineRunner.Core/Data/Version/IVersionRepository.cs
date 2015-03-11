using System.Collections.Generic;
using Simsip.LineRunner.Entities.Version;

namespace Simsip.LineRunner.Data.Version
{
    public interface IVersionRepository
    {
        VersionEntity GetVersion();
        void Update(VersionEntity version);
    }
}
