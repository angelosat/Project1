using Project1.Core.Entities;
using Project1.Core.Legacy;
using System;

namespace Project1.Core.Components
{
    public class Message
    {
        public enum Types
        {
            Attacked,
            HealthLost,
            SlotInteraction,
            HitGround,
            ChatPlayer,
            EntityCollision,
            BlockEntityAdded,
            BlockEntityRemoved,
            ItemGot,
            NotEnoughSpace,
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
            ItemLost,
            AttackTargetChanged
        }

        public GameObject Receiver;
        public ObjectEventArgs Args;
        public Action<GameObject> Callback;
        public Message(GameObject receiver, ObjectEventArgs e, Action<GameObject> callback = null)
        {
            this.Receiver = receiver;
            this.Args = e;
            this.Callback = callback;
        }
    }
}
