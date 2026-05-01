using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Input;
using Project1.Framework.UI;
using System.Linq;
using System.Windows.Forms;

namespace Project1.Core;

internal sealed class MousePicker
{
    public static bool BlockTargeting = true;

    public void Perform(MapViewport viewport, bool ignoreEntities = false)
    {
        var result = this.MousePicking(viewport, UIManager.Mouse, ignoreEntities);
        if (!result.HasValue)
            return;
        this.CreateMouseover(viewport, result.Value);
    }

    public PickerResult? MousePicking(MapViewport viewport, Vector2 mousePos, bool ignoreEntities = false)
    {
        var map = viewport.Map;
        var camera = viewport.Camera;
        var width = camera.Width;
        var height = camera.Height;
        var zoom = camera.Zoom;
        var origin = viewport.Origin;
        //var renderer = ctx.Renderer;
        var visibleChunks = map.GetActiveChunks().Values.Where(ch => viewport.Viewport.Intersects(ch.GetScreenBounds(viewport)));
        if (!(ignoreEntities || Controller.IsBlockTargeting()))
            foreach (var chunk in visibleChunks)
                chunk.HitTestEntities(viewport);

        /// uncomment this to prefer targetting entities even when they are behind blocks
        //if (Controller.Instance.MouseoverNext.Object is not null)
        //    return;

        if (!BlockTargeting)
            return null;

        var controller = Controller.Instance;
        var hidewalls = Engine.HideWalls;
        var actor = map.Net.GetPlayer()?.ControllingEntity;
        var playerExists = actor != null;
        var playerGlobal = playerExists ? actor.Global : default;
        var radius = .01f * camera.Zoom * camera.Zoom; //occlusion radius
        var found = false;
        var foundDepth = float.MinValue;
        var foundGlobal = Vector3.Zero;
        var foundMouse = Vector2.Zero;
        Block foundBlock;
        var foundRect = Rectangle.Empty;
        //var camx = camera.Coordinates.X - (width / 2f) / zoom;
        //var camy = camera.Coordinates.Y - (height / 2f) / zoom;
        var mouse = mousePos;// UIManager.Mouse;
        var mousex = (int)mouse.X;
        var mousey = (int)mouse.Y;
        var behind = InputState.IsKeyDown(Keys.Menu);

        var rectw = (int)(Block.Width * zoom);
        var recth = (int)(Block.Height * zoom);
        foreach (var chunk in visibleChunks)
        {
            var chunkBounds = chunk.GetScreenBounds(viewport); // TODO: i already have this, cache it
            if (!chunkBounds.Contains(mousex, mousey))
                continue;

            //Coords.Iso(camera, chunk.X * Chunk.Size, chunk.Y * Chunk.Size, 0, out float chunkx, out float chunky);
            camera.Iso(chunk.X * Chunk.Size, chunk.Y * Chunk.Size, 0, out float chunkx, out float chunky);
            //chunkx -= camx;
            //chunky -= camy;
            chunkx -= origin.X;
            chunky -= origin.Y;
            var foglvl = viewport.FogLevel;
            var drawLevel = viewport.Settings.DrawLevel;
            for (int j = drawLevel; j >= foglvl; j--)
            {
                var slice = chunk.Slices[j];
                if (slice is null)
                    continue;

                /// removing this check because it screws up mousepicking when slices are invalidated by blocks changing (like actors trampling grass)
                //if (!slice.Valid)
                //    continue;
                if (slice.Canvas is null)
                    continue;


                var arrays = slice.Canvas.GetMouseoverableMeshes();


                //if (j == this.MaxDrawZ)
                //    arrays.Add(slice.Unknown.vertices);
                if (j == drawLevel)
                {
                    // i've consolidated mysterious blocks into the "cover" canvas, and removed the "unknown" spritebatch from the slice structure
                    //if(this.MysteriousBlocks)
                    //    arrays = arrays.Append(slice.Unknown.vertices);
                    //else
                    arrays = arrays.Concat(slice.Cover.GetMouseoverableMeshes());
                }

                // HACK
                //if(map.Town.DesignationManager.Renderers[DesignationDefOf.Construct].Slices.TryGetValue(j, out var constructionDesignationMesh))
                //    arrays = arrays.Append(constructionDesignationMesh.vertices);


                foreach (var array in arrays)
                {
                    var count = array.Length;
                    for (int i = count - 4; i >= 0; i -= 4)
                    {
                        if (!this.EarlyOutMousePicking(zoom, array, i, mousex, mousey, chunkx, chunky, rectw, recth, out int rectx, out int recty, out Vector3 global))
                            continue;


                        //var block = chunk.GetBlockFromGlobal(global.X, global.Y, global.Z);
                        var block = map.GetCell(global).Block;

                        if (!block.IsTargetable(global))
                            continue;

                        if (hidewalls)
                        {
                            if (playerExists)
                            {
                                if (global.Z >= playerGlobal.Z)
                                {
                                    if (global.X + global.Y > playerGlobal.X + playerGlobal.Y)
                                    {
                                        if (block.Opaque)
                                        {
                                            //distance between mouse and center of screen normalized between -1,1
                                            var dx = mousex - width / 2f;
                                            var dy = mousey - height / 2f;
                                            var d = new Vector2(dx, dy);
                                            d.Y /= width / (float)height;
                                            d /= new Vector2(width / 2f, height / 2f);
                                            var l = d.LengthSquared();
                                            if (l < radius)
                                                continue;
                                        }
                                    }
                                }
                            }
                        }

                        var xx = (int)((mousex - rectx) / zoom);
                        var yy = (int)((mousey - recty) / zoom);
                        if (!block.MouseMap.HitTestEarly(xx, yy))
                            continue;

                        Coords.Rotate(camera, global.X, global.Y, out int rx, out int ry);
                        var currentDepth = rx + ry + global.Z;

                        if (currentDepth > foundDepth)
                        {
                            foundDepth = currentDepth;
                            foundGlobal = global;
                            foundMouse = mouse;
                            foundRect = new Rectangle(rectx, recty, rectw, recth);
                            foundBlock = block;
                            found = true;
                        }
                        //}
                    }
                }
            }

        }
        if (found)
        {
            // create mouseover anyway even if air in case of undiscovered area? or check drawunknownblocks?
            //this.CreateMouseover(map, foundGlobal, foundRect, foundMouse, behind);
            return new(map, foundGlobal, foundRect, foundMouse, behind);
        }
        return null;
    }


