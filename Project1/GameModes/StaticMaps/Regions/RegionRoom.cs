using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class RegionRoom : Inspectable
    {
        public override string Label => nameof(RegionRoom);
        public int ID;
        public Color Color;
        static readonly Random ColorRand = new();
        readonly HashSet<Region> Regions = new();
        readonly RegionManager Manager;
        public bool IsOutdoors = true;
        private void AssignColor()
        {
            var array = new byte[3];
            ColorRand.NextBytes(array);
            this.Color = new Color(array[0], array[1], array[2]);
        }
        public override string ToString()
        {
            return $"Room: {this.ID}\nRegions: {this.Regions.Count}\nOutdoors: {this.IsOutdoors}";
        }
        public RegionRoom(RegionManager manager)
        {
            this.ID = manager.GetRoomID();
            this.Manager = manager;
            this.AssignColor();
        }
        public void Add(Region region)
        {
            this.Regions.Add(region);
        }
        public void Remove(Region region)
        {
            this.Regions.Remove(region);
            if (this.Regions.Count == 0)
                this.Delete();
        }

        private void Delete()
        {
            this.Manager.RemoveRoom(this.ID);
        }
        public void Init()
        {
            this.IsOutdoors = false;
            foreach(var region in this.Regions)
            {
                if (region.IsOutdoors())
                {
                    this.IsOutdoors = true;
                    break;
                }
            }
        }
    }
}
