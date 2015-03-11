using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Simsip.LineRunner.Data.Groupme;
using Simsip.LineRunner.Entities.Groupme;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;

namespace Simsip.LineRunner.Services.Groupme
{
    public class GroupmeGroupService : IGroupmeGroupService
    {
        private const string TRACE_TAG = "GroupmeGroupService";

        private readonly IRestService _restService;
        private readonly IGroupemeIGroupRepository _groupRepository;

        private const string GROUP_TYPE_PRIVATE = "private";

        private const string DEFAULT_GROUP_IMAGE_URL =
            "/Simsip.Phone;component/Assets/Avatars/Groups/default_group_avatar.png";

        public GroupmeGroupService(IRestService restService, 
                                   IGroupemeIGroupRepository groupRepository)
        {
            _restService = restService;
            _groupRepository = groupRepository;
        }

        #region IGroupService implementation

        public async Task<IList<GroupmeGroupEntity>> StartIndexAsync()
        {
            var returnGroups = new List<GroupmeGroupEntity>();

            try
            {
                var response = await _restService.MakeRequestAsync(GroupmeServiceUrls.GroupIndex,
                                        RestVerbs.Get,
                                        OAuthType.Groupme);

                // TODO: Analyze http status code here before parsing

                returnGroups = ParseIndexResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting groupme groups: " + ex);
            }

            return returnGroups;
        }

        public async Task<GroupmeGroupEntity> StartShowAsync(string groupId)
        {
            GroupmeGroupEntity returnGroup = null;

            try
            {
                var url = string.Format(GroupmeServiceUrls.GroupShow, groupId);

                var response = await _restService.MakeRequestAsync(url,
                                        RestVerbs.Get,
                                        OAuthType.Groupme);

                // TODO: Analyze http status code here before parsing

                returnGroup = ParseShowResponse(response);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception getting groupme group: " + ex);
            }

            return returnGroup;
        }

        #endregion

        #region Helpers

        private List<GroupmeGroupEntity> ParseIndexResponse(string response)
        {
            var returnList = new List<GroupmeGroupEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedGroups = parsedResponse["response"];

                int groupCount = parsedGroups.Count();
                for (int i = 0; i < groupCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedGroup = (JObject)parsedGroups[i];

                        // 
                        bool isNew = false;
                        var serverId = parsedGroup["id"].Value<string>();
                        var groupModel = _groupRepository.ReadByServerId(serverId);
                        if (groupModel == null)
                        {
                            isNew = true;
                            groupModel = new GroupmeGroupEntity();
                        }

                        // Grab the simple values first
                        groupModel.ServerId = parsedGroup["id"].Value<string>();
                        groupModel.Name = parsedGroup["name"].Value<string>();
                        groupModel.Description = parsedGroup["description"].Value<string>();
                        groupModel.CreatorUserId = parsedGroup["creator_user_id"].Value<string>();
                        /* TODO
                        groupModel.CreatedAt = DateTimeUtil.UnixEpoch.AddSeconds(parsedGroup["created_at"].Value<long>());
                        groupModel.UpdatedAt = DateTimeUtil.UnixEpoch.AddSeconds(parsedGroup["updated_at"].Value<long>());
                        */
                        groupModel.ShareUrl = parsedGroup["share_url"].Value<string>();
                        groupModel.MessageCount = parsedGroup["messages"]["count"].Value<int>();
                        groupModel.LastMessageId = parsedGroup["messages"]["last_message_id"].Value<string>();
                        /* TOD
                        groupModel.LastMessageCreatedAt = DateTimeUtil.UnixEpoch.AddSeconds(parsedGroup["messages"]["last_message_created_at"].Value<long>());
                        */
                        groupModel.MessagePreviewNickname = parsedGroup["messages"]["preview"]["nickname"].Value<string>();
                        groupModel.MessagePreviewText = parsedGroup["messages"]["preview"]["text"].Value<string>();
                        groupModel.MessagePreviewImageUrl = parsedGroup["messages"]["preview"]["image_url"].Value<string>();

                        // We only process private groups
                        var groupType = parsedGroup["type"].Value<string>();
                        if (groupType != GROUP_TYPE_PRIVATE)
                        {
                            // We only handle private groups
                            continue;
                        }
                        groupModel.Type = GroupType.Private;

                        // If we don't have an image for the group, assign a default one
                        groupModel.ImageUrl = parsedGroup["image_url"].Value<string>() ?? DEFAULT_GROUP_IMAGE_URL;

                        if (isNew)
                        {
                            _groupRepository.Create(groupModel);
                        }
                        else
                        {
                            _groupRepository.Update(groupModel);
                        }
                    
                        returnList.Add(groupModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual group: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing groups response: " + ex);
            }

            return returnList;
        }

        private GroupmeGroupEntity ParseShowResponse(string response)
        {
            var returnGroup = new GroupmeGroupEntity();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedGroup = parsedResponse["response"];

                bool isNew = false;
                var serverId = parsedGroup["id"].Value<string>();
                var groupModel = _groupRepository.ReadByServerId(serverId);
                if (groupModel == null)
                {
                    isNew = true;
                    groupModel = new GroupmeGroupEntity();
                }

                // Grab the simple values first
                groupModel.ServerId = parsedGroup["id"].Value<string>();
                groupModel.Name = parsedGroup["name"].Value<string>();
                groupModel.Description = parsedGroup["description"].Value<string>();
                groupModel.CreatorUserId = parsedGroup["creator_user_id"].Value<string>();
                /* TODO
                groupModel.CreatedAt = DateTimeUtil.UnixEpoch.AddSeconds(parsedGroup["created_at"].Value<long>());
                groupModel.UpdatedAt = DateTimeUtil.UnixEpoch.AddSeconds(parsedGroup["updated_at"].Value<long>());
                */
                groupModel.ShareUrl = parsedGroup["share_url"].Value<string>();
                groupModel.MessageCount = parsedGroup["messages"]["count"].Value<int>();
                groupModel.LastMessageId = parsedGroup["messages"]["last_message_id"].Value<string>();
                /* TODO
                groupModel.LastMessageCreatedAt = DateTimeUtil.UnixEpoch.AddSeconds(parsedGroup["messages"]["last_message_created_at"].Value<long>());
                */
                groupModel.MessagePreviewNickname = parsedGroup["messages"]["preview"]["nickname"].Value<string>();
                groupModel.MessagePreviewText = parsedGroup["messages"]["preview"]["text"].Value<string>();
                groupModel.MessagePreviewImageUrl = parsedGroup["messages"]["preview"]["image_url"].Value<string>();

                // We only process private groups
                /* TODO
                var groupType = parsedGroup["type"].Value<string>();
                groupModel.Type = groupType;
                */

                // If we don't have an image for the group, assign a default one
                groupModel.ImageUrl = parsedGroup["image_url"].Value<string>() ?? DEFAULT_GROUP_IMAGE_URL;

                if (isNew)
                {
                    _groupRepository.Create(groupModel);
                }
                else
                {
                    _groupRepository.Update(groupModel);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing group response: " + ex);
            }

            return returnGroup;
        }

        #endregion
    }
}
