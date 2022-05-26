namespace EnhancedStreamChat.Interfaces
{
    public interface IPubSubServiceDisconnectReceiver
    {
        void OnPubsubDisconnect(object pubSubService);
    }
}
