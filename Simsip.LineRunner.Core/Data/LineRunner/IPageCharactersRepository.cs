using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPageCharactersRepository
    {
        List<PageCharactersEntity> GetCharacters(int pageNumber);
        List<PageCharactersEntity> GetCharacters(int pageNumber, int lineNumber);
        PageCharactersEntity GetCharacter(int pageNumber, int lineNumber, int characterNumber);
    }
}
