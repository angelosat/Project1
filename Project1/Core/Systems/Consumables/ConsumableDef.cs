using Project1.Core.Graphics;
using Project1.Core.Systems.Consumables.Scrolls;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Consumables;

public sealed class ConsumableDef(string name, string verb, Sprite sprite, Type effectType, Type workerType) : Def(name)
{
    public string Verb = verb;
    public Sprite Sprite = sprite;
    public ConsumableEffect Effect => field ??= ActivatorSafe<ConsumableEffect>.CreateInstance(effectType);
    public ConsumableWorker Worker => field ??= ActivatorSafe<ConsumableWorker>.CreateInstance(workerType);
}
