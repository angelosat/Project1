using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IItemPreferenceContext
    { }
    public interface IItemPreferencesManager : ISaveable, ISerializableNew
    {
        IItemPreferenceContext GetPreference(Entity item);
        Entity GetPreference(IItemPreferenceContext context);
        Entity GetPreference(IItemPreferenceContext context, out int score);
        IEnumerable<Entity> GetJunk();
        void RemoveJunk(Entity entity);
        //bool AddPreference(Entity item);
        void AddPreference(IItemPreferenceContext context, Entity item, int score);
        void RemovePreference(IItemPreferenceContext context);
        bool IsPreference(Entity item);
        Control Gui { get; }
        void ResolveReferences();
        int GetScore(IItemPreferenceContext context, Entity item);
        Control GetListControl(Entity entity);
        (IItemPreferenceContext role, int score) FindBestRole(Entity entity);
        IEnumerable<(IItemPreferenceContext role, int score)> Evaluate(Entity entity);
        IEnumerable<(IItemPreferenceContext role, Entity item, int score)> GetPotential();
        void OnSpawn(MapBase oldMap);
        void OnDespawn(MapBase oldMap);
        void OnMapLoaded();
        void ModifyBias(Entity entity, int value);
        void ForceDrop(Entity item);
        IEnumerable<(IItemPreferenceContext role, Entity item, int score)> GetPotentialAll();
    }
}
