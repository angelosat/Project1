using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IStorageNew
    {
        StorageSettings Settings { get; }
        void FiltersGuiCallback(ItemDef item, MaterialDef material);
        void FiltersGuiCallback(ItemDef item, Def variation);
        void FiltersGuiCallback(ItemCategory category);
        Dictionary<TargetArgs, int> GetPotentialHaulTargets(Actor actor, GameObject item, out int maxamount);
        IEnumerable<TargetArgs> GetPotentialHaulTargets(Actor actor, GameObject item);
    }
    [Obsolete]
    public interface IStorage
    {
        MapBase Map { get; }
        int ID { get; }
        StorageSettings Settings { get; }
        bool Accepts(Entity item);
        Dictionary<TargetArgs, int> GetPotentialHaulTargets(Actor actor, GameObject item, out int maxamount);
        IEnumerable<TargetArgs> GetPotentialHaulTargets(Actor actor, GameObject item);
        bool IsValidStorage(Entity item, TargetArgs target, int quantity);
    }
}
