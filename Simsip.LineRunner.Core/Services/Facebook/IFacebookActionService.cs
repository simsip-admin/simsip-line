using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Facebook
{
    /// <summary>
    /// Reference: https://developers.facebook.com/docs/opengraph/using-actions/
    /// </summary>
    public interface IFacebookActionService
    {
        #region Actions

        Task<String> AddAction(FacebookUserEntity user,
                               FacebookActionEntity action);
        Task<IList<FacebookActionEntity>> GetActions(FacebookUserEntity user);
        
        Task UpdateAction(FacebookActionEntity album); 

        Task DeleteAction(FacebookActionEntity album);

        #endregion
    }
}
