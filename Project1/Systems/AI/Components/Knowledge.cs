using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.AI
{
    public class Knowledge
    {
        public Dictionary<GameObject, Memory> Objects = new Dictionary<GameObject, Memory>();
        public void Update()
        {
            // TODO: maybe create a new list and add only continuing memories instead of removing them
            foreach (KeyValuePair<GameObject, Memory> memory in this.Objects.ToDictionary(foo => foo.Key, foo => foo.Value))
            {
                if (memory.Value.Update())
                    this.Objects.Remove(memory.Key);
            }
        }

        public void Invalidate()
        {
            foreach(var obj in Objects.Values)
                obj.State = Memory.States.Invalid;
        }

        public override string ToString()
        {
            string text = "";
            foreach (Memory m in Objects.Values)
                text += m.ToString() + "\n";
            return text.TrimEnd('\n');
        }
    }
}
