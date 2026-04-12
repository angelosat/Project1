using Project1.Core.Graphics;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Input.Orders
{
    public enum ValidSelectedCount { Any, Single }
    public sealed class OrderCommandDef(string name, Sprite sprite, Type workerType, ValidSelectedCount validCount = default) : Def(name)
    {
        public Sprite Sprite = sprite;
        public string Verb;
        public ValidSelectedCount ValidCount = validCount;
        internal readonly OrderCommandWorker Worker = ActivatorSafe<OrderCommandWorker>.CreateInstance(workerType);
    }
}
