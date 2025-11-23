using System;

namespace Start_a_Town_
{
    public class MouseoverEventArgs : EventArgs
    {
        public Object ObjectNext, ObjectLast;
        public MouseoverEventArgs(Object objNext, Object objLast)
        {
            ObjectNext = objNext;
            ObjectLast = objLast;
        }
    }
}
