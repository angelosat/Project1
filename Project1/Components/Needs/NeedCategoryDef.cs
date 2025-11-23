namespace Start_a_Town_
{
    public sealed class NeedCategoryDef : Def
    {
        public NeedDef[] BaseNeedDefs;
        public NeedCategoryDef(string name) : base(name)
        {

        }
        static public readonly NeedCategoryDef NeedCategoryPhysiological = new NeedCategoryDef("Physiological")
        {
            BaseNeedDefs = new NeedDef[] {
                NeedDef.Energy,
                NeedDef.Hunger,
                NeedDef.Curiosity, }
        };

        static public readonly NeedCategoryDef NeedCategoryRelationships = new NeedCategoryDef("Relationships")
        {
            BaseNeedDefs = new NeedDef[] {
                NeedDef.Social }
        };

        static public readonly NeedCategoryDef NeedCategoryEsteem = new NeedCategoryDef("Esteem")
        {
            BaseNeedDefs = new NeedDef[] {
                NeedDef.Work }
        };

        static public readonly NeedCategoryDef NeedCategoryCognitive = new NeedCategoryDef("Cognitive")
        {
            BaseNeedDefs = new NeedDef[] {
                NeedDef.Curiosity }
        };
    }
}
