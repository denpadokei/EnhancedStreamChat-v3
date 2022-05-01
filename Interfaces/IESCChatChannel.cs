using System;
using System.Collections.Generic;
using System.Text;

namespace EnhancedStreamChat.Interfaces
{
    public interface IESCChatChannel
    {
        public string ID { get; }
        public string Name { get; }
        public void SendMessage(string message);
    }
}
