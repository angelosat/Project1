namespace Start_a_Town_
{
    public class ToolUseDef : Def/*, IItemPreferenceContext*/
    {
        public string Description { get; protected set; }

        public ToolUseDef(string name, string description) : base(name)
        {
            this.Description = description;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
