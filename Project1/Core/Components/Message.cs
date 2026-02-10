using System;
using Project1.Core.Entities;
using Project1.Core.Legacy;

namespace Project1.Core.Components
{
    public class Message
    {
        public enum Types
        {
            Attacked,
            SlotInteraction,
            HitGround,
            ChatPlayer,
            EntityCollision,
            BlockEntityAdded,
            BlockEntityRemoved,
            ItemGot,
            EntityDespawned,
            EntitySpawned,
            NpcsUpdated,
            AILogUpdated,
            EntityHitCeiling,
            EntityFootStep,
            MiningDesignation,
            ObjectDisposed,
            OrderParametersChanged,
            PlantReady,
            EntityAttacked,
            ItemOwnerChanged,
            ServerResponseReceived,
            ChunksLoaded,
            ServerNoResponse,
            PlayerControlNpc,
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
