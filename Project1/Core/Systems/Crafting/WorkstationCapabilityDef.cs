using Project1.Core.AI;
using Project1.Core.Towns.Duties;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Crafting;

public sealed class WorkstationCapabilityDef(string name, Type workerType, DutyDef operatorDuty) : Def(name)
{
    public Type OrderType;
    public Type Output;
    public Def[] OutputSpecific = [];
    public PlanDef Plan;
    public DutyDef OperatorDuty = operatorDuty;

    public WorkstationCapabilityWorker Worker => field ??= ActivatorSafe<WorkstationCapabilityWorker>.CreateInstance(workerType);
}
