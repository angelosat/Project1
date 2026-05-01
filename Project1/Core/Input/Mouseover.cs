using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.UI.Slots;
using System;

namespace Project1.Core.Input;

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
                var goslot = (value as Slot).Tag;
                this.Target = new InteractionTarget(goslot);
            }
            else if (value is GameObject && !Controller.BlockTargeting)
            { 
                this.Target = new InteractionTarget(value as Entity);
                this.TargetEntity = this.Target;
            }
            else if (value is InteractionTarget)
            {
                var target = value as InteractionTarget;
                if (target.Type == TargetType.Cell)
                    this.TargetCell = target;
                else if (target.Type == TargetType.Entity)
                    this.Target = target;
            }
        }
    }
    public bool Multifaceted;
    public Vector3 Face;
    public Vector3 Precise;
    public InteractionTarget Target, TargetCell, TargetEntity;
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
