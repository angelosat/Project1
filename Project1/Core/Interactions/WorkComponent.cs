using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Screens;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Interactions;

public class WorkComponent : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Work;
    public new class Spec : Spec<WorkComponent> { }
    public override string Name { get; } = "Work";
    public Interaction Task { get; set; }
    public InteractionTarget Target { get; set; }

    public void Interrupt(bool success = false)
    {
        if (this.Task == null)
            return;
        if(this.Owner.Net.IsClient)
            $"{this.Owner.Net} interrupt".ToConsole();
        this.Task.Interrupt(success);
    }
    internal void OnAnimationHook()
    {
        this.Task.OnAnimationHook();
    }
    internal void OnToolContact()
    {
        this.Task.OnToolContact();
    }
    public Interaction Perform(InteractionDef taskDef, InteractionTarget target, int count = -1)
    {
        var interaction = taskDef.Create(this.Owner as Actor, target, count);
        this.Start(interaction);
        this.Owner.Map.Events.Post(new InteractionStartedEvent(this.Owner as Actor, taskDef, target));
        return interaction;
    }
    public void Start(Interaction task)
    {
        ArgumentNullException.ThrowIfNull(task);
        var parent = this.Owner as Actor;
        this.Interrupt();
        this.Task = task;
        this.Target = task.Target;
        parent.FaceTowards(this.Target);
    }

    public void End(bool success = false)
    {
        this.Interrupt(success);
    }

    public override void Tick()
    {
        if (this.Task == null)
            return;

        this.Task.Tick();

        if (this.Task.State == Interaction.States.Running)
            return;

        if(this.Task.State == Interaction.States.Finished)
            Stop();
    }

    public void Stop()
    {
        if (this.Task is null)
            return;
        this.Task.StopAnimation();
        this.Task = null;
        this.Target = null;
        this.Owner.LastMap.Events.Post<InteractionStoppedEvent>(new(this.Owner as Actor));
    }

    public override void DrawUI(SpriteBatch sb, MapViewport viewport)
    {
        if (this.Task == null)
            return;
        this.Task.DrawUI(sb, viewport.Camera);
    }

    public override void Write(IDataWriter w)
    {
        var isInteracting = (this.Task != null);
        w.Write(isInteracting);
        if (!isInteracting)
            return;
        this.Target.Write(w);
        w.Write(this.Task.Def);
        this.Task.Write(w);
    }
    public override void Read(IDataReader r)
    {
        var isinteracting = r.ReadBoolean();
        if (!isinteracting)
            return;
        this.Target = InteractionTarget.Read(this.Owner.World, r);
        var interactionDef = r.ReadDef<InteractionDef>();
        var interaction = interactionDef.Create(this.Owner as Actor, this.Target);
        interaction.Read(r);
        this.Task = interaction;
    }
    internal override void SaveExtra(SaveTag tag)
    {
        var isInteracting = (this.Task != null);
        tag.Add(isInteracting.Save("IsInteracting"));
        if (!isInteracting)
            return;
        tag.Add(this.Target.Save("Target"));
        tag.Add(this.Task.SaveAs("Interaction"));
    }
    internal override void LoadExtra(SaveTag save)
    {
        if (!save.TryGetTagValueOrDefault("IsInteracting", out bool isInteracting))
            return;
        if (!isInteracting)
            return;
        this.Target = new InteractionTarget(save["Target"]);
        var interactionTag = save["Interaction"];
        var inter = Interaction.Load(interactionTag);
        this.Task = inter;
    }
    public override void OnObjectSynced(GameObject parent)
    {
        this.OnObjectLoaded(parent);
    }
    internal override void OnMapLoaded(GameObject parent)
    {
        this.Target?.InitializeProvider(parent.Map.World);
    }
}