    public bool EarlyOutMousePicking(float zoom, MyVertex[] array, int i, float mousex, float mousey, float chunkx, float chunky, int rectw, int recth, out int rectx, out int recty, out Vector3 global)
    {
        rectx = recty = 0;
        var v = array[i];
        global = v.BlockCoords;
        var tl = v.Position;

        var br = array[i + 2].Position;
        if (br.X - tl.X == 0)
            return false;

        var xxx = tl.X + chunkx;
        rectx = (int)(xxx * zoom);
        if (mousex < rectx)
            return false;

        var yyy = tl.Y + chunky;
        recty = (int)(yyy * zoom);
        if (mousey < recty)
            return false;

        if (mousex >= rectx + rectw)
            return false;

        if (mousey >= recty + recth)
            return false;

        return true;
    }

    public void CreateMouseover(MapViewport viewport, PickerResult result)
    {
        var point = result.Point;
        var camera = viewport.Camera;
        var global = result.Global;
        var map = viewport.Map;
        var rect = result.Rect;
        var zoom = camera.Zoom;
        var behind = result.Behind;
        var rotation = camera.Rotation;
        /// uncomment this to prefer targetting entities even when they are behind blocks
        /// i also call this at the start of the mouspicking method, no need to call it here too
        //if (Controller.Instance.MouseoverNext.Object != null)
        //    return;
        if (Controller.Instance.MouseoverNext.Object is InteractionTarget target && target.Object is Entity obj)
            if (camera.GetDrawDepthSimple(obj.CellIfSpawned.Value) > camera.GetDrawDepthSimple(global)) // HACK
                return;

        if (!map.TryGetAll(global, out var chunk, out var cell))
            return;

        var uvCoords = new Vector2((point.X - rect.X) / zoom, (point.Y - rect.Y) / zoom);
        int faceIndex = (int)uvCoords.Y * cell.Block.MouseMap.Texture.Width + (int)uvCoords.X;

        // find block coordinates
        var sample = cell.Block.UV[faceIndex];
        float u = sample.R / 255f;
        float v = sample.G / 255f;
        float w = sample.B / 255f;
        var precise = new Vector3(u, v, w);// Vector3.Zero;
        precise.X -= 0.5f;
        precise.Y -= 0.5f; // compensate for (0,0) being at the center of the block

        cell.Block.MouseMap.HitTest(behind, (int)uvCoords.X, (int)uvCoords.Y, out Vector3 vec);

        // comment these lines if i want to select blocks even if mouseover face is inaccessible
        //if (!Cell.CheckFace(this, cell, vec))
        //    return;

        Coords.Rotate((int)rotation, vec, out Vector3 rotVec);
        precise = precise.Rotate(-rotation);
        // TODO: find more elegant way to do this
        if (rotVec == Vector3.UnitX || rotVec == -Vector3.UnitX)
            precise.X = 0;
        else if (rotVec == Vector3.UnitY || rotVec == -Vector3.UnitY)
            precise.Y = 0;
        else if (rotVec == Vector3.UnitZ || rotVec == -Vector3.UnitZ)
            precise.Z = 0;
        var depth = global.GetDrawDepth(map, camera);
        Controller.SetMouseoverBlock(depth, map, global, rotVec, precise);
    }
}
record struct PickerResult(MapBase Map, Vector3 Global, Rectangle Rect, Vector2 Point, bool Behind);
