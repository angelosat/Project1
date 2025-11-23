using Start_a_Town_.UI;

namespace Start_a_Town_.AI
{
    class PersonalityUI : GroupBox
    {
        public PersonalityUI()
        {

        }
        public void Refresh(Actor npc)
        {
            this.ClearControls();
            var p = npc.Personality;
            foreach (var t in p.Traits)
            {
                this.AddControlsBottomLeft(t.GetListControlGui());
            }
        }
    }
}
