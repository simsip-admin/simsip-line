using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Services.Groupme
{
    public interface IGroupmeGroupService
    {
        Task<IList<GroupmeGroupEntity>> StartIndexAsync();

        Task<GroupmeGroupEntity> StartShowAsync(string groupId);

    }
}