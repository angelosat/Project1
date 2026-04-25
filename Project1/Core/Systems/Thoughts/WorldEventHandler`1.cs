using Project1.Framework.Events;

namespace Project1.Core.Systems.Thoughts
{
    abstract class WorldEventHandler<T> : IWorldEventHandler where T : IEventPayload
    {
        internal abstract void Handle(T e);

        public void Register() => Registry.WorldEventHooksServer.Register<T>(this.Handle);

    }
}
