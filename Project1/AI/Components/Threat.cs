using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    public class Threat : IComparable<Threat>
    {
        public float Value { get; set; }
        public GameObject Parent { get; set; }
        public GameObject Entity { get; set; }
        public Threat(GameObject parent, float value, GameObject entity)
        {
            this.Parent = parent;
            this.Value = value;
            this.Entity = entity;
        }
        public int CompareTo(Threat other)
        {
            if (this.Entity == other.Entity)
                return 0;
            if (this.Value != other.Value)
                return this.Value > other.Value ? -1 : 1;
            float thisDistance = Vector3.Distance(this.Parent.Global, this.Entity.Global);
            float otherDistance = Vector3.Distance(this.Entity.Global, other.Entity.Global);
            if (thisDistance != otherDistance)
                return thisDistance < otherDistance ? -1 : 1;
            return this.Entity.RefID < other.Entity.RefID ? -1 : 1;
        }
        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Entity.Name, this.Value.ToString());
        }
    }
}
