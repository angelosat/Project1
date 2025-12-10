using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
{

    public interface IItemPreferencesManager : ISaveable, ISerializableNew
    {
        Def GetPreference(Entity item);
        Entity GetPreference(Def context);
        Entity GetPreference(Def context, out int score);
        IEnumerable<Entity> GetJunk();
        void RemoveJunk(Entity entity);
        //bool AddPreference(Entity item);
        //void AddPreference(IItemPreferenceContext context, Entity item, int score);
        void RemovePreference(Def context);
        bool IsPreference(Entity item);
        Control Gui { get; }
        void ResolveReferences();
        int GetScore(Def context, Entity item);
        Control GetListControl(Entity entity);
        (Def context, int score) FindBestRole(Entity entity);
        IEnumerable<(Def context, int score)> Evaluate(Entity entity);
        IEnumerable<(Def context, Entity item, int score)> GetPotential();
        void OnSpawn(MapBase oldMap);
        void OnDespawn(MapBase oldMap);
        void OnMapLoaded();
        void ModifyBias(Entity entity, int value);
        void ForceDrop(Entity item);
        IEnumerable<(Def role, Entity item, int score)> GetPotentialAll();
    }


    //public interface IItemPreferenceContext
    //{ }

    //public interface IItemPreferencesManager : ISaveable, ISerializableNew
    //{
    //    //bool AddPreference(Entity item);
    //    void AddPreference(IItemPreferenceContext context, Entity item, int score);
    //    IEnumerable<(IItemPreferenceContext role, int score)> Evaluate(Entity entity);
    //    (IItemPreferenceContext role, int score) FindBestRole(Entity entity);
    //    void ForceDrop(Entity item);
    //    IEnumerable<Entity> GetJunk();
    //    Control GetListControl(Entity entity);
    //    IEnumerable<(IItemPreferenceContext role, Entity item, int score)> GetPotential();
    //    IEnumerable<(IItemPreferenceContext role, Entity item, int score)> GetPotentialAll();
    //    IItemPreferenceContext GetPreference(Entity item);
    //    Entity GetPreference(IItemPreferenceContext context);
    //    Entity GetPreference(IItemPreferenceContext context, out int score);
    //    int GetScore(IItemPreferenceContext context, Entity item);
    //    bool IsPreference(Entity item);
    //    void ModifyBias(Entity entity, int value);
    //    void OnDespawn(MapBase oldMap);
    //    void OnMapLoaded();
    //    void OnSpawn(MapBase oldMap);
    //    void RemoveJunk(Entity entity);
    //    void RemovePreference(IItemPreferenceContext context);
    //    void ResolveReferences();

    //    Control Gui { get; }
    //}
}
