using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;
using System.Linq;

namespace Project1.Core.AI
{
    internal sealed class KnowledgeManager(Actor actor)
    {
        readonly Actor Actor = actor;

        Knowledge Knowldge => field ??= Actor.AI.State.Knowledge;

        internal void OnAttachedToWorld()
        {
            var world = Actor.World;
            world.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
        }

        private void OnEntityDisposed(EntityDisposedEvent e)
        {
            Knowldge.Items.Remove(e.Entity.RefId);
        }
    }
}