using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Entities.Youtube;

namespace Simsip.LineRunner.Services.Youtube
{
    interface IYoutubeProfileService
    {
        #region Profile

        /// <summary>
        /// Get the current user's youtube profile.
        /// 
        /// Note, this allows looking up a user's youtube profile
        /// based solely on that you only have a youtube access token.
        /// </summary>
        /// <param name="simsipUser">The simsip user model containing the access token
        /// you wnat to look up a profile for.</param>
        /// <returns>The youtube profile model.</returns>
        Task<YoutubeProfileEntity> GetProfile();

        /// <summary>
        /// Get the youtube profile for the youtube user based on
        /// the simsip user model passed in.
        /// 
        /// Note, this is a lookup via the youtube user id embedded
        /// in the simsip user model as opposed to just using the 
        /// access token in GetProfile.
        /// </summary>
        /// <param name="simsipUser">The simsip user model containing the youtube user id
        /// you want to look up a profile for.</param>
        /// <returns>The youtube profile mdoel.</returns>
        Task<YoutubeProfileEntity> GetProfileByUserId();

        #endregion

        #region Videos

        Task<IList<YoutubeVideoEntity>> GetVideos(YoutubeProfileEntity profile);

        #endregion

    }
}
