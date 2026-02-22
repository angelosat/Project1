using System;

namespace Project1.Core.Components
{
    [Obsolete]
    public class Message
    {
        public enum Types
        {
            ChatPlayer,
            ItemGot,
            EntityHitCeiling,
            OrderParametersChanged,
            ServerResponseReceived,
            ChunksLoaded,
            ServerNoResponse,
            SelectedChanged,
            ContentsChanged,
            SkillIncrease,
            ZoneDesignation,
            ShopsUpdated,
            ShopUpdated,
            TavernMenuChanged,
            OrderDeleted,
        }
    }
}
