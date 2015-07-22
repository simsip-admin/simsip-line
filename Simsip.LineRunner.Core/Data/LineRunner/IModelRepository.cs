using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IModelRepository
    {
        ModelEntity GetModel(string productId, string modelName);
    }
}
