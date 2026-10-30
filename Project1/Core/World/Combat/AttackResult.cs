using Project1.Core.Simulation;

namespace Project1.Core.World.Combat;

record struct AttackResult(SimulationTick Tick, EntityRefId Actor, AttackResultTypes Type, DamageInfo Damage) { }
