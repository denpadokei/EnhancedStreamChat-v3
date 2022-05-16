using System;
using System.Collections.Generic;
using System.Text;

namespace EnhancedStreamChat.Interfaces
{
    public interface IIrcServiceDisconnectReceiver
    {
        void OnDisconnect(object ircService);
    }
}
