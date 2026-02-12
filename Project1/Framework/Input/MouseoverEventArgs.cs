using System;

namespace Project1.Framework.Input
{
    public class MouseoverEventArgs(object objNext, object objLast) : EventArgs
    {
        public object ObjectNext = objNext, ObjectLast = objLast;
    }
}