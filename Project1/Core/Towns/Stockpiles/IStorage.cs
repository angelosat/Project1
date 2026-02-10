using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Legacy.Storage;
using Project1.Core.Materials;
using Project1.Core.Simulation;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.Stockpiles
{
    public interface IStorageNew
    {
        StorageSettings Settings { get; }
        void FiltersGuiCallback(ItemDef item, MaterialDef material);
        void FiltersGuiCallback(ItemDef item, Def variation);
        void FiltersGuiCallback(ItemCategory category);
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
