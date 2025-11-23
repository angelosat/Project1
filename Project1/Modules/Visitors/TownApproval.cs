using System;

namespace Start_a_Town_
{
    public class TownApproval : IProgressBar
    {
        public int ApprovalMin = -100, ApprovalMax = 100;
        public float Value;
        public float Percentage
        {
            get => this.Value / ApprovalMax;
            set => this.Value = (ApprovalMax - ApprovalMin) * value;
        }
        public float Rating => this.Value >= 0 ? this.Value / ApprovalMax : this.Value / ApprovalMin;
    }
}
