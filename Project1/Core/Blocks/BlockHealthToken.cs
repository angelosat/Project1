using Project1.Core.Simulation;
using System;

namespace Project1.Core.Blocks
{
    public interface IBlockHealth
    {
        float HealthPercentage { get; }
    }

    internal class BlockHealthToken : IBlockHealth
    {
        readonly static int TimerMax = Ticks.FromHours(1);
        int Timer = TimerMax;
        internal int Lifetime => TimerMax - this.Timer;
        public float HealthPercentage => (float)this.CurrentHp / this.TotalHp;
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
    
        internal BlockDamageResult ApplyWork(int work)
        {
            this.Refresh();
            this.CurrentHp = Math.Min(this.TotalHp, Math.Max(0, this.CurrentHp + work));
            if (this.CurrentHp == 0)
            {
                this.Cell.HitPoints = 0;
                return BlockDamageResult.HitPointsDepleted;
            }
            var nextDamageStage = (this.TotalHp - this.CurrentHp) / this.Cell.Material.BreakResistance;
            var currentDamageStage = this.Cell.Damage;
            if (nextDamageStage != currentDamageStage)
            {
                this.Cell.Damage = nextDamageStage;
                return BlockDamageResult.DamageLevelChanged;
            }
            return BlockDamageResult.NoChange;
        }

        internal enum BlockDamageResult { NoChange, DamageLevelChanged, HitPointsDepleted }
    }
}
