using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPageCharactersRepository
    {
        List<PageCharactersEntity> GetCharacters(int pageNumber);
        List<PageCharactersEntity> GetCharacters(int pageNumber, int lineNumber);
        List<PageCharactersEntity> GetCharacters(int pageNumber, int[] lineNumbers);
        PageCharactersEntity GetCharacter(int pageNumber, int lineNumber, int characterNumber);
    }
}
