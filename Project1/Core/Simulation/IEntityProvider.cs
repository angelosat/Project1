using Project1.Core.Entities;

#nullable enable

namespace Project1.Core.Simulation;

public interface IEntityProvider
{
    Entity Resolve(EntityRefId refId);
    T Resolve<T>(EntityRefId refId) where T : Entity;
}
