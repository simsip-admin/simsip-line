using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface ITextureRepository
    {
        IList<TextureEntity> GetTextures(string productId, string modelName);
    }
}
