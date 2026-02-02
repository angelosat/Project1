using Project1.Framework.Needs;

namespace Project1.Framework.Needs.Types
{
    class NeedEnergy : NeedWorker
    {
        //Resource _cachedStamina;
        //Resource Stamina => this._cachedStamina ??= this.Parent.Resources[ResourceDefOf.Stamina];

        //public NeedEnergy(Actor parent) : base(parent)
        //{

        //}

        //protected override float FinalDecayMultiplier => 1 + 1 - this.Stamina.CurrentThreshold.Value;
    }
    //class NeedEnergy : Need
    //{
    //    Resource _cachedStamina;
    //    Resource Stamina => this._cachedStamina ??= this.Parent.Resources[ResourceDefOf.Stamina];

    //    public NeedEnergy(Actor parent) : base(parent)
    //    {

    //    }

    //    protected override float FinalDecayMultiplier => 1 + 1 - this.Stamina.CurrentThreshold.Value;
    //}
}
