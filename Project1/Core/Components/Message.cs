using System;

namespace Project1.Core.Components
{
    [Obsolete]
    public class Message
    {
        public enum Types
        {
            ServerNoResponse,
            ShopUpdated,
            TavernMenuChanged,
        }
    }
}
