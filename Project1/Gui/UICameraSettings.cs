using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class UICameraSettings : Panel
    {
        Camera Camera;
        Camera GetCamera()
        {
            return Ingame.GetMap().Camera;
        }
        public UICameraSettings(Camera camera)
        {
            AutoSize = true;
            this.Camera = camera;
            this.AddControlsVertically(
                new CheckBoxNew("Hide Walls", () => Engine.HideWalls = !Engine.HideWalls, () => Engine.HideWalls) { HoverText = "Hides blocks obscuring an entity when the camera is following it" },
                new CheckBoxNew("Hide Terrain", () => camera.ToggleHideBlocksAbove(), () => camera.HideTerrainAbovePlayer),
                //new CheckBoxNew("Block Outlines", () => Engine.BlockOutlines = !Engine.BlockOutlines, () => Engine.BlockOutlines),
                new CheckBoxNew("Fog", () => Camera.Fog = !Camera.Fog, () => Camera.Fog) { HoverText = "Toggles drawing fog at lower elevation levels than the one under the mouse cursor" },
                new CheckBoxNew("Hide blocks", () => camera.HideUnknownBlocks = !camera.HideUnknownBlocks, () => camera.HideUnknownBlocks) { HoverText = "Toggles blocks not exposed to air being drawn as unknown blocks" },
                new CheckBoxNew("Map boundaries", () => camera.DrawTopSlice = !camera.DrawTopSlice, () => camera.DrawTopSlice) { HoverText = "Draws blocks at the boundaries of the map" },
                new CheckBoxNew("Designations", () => camera.DrawZones = !camera.DrawZones, () => camera.DrawZones) { HoverText = "Toggles drawing of designated zones" },
                new CheckBoxNew("Nameplates", () => NameplateManager.ToggleNameplates(), () => Ingame.Instance.NameplateManager.NameplatesEnabled) { HoverText = "Toggles entity nameplates" },
                new CheckBoxNew("Rooms", () => Engine.DrawRooms = !Engine.DrawRooms, () => Engine.DrawRooms),
                new CheckBoxNew("Hide Room Walls", () => camera.HideWalls = !camera.HideWalls, () => camera.HideWalls)
            );
        }
       
        void BorderShading_Click(object sender, EventArgs e)
        {
            this.Camera.BorderShading = !this.Camera.BorderShading;
        }

        void HideUnderground_Click(object sender, EventArgs e)
        {
            this.Camera.HideUnderground = !this.Camera.HideUnderground;
        }

        void Fog_Click(object sender, EventArgs e)
        {
            Camera.Fog = !Camera.Fog;
        }

        public override void Reposition(Vector2 ratio)
        {
            base.Reposition(ratio);
        }
    }
}
