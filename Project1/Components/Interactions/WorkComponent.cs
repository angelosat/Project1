using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.Components
{
    public class WorkComponent : EntityComponent
    {
        public override string Name { get; } = "Work";
        public override object Clone()
        {
            return new WorkComponent();
        }

        public Interaction Task { get; set; }
        public TargetArgs Target { get; set; }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Jumped:
                    this.Interrupt();
                    break;

                default:
                    break;
            }
            return false;
        }

        public void Interrupt(bool success = false)
        {
            if (this.Task == null)
                return;
            this.Task.Interrupt(success);
            this.Task.FinishAction();
            this.Task = null;
            this.Target = null;
        }

        internal void OnToolContact()
        {
            this.Task.OnToolContact();
        }

        public void Perform(Interaction task, TargetArgs target)
        {
            var parent = this.Parent as Actor;
            task.Actor = parent;
            task.Target = target;
            if (task == null)
                throw new ArgumentException();
            this.Interrupt();
            this.Task = task;
            this.Target = target;
            parent.FaceTowards(this.Target);
            this.Task.InitAction();
            if (this.Task.HasFinished)
                this.Task = null;
        }

        public void End(bool success = false)
        {
            this.Interrupt(success);
        }

        public override void Tick()
        {
            var parent = this.Parent;
            if (this.Task == null)
                return;

            this.Task.Update();

            if (this.Task.State == Interaction.States.Running)
            {
                /// dont update direction here because it breaks orienting towards the bed's feet when sleeping (the bed's origin is the head part)
                //if (this.Target.Global != parent.Global)
                //{
                //    var dir = this.Target.Type == TargetType.Direction ? new Vector3(this.Target.Direction, 0) : (this.Target.Global - parent.Global);
                //    dir.Normalize();
                //    parent.Direction = dir;
                //}
                // WARNING: i had to move this here because if the interaction target was this entity itself, then the direction vector became zero and its normal became NaN
                return;
            }

            this.Task.FinishAction();
            this.Task = null;
            this.Target = null;
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.Task == null)
                return;
            this.Task.DrawUI(sb, camera);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            var isInteracting = (this.Task != null);
            w.Write(isInteracting);
            if (!isInteracting)
                return;
            this.Target.Write(w);
            w.Write(this.Task.GetType().FullName);
            this.Task.Write(w);
            //if (this.Task.Animation != null)
            //    this.Task.Animation.Write(w);
        }
        public override void Read(IDataReader r)
        {
            var isinteracting = r.ReadBoolean();
            if (!isinteracting)
                return;
            this.Target = TargetArgs.Read(null, r);
            var interactionType = r.ReadString();
            var interaction = Activator.CreateInstance(Type.GetType(interactionType)) as Interaction;
            interaction.Actor = this.Parent as Actor;
            interaction.Target = this.Target;
            if (interaction.Actor is null)
                throw new Exception();
            interaction.Read(r);
            this.Task = interaction;
            //if (this.Task.Animation != null)
            //    this.Task.Animation.Read(r);
        }
        internal override void SaveExtra(SaveTag tag)
        {
            var isInteracting = (this.Task != null);
            tag.Add(isInteracting.Save("IsInteracting"));
            if (!isInteracting)
                return;
            tag.Add(this.Target.Save("Target"));
            tag.Add(this.Task.SaveAs("Interaction"));
            //if (this.Task.Animation != null)
            //    tag.Add(this.Task.Animation.Save("Animation"));
        }
        internal override void LoadExtra(SaveTag save)
        {
            if (!save.TryGetTagValue("IsInteracting", out bool isInteracting))
                return;
            if (!isInteracting)
                return;
            this.Target = new TargetArgs(save["Target"]);
            var interactionTag = save["Interaction"];
            var inter = Interaction.Load(interactionTag);
            inter.Actor = this.Parent as Actor;
            inter.Target = this.Target;
            //inter.Animation?.Load(save["Animation"]);
            this.Task = inter;
        }

        public override void OnObjectLoaded(GameObject parent)
        {
            if (this.Task != null)
            {
                if (this.Task.Animation != null)
                {
                    parent.AddAnimation(this.Task.Animation);
                    this.Task.AfterLoad();
                }
            }
        }
        public override void OnObjectSynced(GameObject parent)
        {
            this.OnObjectLoaded(parent);
        }
        internal override void MapLoaded(GameObject parent)
        {
            if (this.Target != null)
                //this.Target.Map = parent.Map;
                this.Target.InitializeProvider(parent.Net);
        }

        internal override void ResolveReferences()
        {
            this.Task?.ResolveReferences();
        }
    }
}
