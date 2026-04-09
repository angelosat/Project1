using System.Collections.Generic;

namespace Project1.Core.Towns.Services;

public readonly record struct TownServiceRequestId(ulong Value)
{
    public static readonly TownServiceRequestId Null = new(0);
    public static implicit operator TownServiceRequestId(ulong v) => new(v);
    public static implicit operator ulong(TownServiceRequestId v) => (ulong)v.Value;
}

public class TownComp_ServiceRequests : TownComp
{
    readonly Dictionary<TownServiceRequestId, TownServiceRequest> _openRequests = [];

    public TownComp_ServiceRequests(Town town) : base(town)
    {
    }

    public override string Name => "Services";

    TownServiceRequestId NextId => ++field;

    internal TownServiceRequestId Register(TownServiceRequest request)
    {
        var id = this.NextId;
        request.Id = id;
        this._openRequests.Add(id, request);
        return id;
    }

    internal void Remove(TownServiceRequestId id)
        => this._openRequests.Remove(id);

    public TownServiceRequest Get(TownServiceRequestId id)
        => this._openRequests[id];
}
