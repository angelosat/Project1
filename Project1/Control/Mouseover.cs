using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class Mouseover
    {
        public bool Valid;
        object _Object;
        public object Object
        {
            get
            {
                return _Object;
            }
            set
            {
                this._Object = value;
                if (value is Slot)
                {
                    this.Target = new TargetArgs((value as Slot).Tag);
                }
                else if (value is GameObject && !Controller.BlockTargeting)
                { 
                    this.Target = new TargetArgs(value as GameObject);
                    this.TargetEntity = this.Target;
                }
                else if (value is TargetArgs)
                {
                    var target = value as TargetArgs;
                    if (target.Type == TargetType.Position)
                        this.TargetCell = target;
                    else if (target.Type == TargetType.Entity)
                        this.Target = target;
                }
            }
        }
        public bool Multifaceted;
        public Vector3 Face;
        public Vector3 Precise;
        public TargetArgs Target, TargetCell, TargetEntity;
        public float Depth = float.MinValue;//1;


        public bool TryGet<T>(out T obj) where T : class
        {
            obj = this.Object as T;
            return obj is T;
        }
        public override string ToString()
        {
            return Object != null ? Object.ToString() : "<null>";
        }
    }
}
