namespace EnhancedStreamChat.Interfaces
{
    public interface IESCChatChannel
    {
        public string ID { get; }
        public string Name { get; }
        public void SendMessage(string message);
    }
}
