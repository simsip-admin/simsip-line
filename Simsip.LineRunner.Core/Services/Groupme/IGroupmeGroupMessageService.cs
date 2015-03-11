using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Services.Groupme
{
    public interface IGroupmeGroupMessageService
    {
        Task<IList<GroupmeGroupMessageEntity>> IndexMessagesAsync(string groupId, 
                                                                 string beforeId);
        Task CreateMessageAsync(GroupmeGroupMessageEntity message);
    }
}