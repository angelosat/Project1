using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public partial class LightingEngine
    {
        public class BatchToken
        {
            public Queue<IntVec3> Queue = new();
            public Action Callback = () => { };
            public BatchToken()
            {

            }
            public BatchToken(IEnumerable<IntVec3> positions)
            {
                this.Queue = new Queue<IntVec3>(positions);
            }

            public BatchToken(Action callback)
            {
                this.Callback = callback;
            }
            public BatchToken(IEnumerable<IntVec3> positions, Action callback)
            {
                this.Callback = callback;
                this.Queue = new Queue<IntVec3>(positions);
            }
        }
    }
}
