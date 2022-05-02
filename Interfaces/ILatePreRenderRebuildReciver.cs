using System;

namespace EnhancedStreamChat.Interfaces
{
    public interface ILatePreRenderRebuildReciver
    {
        public void LatePreRenderRebuildHandler(object sender, EventArgs e);
    }
}
