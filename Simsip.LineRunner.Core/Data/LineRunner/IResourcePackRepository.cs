using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IResourcePackRepository
    {
        IList<ResourcePackEntity> GetResourcePacks();

        ResourcePackEntity GetResourcePack(string resourcePackName);
    }
}
