using Project1.Framework.Events;

namespace Project1.Core.Systems.Thoughts
{
    internal abstract class ThoughtSource<T> : WorldEventHandler<T> where T : IEventPayload
    {

    }
}
