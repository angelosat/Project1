using System;

namespace Start_a_Town_
{
    public class ResourceThreshold// : Def
    {
        public readonly string Label;
        ResourceThreshold Next;
        readonly public float Value;
        public ResourceThreshold(string name, float value)// : base(name)
        {
            this.Label = name;
            this.Value = value;
        }
        public ResourceThreshold Get(float value)
        {
            if (value <= this.Value)
                return this;
            else
                return this.Next?.Get(value) ?? this;
        }
        public int GetDepth(float value)
        {
            var n = 0;
            var current = this;
            do
            {
                if (value <= current.Value || current.Next == null)
                    return n;
                current = current.Next;
                n++;
            } while (true);
        }
        public float GetThresholdValue(int index)
        {
            var current = this;
            var n = 0;
            while (n < index)
            {
                current = current.Next;
            }
            return current.Value;
        }
        public ResourceThreshold Add(ResourceThreshold next)
        {
            this.Next = this.Next?.Add(next) ?? next;
            if (next.Value <= this.Value)
                throw new Exception();
            return this;
        }
        public override string ToString()
        {
            return $"{this.Label}: {this.Value:##0%}";
        }
    }
}
