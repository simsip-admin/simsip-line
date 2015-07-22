using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface ICharacterRepository
    {
        CharacterEntity GetCharacter(string productId, string modelName);
    }
}
