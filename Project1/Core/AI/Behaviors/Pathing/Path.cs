using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.Pathing;

public class Path : IEnumerator<IntVec3>//, ISaveable
{
    public Stack<InteractionTarget> StackTargets = new();
    public Stack<IntVec3> Stack;
    IntVec3[] Steps;
    int CurrentStepIndex;
    public IntVec3 Current => this.Steps[CurrentStepIndex];
    object IEnumerator.Current => this.Current;
    public Vector3 Previous => this.Steps[CurrentStepIndex - 1];
    public bool IsLastStep => this.CurrentStepIndex == this.Steps.Length - 1;
    HashSet<IntVec3> CellsToTraverse = new();

    public void Build(NodeBase node)
    {
        this.Stack = new Stack<IntVec3>();
        var current = node;
        while (current.Parent != null)
        {
            this.Stack.Push(current.Global);
            current = current.Parent;
        }
        this.StackTargets = new Stack<InteractionTarget>();
        var currentnode = node;
        while (current.Parent != null)
        {
            this.StackTargets.Push(currentnode.Target);
            currentnode = currentnode.Parent;
        }
        this.Steps = this.Stack.ToArray();
    }
    internal void Build(NodeBase node, IntVec3 start)
    {
        var stack = new Stack<IntVec3>();
        var current = node;
        while (current.Parent != null)
        {
            stack.Push(current.Global);

            var curr = current.Global;
            var prev = current.Parent.Global;
            if (curr.Z == prev.Z)
            {
                var cells = current.CellsToTraverse;
                foreach (var c in cells)
                    this.CellsToTraverse.Add(c);
            }
            current = current.Parent;
        }
        this.Steps = stack.ToArray();
    }
    public Path()
    {

    }
    public Path(List<IntVec3> steps)
    {
        this.Stack = new Stack<IntVec3>(steps);
    }
    public override string ToString()
    {
        var text = "";
        foreach (var item in this.Stack)
            text += item.ToString() + "\n";
        return text.TrimEnd('\n');
    }

    public bool MoveNext()
    {
        this.CurrentStepIndex++;
        return this.CurrentStepIndex < this.Steps.Length;
    }

    public bool IsValid(Actor entity)
    {
        var global = entity.Global.ToCell();
        if (this.CellsToTraverse.Contains(global))
            this.CellsToTraverse.Remove(global);
        foreach (var cell in this.CellsToTraverse)
        {
            if (Pathing.PathingSync.LosPathableFailCondition(entity.Map, cell))
                return false;
        }
        return true;
    }

    public void ConformToBlockHeights(MapBase map)
    {
        for (int i = 0; i < this.Steps.Length; i++)
        {
            var step = this.Steps[i];
            var blockHeight = Block.GetBlockHeight(map, step - IntVec3.UnitZ);
            this.Steps[i] = new IntVec3(step.X, step.Y, step.Z + blockHeight - 1);
        }
    }

    //public SaveTag Save(string name = "")
    //{
    //    var tag = new SaveTag(SaveTag.Types.Compound, name);
    //    this.CurrentStepIndex.Save(tag, "CurrentStepIndex");
    //    this.Steps.Save(tag, "Steps");
    //    this.CellsToTraverse.Save(tag, "CellsToTraverse");
    //    return tag;
    //}

    //public ISaveable Load(SaveTag tag)
    //{
    //    this.CurrentStepIndex.TryLoad(tag, "CurrentStepIndex");
    //    this.CellsToTraverse.TryLoad(tag, "CellsToTraverse");
    //    tag.TryLoad("Steps", ref this.Steps);
    //    tag.TryLoadCollection("CellsToTraverse", ref this.CellsToTraverse);
    //    return this;
    //}

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
}
//public class Path : IEnumerator<Vector3>, ISaveable
//{
//    public Stack<InteractionTarget> StackTargets = new();
//    public Stack<Vector3> Stack;
//    Vector3[] Steps;
//    int CurrentStepIndex;
//    public Vector3 Current => this.Steps[CurrentStepIndex];
//    object IEnumerator.Current => this.Current;
//    public Vector3 Previous => this.Steps[CurrentStepIndex-1];
//    public bool IsLastStep => this.CurrentStepIndex == this.Steps.Length - 1;
//    HashSet<Vector3> CellsToTraverse = new();

//    public void Build(NodeBase node)
//    {
//        this.Stack = new Stack<Vector3>();
//        var current = node;
//        while (current.Parent != null)
//        {
//            this.Stack.Push(current.Global);
//            current = current.Parent;
//        }
//        this.StackTargets = new Stack<InteractionTarget>();
//        var currentnode = node;
//        while (current.Parent != null)
//        {
//            this.StackTargets.Push(currentnode.Target);
//            currentnode = currentnode.Parent;
//        }
//        this.Steps = this.Stack.ToArray();
//    }
//    internal void Build(NodeBase node, Vector3 start)
//    {
//        var stack = new Stack<Vector3>();
//        var current = node;
//        while (current.Parent != null)
//        {
//            stack.Push(current.Global);

//            var curr = current.Global;
//            var prev = current.Parent.Global;
//            if (curr.Z == prev.Z)
//            {
//                var cells = current.CellsToTraverse;
//                foreach (var c in cells)
//                    this.CellsToTraverse.Add(c);
//            }
//            current = current.Parent;
//        }
//        this.Steps = stack.ToArray();
//    }
//    public Path()
//    {

//    }
//    public Path(List<Vector3> steps)
//    {
//        this.Stack = new Stack<Vector3>(steps);
//    }
//    public override string ToString()
//    {
//        var text = "";
//        foreach (var item in this.Stack)
//            text += item.ToString() + "\n";
//        return text.TrimEnd('\n');
//    }

//    public bool MoveNext()
//    {
//        this.CurrentStepIndex++;
//        return this.CurrentStepIndex < this.Steps.Length;
//    }

//    public bool IsValid(Actor entity)
//    {
//        var global = entity.Global.ToCell();
//        if (this.CellsToTraverse.Contains(global))
//            this.CellsToTraverse.Remove(global);
//        foreach(var cell in this.CellsToTraverse)
//        {
//            if (PathingSync.LosPathableFailCondition(entity.Map, cell))
//                return false;
//        }
//        return true;
//    }

//    public void ConformToBlockHeights(MapBase map)
//    {
//        for (int i = 0; i < this.Steps.Length; i++)
//        {
//            var step = this.Steps[i];
//            var blockHeight = Block.GetBlockHeight(map, step - Vector3.UnitZ);
//            this.Steps[i] = new Vector3(step.X, step.Y, step.Z + blockHeight - 1);
//        }
//    }

//    public SaveTag Save(string name = "")
//    {
//        var tag = new SaveTag(SaveTag.Types.Compound, name);
//        this.CurrentStepIndex.Save(tag, "CurrentStepIndex");
//        this.Steps.Save(tag, "Steps");
//        this.CellsToTraverse.Save(tag, "CellsToTraverse");
//        return tag;
//    }

//    public ISaveable Load(SaveTag tag)
//    {
//        this.CurrentStepIndex.TryLoad(tag, "CurrentStepIndex");
//        this.CellsToTraverse.TryLoad(tag, "CellsToTraverse");
//        tag.TryLoad("Steps", ref this.Steps);
//        tag.TryLoadCollection("CellsToTraverse", ref this.CellsToTraverse);
//        return this;
//    }

//    public void Dispose()
//    {
//        throw new NotImplementedException();
//    }

//    public void Reset()
//    {
//        throw new NotImplementedException();
//    }
//}
