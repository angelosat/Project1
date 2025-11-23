namespace Start_a_Town_.Components
{
    public class NeedEffect : ConsumableEffect
    {
        public NeedDef Type;
        public float Value;

        public NeedEffect(NeedDef type, float value)
        {
            this.Type = type;
            this.Value = value;
        }

        public override void Apply(GameObject actor)
        {
            var need = actor.GetNeed(this.Type);
            if (need == null)
                return;
            //need.Value += this.Value;
            need.SetValue(need.Value + this.Value, actor);
        }
    }
}
