namespace EnhancedStreamChat.Interfaces
{
    public interface IESCChatUser
    {
        string Id
        {
            get;
        }

        string UserName
        {
            get;
        }

        string DisplayName
        {
            get;
        }

        string Color
        {
            get;
        }

        bool IsBroadcaster
        {
            get;
        }

        bool IsModerator
        {
            get;
        }
    }
}
