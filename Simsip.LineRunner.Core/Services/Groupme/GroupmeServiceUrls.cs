namespace Simsip.LineRunner.Services.Groupme
{
    class GroupmeServiceUrls
    {
        public const string BaseUrl = "https://api.groupme.com/v3";
        public const string GroupIndex = BaseUrl + "/groups";
        public const string GroupShow = BaseUrl + "/groups/{0}";
        public const string GroupMessageIndex = BaseUrl + "/groups/{0}/messages";
    }
}
