using System;
using Project1.Core.Base;
using Project1.Core.Simulation;

namespace Project1.Core.Blocks
{
    internal class BlockHealthToken
    {
        readonly static int TimerMax = Ticks.FromHours(1);
        int Timer = TimerMax;
        internal int Lifetime => TimerMax - this.Timer;
        internal float HealthPercentage => (float)this.CurrentHp / this.TotalHp;
        void Refresh() => this.Timer = TimerMax;
        internal void Tick() => this.Timer--;
        internal bool HasExpired => this.Timer <= 0;
        readonly internal Cell Cell;
        private readonly int TotalHp;
        private int CurrentHp;

        internal BlockHealthToken(Cell cell)
        {
            this.TotalHp = Cell.HitPointsMax * cell.Material.BreakResistance;
            this.CurrentHp = cell.HitPoints * cell.Material.BreakResistance;
            this.Cell = cell;
        }
        /// <summary>
        /// Returns true if chunk z-slice needs invalidation
        /// </summary>
        internal bool ApplyWork(int work)
        {
            this.Refresh();
            this.CurrentHp = Math.Min(this.TotalHp, Math.Max(0, this.CurrentHp + work));
            if (this.CurrentHp == 0)
            {
                this.Cell.HitPoints = 0;
                return true;
            }
            var nextDamageStage = (this.TotalHp - this.CurrentHp) / this.Cell.Material.BreakResistance;
            var currentDamageStage = this.Cell.Damage;
            if (nextDamageStage != currentDamageStage)
            {
                this.Cell.Damage = nextDamageStage;
                return true;
            }
            return false;
        }
    }
}
