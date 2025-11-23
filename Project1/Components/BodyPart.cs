using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    class BodyPart
    {
        public GameObjectSlot Base;
        public GameObjectSlot Wearing;

        public BodyPart(GameObject bodyPart = null, GameObject wearing = null)
        {
            Base = new GameObjectSlot(bodyPart);
            Wearing = new GameObjectSlot(wearing);
        }

        public override string ToString()
        {
            return "Base: " + Base.ToString() +
                "\nWearing: " + Wearing.ToString();
        }

        public virtual GameObject Object
        {
            get
            {
                return Wearing.Object ?? Base.Object;
            }
        }

        public List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            if (Base.Object != null)
                data.Add(new SaveTag(SaveTag.Types.Compound, "Base", Base.Save()));
            if (Wearing.Object != null)
                data.Add(new SaveTag(SaveTag.Types.Compound, "Wearing", Wearing.Save()));

            return data;
        }

        static public BodyPart Load(SaveTag data)
        {
            BodyPart part = new BodyPart();
           
            if ((data.Value as Dictionary<string, SaveTag>).ContainsKey("Base"))
                part.Base = GameObjectSlot.Create(data["Base"]);
            if ((data.Value as Dictionary<string, SaveTag>).ContainsKey("Wearing"))
                part.Wearing = GameObjectSlot.Create(data["Wearing"]);
            return part;
        }
    }
}
