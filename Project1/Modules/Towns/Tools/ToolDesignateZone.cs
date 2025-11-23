namespace Start_a_Town_
{
    class ToolDesignateZone : ToolZoningPositionsNew
    {
        readonly ZoneDef Def;
        readonly int CurrentZoneID;
        int ClickedZoneID;
        int EditingZone => this.CurrentZoneID != 0 ? this.CurrentZoneID : this.ClickedZoneID;
        readonly string _helpText = "Hold control to clear designations";
        public override string HelpText => _helpText;
        readonly Town Town;
        public ToolDesignateZone()
        {

        }
        public ToolDesignateZone(Town town, ZoneDef def)
        {
            this.Def = def;
            this.CurrentZoneID = 0;
            this.Add = this.Perform;
            this.Town = town;
        }
        void Perform(IntVec3 arg1, int arg2, int arg3, bool arg4)
        {
            PacketZoneDesignation.Send(this.Town.Net, this.Def, this.EditingZone, arg1, arg2, arg3, arg4);
        }

        public override Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.MouseLeftPressed(e);
            var zone = this.Town.GetZoneAt(this.Begin);
            this.ClickedZoneID = zone != null ? zone.ID : 0;
            return Messages.Default;
        }
    }
}
