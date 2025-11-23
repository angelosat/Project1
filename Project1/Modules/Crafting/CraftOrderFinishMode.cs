using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class CraftOrderFinishMode
    {
        public enum Modes { XTimes, UntilX, Forever }
        public Modes Mode;
        public Func<CraftOrder, string> GetString;
        public Func<CraftOrder, bool> IsActive;
        public Action<CraftOrder> OnComplete = o => { };
        public override string ToString()
        {
            return base.ToString();
        }
        CraftOrderFinishMode()
        {

        }
        static readonly CraftOrderFinishMode XTimes = new CraftOrderFinishMode() { Mode = Modes.XTimes, GetString = c => string.Format("Do {0} times", c.Quantity), IsActive = c => c.Quantity > 0, OnComplete = c => c.Quantity-- };
        static readonly CraftOrderFinishMode UntilX = new CraftOrderFinishMode() { Mode = Modes.UntilX, GetString = c => string.Format("Do until {0}", c.Quantity), IsActive = c => false };
        static readonly CraftOrderFinishMode Forever = new CraftOrderFinishMode() { Mode = Modes.Forever, GetString = c => string.Format("Do forever", c.Quantity), IsActive = c => true };
        public static readonly List<CraftOrderFinishMode> AllModes = new List<CraftOrderFinishMode>() { XTimes, UntilX, Forever };
        public static readonly Dictionary<Modes, CraftOrderFinishMode> All = new Dictionary<Modes, CraftOrderFinishMode>() {
            { Modes.XTimes, XTimes },
            { Modes.UntilX, UntilX },
            { Modes.Forever, Forever } };

        internal static CraftOrderFinishMode GetMode(int v)
        {
            return All[(Modes)v];
        }
    }
}
