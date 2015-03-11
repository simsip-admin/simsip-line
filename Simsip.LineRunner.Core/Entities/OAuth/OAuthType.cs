
namespace Simsip.LineRunner.Entities.OAuth
{
    /// <summary>
    /// IMPORTANT: Order cannot be changed as the enum is stored to the database as
    /// converted integer of enumeration.
    /// </summary>
    public enum OAuthType
    {
        None,
        Facebook,
        Groupme,
        Youtube
    }

}
