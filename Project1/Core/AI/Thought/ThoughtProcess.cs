using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.AI.Thought
{
    public abstract class ThoughtProcess
    {
        public abstract void Tick(AIState state);
    }
}
