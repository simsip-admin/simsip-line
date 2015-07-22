using System.Collections.Generic;
using Simsip.LineRunner.Entities.LineRunner;

namespace Simsip.LineRunner.Data.LineRunner
{
    public interface IPageCharactersRepository
    {
        List<PageCharactersEntity> GetCharacters(string productId, int pageNumber);
        List<PageCharactersEntity> GetCharacters(string productId, int pageNumber, int lineNumber);
        List<PageCharactersEntity> GetCharacters(string productId, int pageNumber, int[] lineNumbers);
        PageCharactersEntity GetCharacter(string productId, int pageNumber, int lineNumber, int characterNumber);
    }
}
