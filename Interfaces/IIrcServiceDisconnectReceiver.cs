namespace EnhancedStreamChat.Interfaces
{
    public interface IIrcServiceDisconnectReceiver
    {
        void OnIrcDisconnect(object ircService);
    }
}
