using System;

namespace Project1.Core.Input
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
