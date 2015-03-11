using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Facebook
{
    public interface IFacebookUserService
    {
        #region User

        Task<FacebookUserEntity> GetUser();

        #endregion
    }
}
