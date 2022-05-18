namespace EnhancedStreamChat.Interfaces
{
    public interface IIrcServiceDisconnectReceiver
    {
        void OnDisconnect(object ircService);
    }
}
