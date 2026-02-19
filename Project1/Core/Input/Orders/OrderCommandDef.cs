using Project1.Core.Graphics;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Input.Orders
{
    public sealed class OrderCommandDef(string name, Sprite sprite, Type workerType) : Def(name)
    {
        public Sprite Sprite = sprite;
        public string Verb;
        internal readonly CommandWorker Worker = ActivatorSafe<OrderCommandWorker>.CreateInstance(workerType);
    }
}
