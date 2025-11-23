using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    class NeedsCollection : Dictionary<Need.Types, Need>
    {
        public NeedsCollection(params Need[] needs)
        {
            foreach (var need in needs)
                this.Add(need.ID, need);
        }

        static public NeedsCollection Empty { get { return new NeedsCollection(); } }

        public override string ToString()
        {
            string text = "";
            foreach (var need in this.Values)
                text += "[" + need.Name + ": " + need.Value.ToString() + " (" + (need.Decay > 0 ? "+" : "-") + need.Decay.ToString("n2") + ")]\n";
            return text;
        }
    }
    
}
