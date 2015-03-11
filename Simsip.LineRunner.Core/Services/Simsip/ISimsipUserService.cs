using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Simsip
{
    public interface ISimsipUserService
    {
        Task<SimsipUserEntity> AddUserAsync(SimsipUserEntity user);

        Task<SimsipUserEntity> GetUserAsync(FacebookUserEntity facebookUser);

        Task<SimsipUserEntity> UpdateUserAsync(SimsipUserEntity user);

        Task DeleteUserAsync(SimsipUserEntity user);
    }
}
