namespace Start_a_Town_
{
    static class PlantsExtensions
    {
        /// <summary>
        /// TODO move to extensions class
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
        static public bool IsSeedFor(this GameObject entity, PlantProperties plant)
        {
            return entity.Def == ItemDefOf.Seeds && entity.GetComponent<SeedComponent>().Plant == plant;
        }
    }
}
