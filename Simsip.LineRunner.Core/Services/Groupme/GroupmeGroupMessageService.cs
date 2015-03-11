using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Data.Groupme;
using Simsip.LineRunner.Entities.Groupme;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;

namespace Simsip.LineRunner.Services.Groupme
{
    public class GroupmeGroupMessageService : IGroupmeGroupMessageService
    {
        private const string TRACE_TAG = "GroupmeGroupMessageService";

        private const string DEFAULT_MESSAGE_AVATAR_URL =
            "/Simsip.Phone;component/Assets/Avatars/Users/default_user_avatar.png";

        private readonly IRestService _restService;
        private readonly IGroupemeGroupMessageRepository _groupMessageRepository;
        private readonly IGroupmeMessageAttachmentRepository _messageAttachmentRepository;


        #region Construction

        public GroupmeGroupMessageService(IRestService restService,
                                          IGroupemeGroupMessageRepository groupMessageRepository, 
                                          IGroupmeMessageAttachmentRepository messageAttachmentRepository)
        {
            _restService = restService;
            _groupMessageRepository = groupMessageRepository;
            _messageAttachmentRepository = messageAttachmentRepository;
        }

        #endregion

        #region IGroupMessageService implementation

        public async Task<IList<GroupmeGroupMessageEntity>> IndexMessagesAsync(string groupId, 
                                                                        string beforeId)
        {
            var returnMessages = new List<GroupmeGroupMessageEntity>();

            try
            {
                var url = string.Format(GroupmeServiceUrls.GroupMessageIndex, groupId);

                var response = await _restService.MakeRequestAsync(url,
                                        RestVerbs.Get,
                                        OAuthType.Groupme);

                // TODO: Analyze http status code here before parsing

                returnMessages = ParseIndex(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting groupme messages: " + ex);
            }

            return returnMessages;
        }


        public async Task CreateMessageAsync(GroupmeGroupMessageEntity message)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region Helper functions

        private void CreateAttachmentsAsync(GroupmeGroupMessageEntity message, Action success, Action<Exception> error)
        {
        }

        private List<GroupmeGroupMessageEntity> ParseIndex(string response)
        {
            var returnList = new List<GroupmeGroupMessageEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedMessages = parsedResponse["response"]["messages"];

                int messageCount = parsedMessages.Count();
                for (int i = 0; i < messageCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedMessage = (JObject) parsedMessages[i];

                        bool isNew = false;
                        var serverId = parsedMessage["id"].Value<string>();
                        var messageModel = _groupMessageRepository.ReadByServerId(serverId);
                        if (messageModel == null)
                        {
                            isNew = true;
                            messageModel = new GroupmeGroupMessageEntity();
                        }

                        // Grab the simple values first
                        messageModel.ServerId = parsedMessage["id"].Value<string>();
                        messageModel.GroupId = parsedMessage["group_id"].Value<string>();
                        messageModel.SourceGuid = parsedMessage["source_guid"].Value<string>();
                        /* TODO
                        messageModel.CreatedAt =
                            DateTimeUtil.UnixEpoch.AddSeconds(parsedMessage["created_at"].Value<long>());
                        */
                        messageModel.UserId = parsedMessage["user_id"].Value<string>();
                        messageModel.Name = parsedMessage["name"].Value<string>();
                        messageModel.Name = parsedMessage["text"].Value<string>();
                        messageModel.IsSystem = parsedMessage["system"].Value<bool>();

                        // Avatar url
                        // If we don't have an image for the group, assign a default one
                        messageModel.AvatarUrl = parsedMessage["avatar_url"].Value<string>() ??
                                                 DEFAULT_MESSAGE_AVATAR_URL;

                        // Favorited by
                        var parsedFavoritedBy = parsedMessage["favorited_by"];
                        if (parsedFavoritedBy != null)
                        {
                            var favoritedBy = new StringBuilder();
                            foreach (var favoritedById in parsedFavoritedBy)
                            {
                                favoritedBy.Append(favoritedById + ",");
                            }
                            messageModel.FavoritedBy = favoritedBy.ToString();
                            messageModel.FavoritedCount = parsedFavoritedBy.Count();

                        }
                        else
                        {
                            messageModel.FavoritedBy = string.Empty;
                            messageModel.FavoritedCount = 0;
                        }

                        // Loop over any attachments
                        var parsedAttachments = parsedMessage["attachments"];
                        if (parsedAttachments != null)
                        {
                            // First, determine if we have already recorded attachments for this message
                            var onDeviceAttachments = _messageAttachmentRepository.ReadByMessageId(messageModel.ServerId);

                            // Only add in attachments if we haven't done so yet
                            if (onDeviceAttachments.Count == 0)
                            {
                                // Loop over attachments from server
                                var attachmentTypeOrder = 0;
                                foreach (var parsedAttachment in parsedAttachments)
                                {
                                    // Only doing images for now
                                    var attachmentType = parsedAttachment["type"].Value<string>();
                                    if (attachmentType != MessageAttachmentType.Image)
                                    {
                                        continue;
                                    }

                                    // TODO: Attachment implementation is not used yet
                                    messageModel.PictureUrl = parsedAttachment["url"].Value<string>();

                                    // Build the model
                                    var attachmentModel = new GroupmeMessageAttachmentEntity
                                        {
                                            MessageId = messageModel.ServerId,
                                            Type = MessageAttachmentType.Image,
                                            Order = attachmentTypeOrder++,
                                            Url = parsedAttachment["url"].Value<string>()
                                        };

                                    // Save it to device db
                                    _messageAttachmentRepository.Create(attachmentModel);
                                }
                            }
                        }

                        if (isNew)
                        {
                            _groupMessageRepository.Create(messageModel);
                        }
                        else
                        {
                            _groupMessageRepository.Update(messageModel);
                        }
                    
                        returnList.Add(messageModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual message: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing messages response: " + ex);
            }

            return returnList;
        }

        #endregion
    }
}
