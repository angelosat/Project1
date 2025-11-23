using System.Collections.Generic;

namespace Start_a_Town_
{
    public class Memory
    {
        public enum States { Invalid, Valid }

        public States State;
        public GameObject Object;
        public List<string> Needs;
        public float Score, Decay, Interest;

        public Memory(GameObject obj, float interest, float score, float decay, GameObject actor)
        {
            this.Interest = interest;
            this.Object = obj;
            this.Score = score;
            this.Decay = decay;
            this.State = States.Invalid;
            Validate(actor);
        }
        public Memory Refresh(GameObject parent)
        {
            this.Score = 100;
            if (State == States.Invalid)
                Validate(parent);
            return this;
        }
        public bool Update()
        {
            Score -= Decay;
            if (Score <= 0)
                return true;
            return false;
        }
        public override string ToString()
        {
            return Object.Name + " - Interest: " + Interest + " - Score: " + Score;
        }
        public void Validate(GameObject actor)
        {
            this.State = Memory.States.Valid;
            this.Needs = new List<string>();
        }
    }
}
