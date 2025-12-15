using System;

namespace Start_a_Town_
{
    public class ItemSystem : IItemCreationSystem
    {
        public Entity Create(Def profile, ItemCreationArgs args)
        {
            if(profile is not ItemDef def)
                throw new InvalidOperationException($"{nameof(ItemSystem)} received wrong profile");
            //var entity = (Entity)Activator.CreateInstance(def.ItemClass);
            //entity.InitComps(def);
            var entity = def.Create();
            return entity;
        }
    }
}
