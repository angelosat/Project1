using System;

namespace Start_a_Town_.Components
{
    public class Message
    {
        public enum Types
        {
            Default,
            Death,
            InteractionInterrupted,
            OutOfRange,
            Attacked,
            HealthLost,
            SlotInteraction,
            HitGround,
            Jumped,
            ChatPlayer,
            EntityCollision,
            BlockEntityAdded,
            BlockEntityRemoved,
            //BlocksChanged,
            ItemGot,
            NotEnoughSpace,
            EntityDespawned,
            EntitySpawned,
            NpcsUpdated,
            AILogUpdated,
            EntityHitCeiling,
            EntityHitGround,
            EntityFootStep,
            MiningDesignation,
            ObjectDisposed,
            OrderParametersChanged,
            PlantHarvested,
            PlantReady,
            EntityAttacked,
            NeedUpdated,
            ItemOwnerChanged,
            PlayerConnected,
            PlayerDisconnected,
            ServerResponseReceived,
            ChunksLoaded,
            ServerNoResponse,
            PlayerControlNpc,
            SelectedChanged,
            ContentsChanged,
            SkillIncrease,
            ZoneDesignation,
            //ActorGearUpdated,
            //NewAdventurerCreated,
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
