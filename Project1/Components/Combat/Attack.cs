using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    [Obsolete]
    class Attack
    {
        public enum States { Ready, Charging, Charged, Delivering }
        public static double DefaultArc = Math.PI / 6d;

        public Vector3 Direction, Momentum;
        public GameObject Attacker;
        public float Charge;
        public Func<Vector3, Vector3, GameObject, bool> CollisionType;

        public int Value
        {
            get
            {
                var charge = Math.Max(.1f, this.Charge); // maybe change range of charge to never be 0?
                return (int)Math.Ceiling(charge);// * Damage.Values.Aggregate((a, b) => a + b)); 
            }
        }

        static public Attack Create(GameObject source, Vector3 direction, Func<Vector3, Vector3, GameObject, bool> collisionType, Func<float> chargeFunc)
        {
            return new Attack(source, direction, collisionType, chargeFunc());
        }

        Attack(GameObject attacker, Vector3 direction, Func<Vector3, Vector3, GameObject, bool> collisionType, float charge)
        {
            this.Attacker = attacker;
            this.Direction = direction;
            this.Momentum = attacker.Velocity;
            this.CollisionType = collisionType;
            Charge = charge;
            if (attacker.TryGetComponent<InventoryComponent>(c =>
            {
                //GameObjectSlot holdSlot = null;
                //this.Damage = WeaponComponent.GetDamage(holdSlot.Object) ?? WeaponComponent.GetDamage(attacker.GetComponent<BodyComponent>().BodyParts[Stat.Mainhand.Name].Base.Object);
            })) { }
            else
            {
                //this.Damage = WeaponComponent.GetDamage(attacker.GetComponent<BodyComponent>().BodyParts[Stat.Mainhand.Name].Base.Object);
            }
        }

        static public Func<Vector3, Vector3, GameObject, bool> Ray
        {
            get
            {
                return (origin, direction, target) =>
                {
                    Ray ray = new Ray(origin, direction);
                    var cylinder = BoundingCylinder.Create(target);
                    var success = cylinder.Intersects(ray);
                    return success;
                };
            }
        }
        static public Func<GameObject, GameObject, Vector3, bool> Arc
        {
            get
            {
                return (actor, target, direction) =>
                {
                    Vector3 distance = target.Global - actor.Global;
                    distance.Normalize();
                    float angle = (float)Math.Acos(Vector3.Dot(direction, distance));
                    return (Math.Abs(angle) < Components.Attack.DefaultArc);
                };
            }
        }

        internal Vector3 GetMomentum()
        {
            float coef = this.Charge;// * (1 + StatsComponent.GetStatOrDefault(this.Attacker, Stat.Types.Knockback, 0));
            return this.Momentum * coef;
        }
    }
}
