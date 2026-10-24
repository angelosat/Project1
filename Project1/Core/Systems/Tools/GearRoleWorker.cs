using Project1.Core.Entities;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Systems.Tools
{
    public class GearRoleWorker
    {
        public virtual IEnumerable<LabelNew> GetControls(Entity item) => [];
        public virtual void InitIntrinsicStats(Entity item) { }
    }
}
