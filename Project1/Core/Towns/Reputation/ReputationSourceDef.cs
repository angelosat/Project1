using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Towns.Reputation;

public sealed class ReputationSourceDef(string name, Type workerType) : Def(name) 
{
    public readonly ReputationSourceWorker Worker = ActivatorSafe<ReputationSourceWorker>.CreateInstance(workerType);
}
