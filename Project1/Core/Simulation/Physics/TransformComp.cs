using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Simulation.Physics
{
    public sealed class TransformComp : EntityComp
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Transform;
        public override string Name { get; } = "Position";

        public ITransformAnchor Anchor;
        public GameObject ParentEntity;
        public Vector2 Direction;
        public Vector3 Velocity;

        Vector3 _global;
        public Vector3 Global
        {
            //get => this._global;
            get => this.Anchor?.Global ?? this._global;
            set => this._global = value;
        }
        public MapBase Map
        {
            get => this.Anchor?.Map ?? field;
            set => field = value;
        }
        public bool IsSpawned => this.Map is not null && this.Anchor is null;
        public bool IsSpawnedIn(MapBase map) => this.Map == map && this.Anchor is null;
        public void Detach()
        {
            this.Anchor = null;
            //if (!this.IsSpawned)
            //    return;
            this.Map?.Despawn(this.Owner);
            this.Map = null;
        }
        public override string ToString()
        {
            return this.Global.ToString() + "\n" +
                "Velocity: " + this.Velocity.ToString() + "\n" +
                "Direction: " + this.Direction.ToString() + "\n" +
                base.ToString();
        }
        public static Rectangle GetScreenBounds(Camera camera, SpriteComp sprComp, Vector3 global)
        {
            camera.CullingCheck(global.X, global.Y, global.Z, sprComp.Sprite.GetBounds(), out Rectangle bounds);
            return bounds;
        }

        internal override void SaveExtra(SaveTag tag)
        {
            this.Global.Save(tag, "Global");
            this.Velocity.Save(tag, "Velocity");
            this.Direction.Save(tag, "Direction");
        }
        internal override void LoadExtra(SaveTag data)
        {
            data.TryGetTag("Global", t => this.Global = t.LoadVector3());
            data.TryGetTag("Velocity", t => this.Velocity = t.LoadVector3());
            data.TryGetTag("Direction", t => this.Direction = t.LoadVector2());
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.Global);
            w.Write(this.Velocity);
            w.Write(this.Direction);
        }

        public override void Read(IDataReader r)
        {
            this.Global = r.ReadVector3();
            this.Velocity = r.ReadVector3();
            this.Direction = r.ReadVector2();
        }
    }
}
